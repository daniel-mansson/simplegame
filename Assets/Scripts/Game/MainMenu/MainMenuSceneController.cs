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
    /// SceneController for the MainMenu scene. Thin wiring board only:
    /// holds [SerializeField] refs, creates MainMenuFlowPresenter in Initialize(),
    /// and delegates RunAsync to it.
    ///
    /// All navigation and popup orchestration lives in <see cref="MainMenuFlowPresenter"/>.
    /// </summary>
    public class MainMenuSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private ConfirmDialogView _confirmDialogView;
        [SerializeField] private GameObject _homePanel;
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopView _shopView;

        private MainMenuFlowPresenter _flowPresenter;

        public void Initialize(UIFactory uiFactory, PopupManager<PopupId> popupManager,
                               MetaProgressionService metaProgression = null,
                               ProgressionService progression = null,
                               IGoldenPieceService goldenPieces = null,
                               ICoinsService coins = null,
                               IViewResolver viewResolver = null,
                               IAdService adService = null)
        {
            IInSceneScreenManager<MainMenuScreenId> screenManager = null;
            if (_homePanel != null && _shopPanel != null)
            {
                var panels = new Dictionary<MainMenuScreenId, GameObject>
                {
                    { MainMenuScreenId.Home, _homePanel },
                    { MainMenuScreenId.Shop, _shopPanel },
                };
                screenManager = new InSceneScreenManager<MainMenuScreenId>(panels);
                screenManager.ShowScreen(MainMenuScreenId.Home);
            }
            else
            {
                Debug.LogWarning("[MainMenuSceneController] _homePanel or _shopPanel not wired — in-scene screen switching disabled.");
            }

            _flowPresenter = new MainMenuFlowPresenter(
                mainMenuView:    _mainMenuView,
                confirmDialogView: _confirmDialogView,
                shopView:        _shopView,
                uiFactory:       uiFactory,
                popupManager:    popupManager,
                screenManager:   screenManager,
                metaProgression: metaProgression,
                progression:     progression,
                goldenPieces:    goldenPieces,
                coins:           coins,
                viewResolver:    viewResolver,
                adService:       adService);
        }

        // ── Test seams — delegate to flow presenter ────────────────────────

        public void SetViewsForTesting(IMainMenuView mainMenuView,
                                       IConfirmDialogView confirmDialogView = null,
                                       IObjectRestoredView objectRestoredView = null)
            => _flowPresenter.SetViewsForTesting(mainMenuView, confirmDialogView, objectRestoredView);

        // ── ISceneController ──────────────────────────────────────────────

        public UniTask<ScreenId> RunAsync(CancellationToken ct = default)
            => _flowPresenter.RunAsync(ct);
    }
}
