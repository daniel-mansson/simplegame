using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace SimpleGame.Game.Services
{
    /// <summary>
    /// Real in-app purchase service backed by Unity Purchasing and PlayFab receipt validation.
    ///
    /// Flow:
    ///   1. InitializeAsync() — registers products with Unity Purchasing, waits for store init.
    ///   2. BuyAsync(productId) — initiates store purchase via IStoreController.
    ///   3. ProcessPurchase callback — extracts receipt, calls PlayFab validate endpoint.
    ///   4. On PlayFab success — Earn + Save coins, call ConfirmPendingPurchase, resolve result.
    ///   5. On any failure — resolve with appropriate IAPOutcome, do NOT ConfirmPendingPurchase.
    ///
    /// Threading: All Unity Purchasing callbacks fire on the main thread. UniTask TCS bridges
    /// are used for both the init wait and the per-purchase wait (D084 pattern).
    ///
    /// Dependencies injected via constructor:
    ///   IAPProductCatalog — product definitions (IDs + coin amounts)
    ///   ICoinsService — coin grant on success
    ///   IPlayFabAuthService — login guard before PlayFab calls
    /// </summary>
    public class UnityIAPService : IIAPService, IDetailedStoreListener
    {
        private readonly IAPProductCatalog _catalog;
        private readonly ICoinsService _coins;
        private readonly IPlayFabAuthService _auth;

        private IStoreController _controller;
        private IExtensionProvider _extensions;

        private UniTaskCompletionSource _initTcs;
        private bool _initFailed;

        // Per-purchase state — only one purchase in flight at a time.
        private UniTaskCompletionSource<IAPResult> _purchaseTcs;
        private Product _pendingProduct;

        public bool IsInitialized => _controller != null;

        public UnityIAPService(IAPProductCatalog catalog, ICoinsService coins, IPlayFabAuthService auth)
        {
            _catalog = catalog;
            _coins = coins;
            _auth = auth;
        }

        // -----------------------------------------------------------------------
        // IIAPService
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public async UniTask InitializeAsync()
        {
            if (IsInitialized) return;
            if (_initTcs != null) { await _initTcs.Task; return; }

            _initTcs = new UniTaskCompletionSource();

            var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
            // StandardUser FakeStore: shows a Buy/Cancel dialog per purchase only — no dialog at init.
            // Receipt will be "ThisIsFakeReceiptData" so PlayFab validation returns Invalid Receipt.
            // The ValidationFailed path is exercised; coin grant is skipped (expected in Editor).
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
            var builder = ConfigurationBuilder.Instance(module);

            if (_catalog?.Products != null)
            {
                foreach (var def in _catalog.Products)
                {
                    if (!string.IsNullOrEmpty(def?.ProductId))
                        builder.AddProduct(def.ProductId, ProductType.Consumable);
                }
            }

            UnityPurchasing.Initialize(this, builder);
            await _initTcs.Task;

            if (_initFailed)
                Debug.LogWarning("[UnityIAPService] Initialization failed — purchases unavailable.");
            else
                Debug.Log("[UnityIAPService] Initialized. Products loaded.");
        }

        /// <inheritdoc/>
        public async UniTask<IAPResult> BuyAsync(string productId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[UnityIAPService] BuyAsync called before initialization.");
                return IAPResult.Failed(IAPOutcome.PaymentFailed);
            }

            if (_purchaseTcs != null)
            {
                Debug.LogWarning("[UnityIAPService] A purchase is already in progress.");
                return IAPResult.Failed(IAPOutcome.PaymentFailed);
            }

            var product = _controller.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogWarning($"[UnityIAPService] Product not available: {productId}");
                return IAPResult.Failed(IAPOutcome.PaymentFailed);
            }

            _purchaseTcs = new UniTaskCompletionSource<IAPResult>();
            Debug.Log($"[UnityIAPService] BuyAsync: calling InitiatePurchase for {productId}");
            _controller.InitiatePurchase(product);
            Debug.Log($"[UnityIAPService] BuyAsync: InitiatePurchase returned. TCS status={_purchaseTcs?.UnsafeGetStatus().ToString() ?? "null"}");

            // Start a timeout that resolves the TCS if the store callback never fires.
            // We fire-and-forget it; OnPurchaseFailed/ValidateAndGrantAsync both call
            // TrySetResult so whichever arrives first wins — subsequent calls are no-ops.
#if UNITY_EDITOR
            const int timeoutMs = 30_000;
#else
            const int timeoutMs = 120_000;
#endif
            TimeoutPurchaseAsync(_purchaseTcs, timeoutMs).Forget();

            IAPResult result;
            try
            {
                Debug.Log($"[UnityIAPService] BuyAsync: about to await TCS (status={_purchaseTcs?.UnsafeGetStatus().ToString() ?? "null"})");
                result = await _purchaseTcs.Task;
                Debug.Log($"[UnityIAPService] BuyAsync: await returned. result={result.Outcome}");
            }
            finally
            {
                _purchaseTcs = null;
            }
            return result;
        }

        private static async UniTaskVoid TimeoutPurchaseAsync(
            UniTaskCompletionSource<IAPResult> tcs, int delayMs)
        {
            await UniTask.Delay(delayMs);
            if (tcs.TrySetResult(IAPResult.Failed(IAPOutcome.PaymentFailed)))
                Debug.LogWarning("[UnityIAPService] BuyAsync timed out — store callback never arrived. Is UGS initialised?");
        }

        // -----------------------------------------------------------------------
        // IDetailedStoreListener — Unity Purchasing callbacks (main thread)
        // -----------------------------------------------------------------------

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _initTcs?.TrySetResult();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[UnityIAPService] OnInitializeFailed: {error}");
            _initFailed = true;
            _initTcs?.TrySetResult();
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[UnityIAPService] OnInitializeFailed: {error} — {message}");
            _initFailed = true;
            _initTcs?.TrySetResult();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            _pendingProduct = args.purchasedProduct;
            Debug.Log($"[UnityIAPService] ProcessPurchase: {_pendingProduct.definition.id}");

            // Validate asynchronously then confirm. Return Pending so Unity holds the transaction.
            ValidateAndGrantAsync(_pendingProduct).Forget();
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var reason = failureDescription?.reason ?? PurchaseFailureReason.Unknown;
            var tcsState = _purchaseTcs == null ? "null" : _purchaseTcs.UnsafeGetStatus().ToString();
            Debug.LogWarning($"[UnityIAPService] OnPurchaseFailed(desc): {product?.definition.id} — {reason} | tcs={tcsState}");

            var outcome = reason == PurchaseFailureReason.UserCancelled
                ? IAPOutcome.Cancelled
                : IAPOutcome.PaymentFailed;

            var set = _purchaseTcs?.TrySetResult(IAPResult.Failed(outcome));
            Debug.LogWarning($"[UnityIAPService] OnPurchaseFailed(desc): TrySetResult={set}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var tcsState = _purchaseTcs == null ? "null" : _purchaseTcs.UnsafeGetStatus().ToString();
            Debug.LogWarning($"[UnityIAPService] OnPurchaseFailed(reason): {product?.definition.id} — {failureReason} | tcs={tcsState}");

            var outcome = failureReason == PurchaseFailureReason.UserCancelled
                ? IAPOutcome.Cancelled
                : IAPOutcome.PaymentFailed;

            var set = _purchaseTcs?.TrySetResult(IAPResult.Failed(outcome));
            Debug.LogWarning($"[UnityIAPService] OnPurchaseFailed(reason): TrySetResult={set}");
        }

        // -----------------------------------------------------------------------
        // PlayFab receipt validation
        // -----------------------------------------------------------------------

        private async UniTaskVoid ValidateAndGrantAsync(Product product)
        {
            if (!_auth.IsLoggedIn)
            {
                Debug.LogWarning("[UnityIAPService] Not logged in — skipping PlayFab validation. No coins granted.");
                _purchaseTcs?.TrySetResult(IAPResult.Failed(IAPOutcome.ValidationFailed));
                return;
            }

            var productId = product.definition.id;
            var coinsAmount = GetCoinsForProduct(productId);

            bool validated = false;

#if UNITY_EDITOR
            // FakeStore produces "ThisIsFakeReceiptData" — PlayFab would reject it.
            // Auto-validate in Editor so the full post-validation flow (coin grant,
            // ConfirmPendingPurchase, UI update) can be exercised without a real device.
            Debug.Log("[UnityIAPService] Editor: skipping PlayFab receipt validation (FakeStore receipt).");
            validated = true;
            await UniTask.CompletedTask; // keeps async signature valid in Editor-only build
#elif UNITY_IOS
            validated = await ValidateIOSAsync(product);
#elif UNITY_ANDROID
            validated = await ValidateGooglePlayAsync(product);
#else
            // Unsupported platform — treat as validated for development convenience.
            Debug.Log("[UnityIAPService] Unsupported platform: skipping PlayFab validation.");
            validated = true;
#endif

            if (validated)
            {
                _controller.ConfirmPendingPurchase(product);

                if (coinsAmount > 0 && _coins != null)
                {
                    _coins.Earn(coinsAmount);
                    _coins.Save();
                    Debug.Log($"[UnityIAPService] Granted {coinsAmount} coins for {productId}.");
                }

                _purchaseTcs?.TrySetResult(IAPResult.Succeeded(coinsAmount));
            }
            else
            {
                // Do NOT ConfirmPendingPurchase — Unity will re-deliver on next launch.
                Debug.LogWarning($"[UnityIAPService] PlayFab validation failed for {productId}. No coins granted.");
                _purchaseTcs?.TrySetResult(IAPResult.Failed(IAPOutcome.ValidationFailed));
            }
        }

        private async UniTask<bool> ValidateIOSAsync(Product product)
        {
            // Unity Purchasing surfaces the raw receipt as product.receipt (JSON wrapper).
            // The JWS receipt data is in the "Payload" field of that JSON.
            string jwsReceiptData = ExtractIOSPayload(product.receipt);
            if (string.IsNullOrEmpty(jwsReceiptData))
            {
                Debug.LogWarning("[UnityIAPService] iOS receipt payload empty.");
                return false;
            }

            var priceInCents = Mathf.RoundToInt((float)(product.metadata.localizedPrice * 100));
            var currencyCode = product.metadata.isoCurrencyCode;

            var request = new ValidateIOSReceiptRequest
            {
                JwsReceiptData  = jwsReceiptData,
                CurrencyCode    = currencyCode,
                PurchasePrice   = priceInCents,
            };

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.ValidateIOSReceipt(
                request,
                result =>
                {
                    Debug.Log("[UnityIAPService] iOS receipt validated by PlayFab.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"[UnityIAPService] iOS PlayFab validation error: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }

        private async UniTask<bool> ValidateGooglePlayAsync(Product product)
        {
            // Unity Purchasing surfaces the Google Play receipt as product.receipt (JSON wrapper).
            // PayloadJSON contains the ReceiptJson and Signature fields.
            if (!ExtractGooglePlayReceipt(product.receipt, out var receiptJson, out var signature))
            {
                Debug.LogWarning("[UnityIAPService] Google Play receipt extraction failed.");
                return false;
            }

            var priceInMicros = (uint)Mathf.RoundToInt((float)(product.metadata.localizedPrice * 100));
            var currencyCode = product.metadata.isoCurrencyCode;

            var request = new ValidateGooglePlayPurchaseRequest
            {
                ReceiptJson   = receiptJson,
                Signature     = signature,
                CurrencyCode  = currencyCode,
                PurchasePrice = priceInMicros,
            };

            var tcs = new UniTaskCompletionSource<bool>();
            PlayFabClientAPI.ValidateGooglePlayPurchase(
                request,
                result =>
                {
                    Debug.Log("[UnityIAPService] Google Play receipt validated by PlayFab.");
                    tcs.TrySetResult(true);
                },
                error =>
                {
                    Debug.LogError($"[UnityIAPService] Google Play PlayFab validation error: {error.ErrorMessage}");
                    tcs.TrySetResult(false);
                });

            return await tcs.Task;
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private int GetCoinsForProduct(string productId)
        {
            if (_catalog?.Products == null) return 0;
            foreach (var def in _catalog.Products)
            {
                if (def?.ProductId == productId)
                    return def.CoinsAmount;
            }
            Debug.LogWarning($"[UnityIAPService] No catalog entry for product: {productId}");
            return 0;
        }

        /// <summary>
        /// Extracts the JWS payload from Unity Purchasing's receipt JSON wrapper.
        /// Unity wraps the raw store receipt in: {"Store":"AppleAppStore","TransactionID":"...","Payload":"<base64>"}
        /// </summary>
        private static string ExtractIOSPayload(string unityReceipt)
        {
            if (string.IsNullOrEmpty(unityReceipt)) return null;
            try
            {
                // Simple JSON field extraction — avoids adding a JSON library dependency.
                const string key = "\"Payload\":\"";
                int start = unityReceipt.IndexOf(key, StringComparison.Ordinal);
                if (start < 0) return null;
                start += key.Length;
                int end = unityReceipt.IndexOf('"', start);
                return end > start ? unityReceipt.Substring(start, end - start) : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityIAPService] iOS receipt parse error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts ReceiptJson and Signature from Unity Purchasing's Google Play receipt wrapper.
        /// Unity wraps the payload in: {"Store":"GooglePlay","TransactionID":"...","Payload":"{\"json\":\"...\",\"signature\":\"...\"}"}
        /// </summary>
        private static bool ExtractGooglePlayReceipt(string unityReceipt, out string receiptJson, out string signature)
        {
            receiptJson = null;
            signature = null;
            if (string.IsNullOrEmpty(unityReceipt)) return false;
            try
            {
                const string payloadKey = "\"Payload\":\"";
                int pStart = unityReceipt.IndexOf(payloadKey, StringComparison.Ordinal);
                if (pStart < 0) return false;
                pStart += payloadKey.Length;

                // The payload is a JSON string — find its closing quote (escaped).
                // Replace escape sequences to get the inner JSON.
                int pEnd = unityReceipt.IndexOf("\"}}", pStart, StringComparison.Ordinal);
                if (pEnd < 0) pEnd = unityReceipt.IndexOf("\"}", pStart, StringComparison.Ordinal);
                if (pEnd < 0) return false;

                string payload = unityReceipt.Substring(pStart, pEnd - pStart)
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\");

                // Extract "json" field.
                const string jsonKey = "\"json\":\"";
                int jStart = payload.IndexOf(jsonKey, StringComparison.Ordinal);
                if (jStart < 0) return false;
                jStart += jsonKey.Length;
                int jEnd = payload.IndexOf("\",", jStart, StringComparison.Ordinal);
                if (jEnd < 0) jEnd = payload.IndexOf("\"}", jStart, StringComparison.Ordinal);
                if (jEnd < 0) return false;
                receiptJson = payload.Substring(jStart, jEnd - jStart).Replace("\\\"", "\"");

                // Extract "signature" field.
                const string sigKey = "\"signature\":\"";
                int sStart = payload.IndexOf(sigKey, StringComparison.Ordinal);
                if (sStart < 0) return false;
                sStart += sigKey.Length;
                int sEnd = payload.IndexOf("\"", sStart, StringComparison.Ordinal);
                if (sEnd < 0) return false;
                signature = payload.Substring(sStart, sEnd - sStart);

                return !string.IsNullOrEmpty(receiptJson) && !string.IsNullOrEmpty(signature);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnityIAPService] Google Play receipt parse error: {ex.Message}");
                return false;
            }
        }
    }
}
