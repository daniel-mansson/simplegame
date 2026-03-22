using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Core.ScreenManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using UnityEngine;
namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// SceneController for the MainMenu scene (now the meta world main screen).
    /// Shows the current environment with restorable objects, golden piece
    /// balance, and play button. Handles object restoration inline with
    /// ObjectRestored celebration popup. Returns ScreenId for navigation.
    ///
    /// Uses <see cref="InSceneScreenManager{MainMenuScreenId}"/> to switch between
    /// the Home panel and the Shop panel within the same scene (no scene load).
    ///
    /// ResetProgress: shows confirm dialog, then resets all services and recreates presenter.
    /// NextEnvironment: advances to the next environment and recreates presenter.
    /// </summary>
    public class MainMenuSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private ConfirmDialogView _confirmDialogView;
        [SerializeField] private GameObject _homePanel;
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopView _shopView;

        private IViewResolver _viewResolver;
        private IMainMenuView _mainMenuViewOverride;
        private IConfirmDialogView _confirmDialogViewOverride;
        private IObjectRestoredView _objectRestoredViewOverride;

        private IInSceneScreenManager<MainMenuScreenId> _screenManager;
        private bool _useInSceneScreenManager;

        private IMainMenuView ActiveMainMenuView => _mainMenuViewOverride != null ? _mainMenuViewOverride : _mainMenuView;

        private IConfirmDialogView ActiveConfirmDialogView
        {
            get
            {
                if (_confirmDialogViewOverride != null) return _confirmDialogViewOverride;
                if (_confirmDialogView != null) return _confirmDialogView;
                var found = _viewResolver?.Get<IConfirmDialogView>();
                if (found == null)
                    Debug.LogError("[MainMenuSceneController] ConfirmDialogView not found in any loaded scene.");
                return found;
            }
        }

        private IObjectRestoredView ActiveObjectRestoredView
        {
            get
            {
                if (_objectRestoredViewOverride != null) return _objectRestoredViewOverride;
                var found = _viewResolver?.Get<IObjectRestoredView>();
                if (found == null)
                    Debug.LogError("[MainMenuSceneController] ObjectRestoredView not found in any loaded scene.");
                return found;
            }
        }

        private UIFactory _uiFactory;
        private PopupManager<PopupId> _popupManager;
        private MetaProgressionService _metaProgression;
        private ProgressionService _progression;
        private IGoldenPieceService _goldenPieces;
        private ICoinsService _coins;
        private IAdService _adService;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, PopupManager<PopupId> popupManager,
                               MetaProgressionService metaProgression = null,
                               ProgressionService progression = null,
                               IGoldenPieceService goldenPieces = null,
                               ICoinsService coins = null,
                               IViewResolver viewResolver = null,
                               IAdService adService = null)
        {
            _uiFactory = uiFactory;
            _popupManager = popupManager;
            _metaProgression = metaProgression;
            _progression = progression;
            _goldenPieces = goldenPieces;
            _coins = coins;
            _viewResolver = viewResolver;
            _adService = adService;

            // Build InSceneScreenManager if panels are wired
            if (_homePanel != null && _shopPanel != null)
            {
                var panels = new Dictionary<MainMenuScreenId, GameObject>
                {
                    { MainMenuScreenId.Home, _homePanel },
                    { MainMenuScreenId.Shop, _shopPanel },
                };
                _screenManager = new InSceneScreenManager<MainMenuScreenId>(panels);
                _useInSceneScreenManager = true;
                _screenManager.ShowScreen(MainMenuScreenId.Home);
            }
            else
            {
                _useInSceneScreenManager = false;
                Debug.LogWarning("[MainMenuSceneController] _homePanel or _shopPanel not wired — in-scene screen switching disabled.");
            }
        }

        /// <summary>
        /// For editor / test use: supply mock views that override the serialized fields.
        /// </summary>
        public void SetViewsForTesting(IMainMenuView mainMenuView,
                                        IConfirmDialogView confirmDialogView = null,
                                        IObjectRestoredView objectRestoredView = null)
        {
            _mainMenuViewOverride = mainMenuView;
            _confirmDialogViewOverride = confirmDialogView;
            _objectRestoredViewOverride = objectRestoredView;
        }

        /// <summary>
        /// For editor / test use: inject a pre-built screen manager.
        /// </summary>
        public void SetScreenManagerForTesting(IInSceneScreenManager<MainMenuScreenId> screenManager)
        {
            _screenManager = screenManager;
            _useInSceneScreenManager = true;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            while (true)
            {
                // Determine current environment each iteration (may change after reset/next)
                var (currentEnv, envIndex) = GetCurrentEnvironment();
                bool hasNext = HasNextEnvironment(envIndex);

                var presenter = _uiFactory.CreateMainMenuPresenter(ActiveMainMenuView, currentEnv, hasNext);
                presenter.Initialize();

                // Show debug ad buttons when an ad service is available
                if (_adService != null)
                    ActiveMainMenuView.SetDebugAdsVisible(true);

                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        var action = await presenter.WaitForAction();

                        if (action == MainMenuAction.Settings)
                            return ScreenId.Settings;

                        if (action == MainMenuAction.Play)
                            return ScreenId.InGame;

                        if (action == MainMenuAction.ObjectRestored)
                        {
                            await HandleObjectRestoredPopupAsync(presenter.LastRestoredObjectName, ct);
                            presenter.RefreshView();
                        }

                        if (action == MainMenuAction.ResetProgress)
                        {
                            var confirmed = await ShowConfirmDialogAsync("Reset all progress?", ct);
                            if (confirmed)
                            {
                                _metaProgression?.ResetAll();
                                _goldenPieces?.ResetAll();
                                _progression?.ResetLevel();
                                Debug.Log("[MainMenuSceneController] All progress reset.");
                                break; // break inner loop → recreate presenter with fresh state
                            }
                            presenter.RefreshView();
                        }

                        if (action == MainMenuAction.NextEnvironment)
                        {
                            break; // break inner loop → recreate presenter with next env
                        }

                        if (action == MainMenuAction.OpenShop)
                        {
                            if (_useInSceneScreenManager)
                                _screenManager.ShowScreen(MainMenuScreenId.Shop);
                            await HandleShopScreenAsync(presenter, ct);
                            if (_useInSceneScreenManager)
                                _screenManager.GoBack();
                        }

                        if (action == MainMenuAction.DebugShowRewarded)
                        {
                            await HandleDebugRewardedAsync(ct);
                        }

                        if (action == MainMenuAction.DebugShowInterstitial)
                        {
                            await HandleDebugInterstitialAsync(ct);
                        }

                        if (action == MainMenuAction.DebugShowBanner)
                        {
                            HandleDebugBanner();
                        }
                    }
                }
                finally
                {
                    presenter.Dispose();
                }
            }
        }

        private async UniTask HandleShopScreenAsync(MainMenuPresenter homePresenter, CancellationToken ct)
        {
            // ShopView lives in the ShopPanel — resolved via direct SerializeField, not viewResolver
            var shopView = _shopView as IShopView;
            if (shopView == null)
            {
                Debug.LogWarning("[MainMenuSceneController] ShopView not wired — waiting for Back button.");
                await homePresenter.WaitForCloseShopAsync();
                return;
            }

            var shopPresenter = _uiFactory.CreateShopPresenter(shopView);
            shopPresenter.Initialize();
            try
            {
                // WaitForResult resolves on cancel OR purchase.
                // The Back button in the ShopPanel fires OnShopBackClicked → MainMenuPresenter
                // sets action to CloseShop, BUT we're not looping on WaitForAction here.
                // So wire: also resolve when CloseShop fires from homePresenter.
                //
                // Strategy: race WaitForResult (cancel button on ShopView) vs
                // WaitForCloseShop (back button via homePresenter).
                await UniTask.WhenAny(
                    shopPresenter.WaitForResult(),
                    homePresenter.WaitForCloseShopAsync()
                );
            }
            finally
            {
                shopPresenter.Dispose();
            }
        }

        private (EnvironmentData env, int index) GetCurrentEnvironment()        {
            if (_metaProgression == null || _metaProgression.WorldData == null
                || _metaProgression.WorldData.environments == null
                || _metaProgression.WorldData.environments.Length == 0)
            {
                Debug.LogWarning("[MainMenuSceneController] No world data available.");
                return (null, -1);
            }

            var envs = _metaProgression.WorldData.environments;
            for (int i = 0; i < envs.Length; i++)
            {
                if (!_metaProgression.IsEnvironmentComplete(envs[i]))
                    return (envs[i], i);
            }
            return (envs[envs.Length - 1], envs.Length - 1);
        }

        private bool HasNextEnvironment(int currentIndex)
        {
            if (_metaProgression == null || _metaProgression.WorldData == null
                || _metaProgression.WorldData.environments == null)
                return false;

            return currentIndex >= 0 && currentIndex < _metaProgression.WorldData.environments.Length - 1;
        }

        private async UniTask HandleObjectRestoredPopupAsync(string objectName, CancellationToken ct)
        {
            var view = ActiveObjectRestoredView;
            if (view == null) return;

            var presenter = _uiFactory.CreateObjectRestoredPresenter(view);
            presenter.Initialize(objectName);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.ObjectRestored, ct);
                await presenter.WaitForContinue();
                await _popupManager.DismissPopupAsync(ct);
            }
            finally
            {
                presenter.Dispose();
            }
        }

        private async UniTask<bool> ShowConfirmDialogAsync(string message, CancellationToken ct)
        {
            var view = ActiveConfirmDialogView;
            if (view == null) return false;

            var presenter = _uiFactory.CreateConfirmDialogPresenter(view);
            presenter.Initialize(message);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.ConfirmDialog, ct);
                var result = await presenter.WaitForConfirmation();
                await _popupManager.DismissPopupAsync(ct);
                return result;
            }
            finally
            {
                presenter.Dispose();
            }
        }

        private async UniTask HandleDebugRewardedAsync(CancellationToken ct)
        {
            if (_adService == null)
            {
                ActiveMainMenuView.UpdateDebugStatus("No ad service");
                return;
            }

            ActiveMainMenuView.UpdateDebugStatus("Loading rewarded…");
            Debug.Log("[MainMenuSceneController] Debug: showing rewarded ad.");

            if (!_adService.IsRewardedLoaded)
            {
                _adService.LoadRewarded();
                // Give a brief moment for the null/test service to respond
                await UniTask.Yield(ct);
            }

            if (!_adService.IsRewardedLoaded)
            {
                ActiveMainMenuView.UpdateDebugStatus("Rewarded: not loaded");
                Debug.Log("[MainMenuSceneController] Debug: rewarded ad not loaded.");
                return;
            }

            ActiveMainMenuView.UpdateDebugStatus("Showing rewarded…");
            var result = await _adService.ShowRewardedAsync(ct);
            var msg = $"Rewarded: {result}";
            ActiveMainMenuView.UpdateDebugStatus(msg);
            Debug.Log($"[MainMenuSceneController] Debug: {msg}");
        }

        private async UniTask HandleDebugInterstitialAsync(CancellationToken ct)
        {
            if (_adService == null)
            {
                ActiveMainMenuView.UpdateDebugStatus("No ad service");
                return;
            }

            ActiveMainMenuView.UpdateDebugStatus("Loading interstitial…");
            Debug.Log("[MainMenuSceneController] Debug: showing interstitial ad.");

            if (!_adService.IsInterstitialLoaded)
            {
                _adService.LoadInterstitial();
                await UniTask.Yield(ct);
            }

            if (!_adService.IsInterstitialLoaded)
            {
                ActiveMainMenuView.UpdateDebugStatus("Interstitial: not loaded");
                Debug.Log("[MainMenuSceneController] Debug: interstitial ad not loaded.");
                return;
            }

            ActiveMainMenuView.UpdateDebugStatus("Showing interstitial…");
            var result = await _adService.ShowInterstitialAsync(ct);
            var msg = $"Interstitial: {result}";
            ActiveMainMenuView.UpdateDebugStatus(msg);
            Debug.Log($"[MainMenuSceneController] Debug: {msg}");
        }

        private void HandleDebugBanner()
        {
            // Banner ads are typically persistent — toggle visibility.
            // The current IAdService doesn't have banner support yet,
            // so this is a placeholder that logs the intent.
            ActiveMainMenuView.UpdateDebugStatus("Banner: not implemented");
            Debug.Log("[MainMenuSceneController] Debug: banner ad requested — IAdService has no banner API yet.");
        }
    }
}
