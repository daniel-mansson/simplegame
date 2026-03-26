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
    /// Owns the MainMenu scene's navigation loop: reacts to <see cref="MainMenuAction"/>
    /// values from <see cref="MainMenuPresenter"/>, orchestrates popup flows
    /// (ObjectRestored, ConfirmDialog, Shop), and handles environment progression.
    ///
    /// Extracted from <see cref="MainMenuSceneController"/> to keep the controller
    /// to a thin wiring board.
    /// </summary>
    public class MainMenuFlowPresenter
    {
        // ── Views ─────────────────────────────────────────────────────────
        private readonly IMainMenuView _mainMenuView;
        private readonly IConfirmDialogView _confirmDialogView;
        private readonly IShopView _shopView;
        private readonly IViewResolver _viewResolver;

        // ── Services ──────────────────────────────────────────────────────
        private readonly UIFactory _uiFactory;
        private readonly PopupManager<PopupId> _popupManager;
        private readonly MetaProgressionService _metaProgression;
        private readonly ProgressionService _progression;
        private readonly IGoldenPieceService _goldenPieces;
        private readonly ICoinsService _coins;
        private readonly IAdService _adService;

        // ── In-scene screen manager ───────────────────────────────────────
        private readonly IInSceneScreenManager<MainMenuScreenId> _screenManager;

        // ── Test overrides ────────────────────────────────────────────────
        private IMainMenuView _viewOverride;
        private IConfirmDialogView _confirmDialogViewOverride;
        private IObjectRestoredView _objectRestoredViewOverride;

        private IMainMenuView ActiveMainMenuView => _viewOverride ?? _mainMenuView;

        private IConfirmDialogView ActiveConfirmDialogView
        {
            get
            {
                if (_confirmDialogViewOverride != null) return _confirmDialogViewOverride;
                if (_confirmDialogView != null) return _confirmDialogView;
                var found = _viewResolver?.Get<IConfirmDialogView>();
                if (found == null)
                    Debug.LogError("[MainMenuFlowPresenter] ConfirmDialogView not found.");
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
                    Debug.LogError("[MainMenuFlowPresenter] ObjectRestoredView not found.");
                return found;
            }
        }

        // ── Constructor ───────────────────────────────────────────────────

        public MainMenuFlowPresenter(
            IMainMenuView mainMenuView,
            IConfirmDialogView confirmDialogView,
            IShopView shopView,
            UIFactory uiFactory,
            PopupManager<PopupId> popupManager,
            IInSceneScreenManager<MainMenuScreenId> screenManager = null,
            MetaProgressionService metaProgression = null,
            ProgressionService progression = null,
            IGoldenPieceService goldenPieces = null,
            ICoinsService coins = null,
            IViewResolver viewResolver = null,
            IAdService adService = null)
        {
            _mainMenuView    = mainMenuView;
            _confirmDialogView = confirmDialogView;
            _shopView        = shopView;
            _uiFactory       = uiFactory;
            _popupManager    = popupManager;
            _screenManager   = screenManager;
            _metaProgression = metaProgression;
            _progression     = progression;
            _goldenPieces    = goldenPieces;
            _coins           = coins;
            _viewResolver    = viewResolver;
            _adService       = adService;
        }

        // ── Test seam API ─────────────────────────────────────────────────

        public void SetViewsForTesting(IMainMenuView mainMenuView,
                                       IConfirmDialogView confirmDialogView = null,
                                       IObjectRestoredView objectRestoredView = null)
        {
            _viewOverride                = mainMenuView;
            _confirmDialogViewOverride   = confirmDialogView;
            _objectRestoredViewOverride  = objectRestoredView;
        }

        // ── Main loop ─────────────────────────────────────────────────────

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            while (true)
            {
                var (currentEnv, envIndex) = GetCurrentEnvironment();
                bool hasNext = HasNextEnvironment(envIndex);

                var presenter = _uiFactory.CreateMainMenuPresenter(ActiveMainMenuView, currentEnv, hasNext);
                presenter.Initialize();

                if (_adService != null)
                    ActiveMainMenuView.SetDebugAdsVisible(true);

                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();
                        var action = await presenter.WaitForAction();

                        if (action == MainMenuAction.Settings)  return ScreenId.Settings;
                        if (action == MainMenuAction.Play)      return ScreenId.InGame;

                        if (action == MainMenuAction.ObjectRestored)
                        {
                            await HandleObjectRestoredPopupAsync(presenter.LastRestoredObjectName, ct);
                            presenter.RefreshView();
                        }
                        else if (action == MainMenuAction.ResetProgress)
                        {
                            var confirmed = await ShowConfirmDialogAsync("Reset all progress?", ct);
                            if (confirmed)
                            {
                                _metaProgression?.ResetAll();
                                _goldenPieces?.ResetAll();
                                _progression?.ResetLevel();
                                break; // recreate presenter with fresh state
                            }
                            presenter.RefreshView();
                        }
                        else if (action == MainMenuAction.NextEnvironment)
                        {
                            break; // recreate presenter with next env
                        }
                        else if (action == MainMenuAction.OpenShop)
                        {
                            _screenManager?.ShowScreen(MainMenuScreenId.Shop);
                            await HandleShopScreenAsync(presenter, ct);
                            _screenManager?.GoBack();
                        }
                        else if (action == MainMenuAction.DebugShowRewarded)
                        {
                            await HandleDebugRewardedAsync(ct);
                        }
                        else if (action == MainMenuAction.DebugShowInterstitial)
                        {
                            await HandleDebugInterstitialAsync(ct);
                        }
                        else if (action == MainMenuAction.DebugShowBanner)
                        {
                            ActiveMainMenuView.UpdateDebugStatus("Banner: not implemented");
                        }
                    }
                }
                finally
                {
                    presenter.Dispose();
                }
            }
        }

        // ── Popup helpers ─────────────────────────────────────────────────

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
            finally { presenter.Dispose(); }
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
            finally { presenter.Dispose(); }
        }

        private async UniTask HandleShopScreenAsync(MainMenuPresenter homePresenter, CancellationToken ct)
        {
            var shopView = _shopView as IShopView;
            if (shopView == null)
            {
                await homePresenter.WaitForCloseShopAsync();
                return;
            }
            var shopPresenter = _uiFactory.CreateShopPresenter(shopView);
            shopPresenter.Initialize();
            try
            {
                await UniTask.WhenAny(
                    shopPresenter.WaitForResult(),
                    homePresenter.WaitForCloseShopAsync()
                );
            }
            finally { shopPresenter.Dispose(); }
        }

        // ── Environment helpers ───────────────────────────────────────────

        private (EnvironmentData env, int index) GetCurrentEnvironment()
        {
            if (_metaProgression?.WorldData?.environments == null
                || _metaProgression.WorldData.environments.Length == 0)
            {
                Debug.LogWarning("[MainMenuFlowPresenter] No world data available.");
                return (null, -1);
            }
            var envs = _metaProgression.WorldData.environments;
            for (int i = 0; i < envs.Length; i++)
                if (!_metaProgression.IsEnvironmentComplete(envs[i]))
                    return (envs[i], i);
            return (envs[envs.Length - 1], envs.Length - 1);
        }

        private bool HasNextEnvironment(int currentIndex)
        {
            if (_metaProgression?.WorldData?.environments == null) return false;
            return currentIndex >= 0 && currentIndex < _metaProgression.WorldData.environments.Length - 1;
        }

        // ── Debug ad helpers ──────────────────────────────────────────────

        private async UniTask HandleDebugRewardedAsync(CancellationToken ct)
        {
            if (_adService == null) { ActiveMainMenuView.UpdateDebugStatus("No ad service"); return; }
            ActiveMainMenuView.UpdateDebugStatus("Loading rewarded…");
            if (!_adService.IsRewardedLoaded) { _adService.LoadRewarded(); await UniTask.Yield(ct); }
            if (!_adService.IsRewardedLoaded) { ActiveMainMenuView.UpdateDebugStatus("Rewarded: not loaded"); return; }
            ActiveMainMenuView.UpdateDebugStatus("Showing rewarded…");
            var result = await _adService.ShowRewardedAsync(ct);
            ActiveMainMenuView.UpdateDebugStatus($"Rewarded: {result}");
        }

        private async UniTask HandleDebugInterstitialAsync(CancellationToken ct)
        {
            if (_adService == null) { ActiveMainMenuView.UpdateDebugStatus("No ad service"); return; }
            ActiveMainMenuView.UpdateDebugStatus("Loading interstitial…");
            if (!_adService.IsInterstitialLoaded) { _adService.LoadInterstitial(); await UniTask.Yield(ct); }
            if (!_adService.IsInterstitialLoaded) { ActiveMainMenuView.UpdateDebugStatus("Interstitial: not loaded"); return; }
            ActiveMainMenuView.UpdateDebugStatus("Showing interstitial…");
            var result = await _adService.ShowInterstitialAsync(ct);
            ActiveMainMenuView.UpdateDebugStatus($"Interstitial: {result}");
        }
    }
}
