using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// SceneController for the InGame scene. Thin wiring board only:
    /// holds [SerializeField] refs, creates InGameFlowPresenter in Initialize(),
    /// and delegates RunAsync to it.
    ///
    /// All game logic lives in <see cref="InGameFlowPresenter"/> (game loop, popups, retry).
    /// All 3D stage logic lives in <see cref="PuzzleStageController"/> (piece spawning, tray layout).
    /// </summary>
    public class InGameSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private InGameView _inGameView;
        [SerializeField] private PuzzleStageController _stage;

        [Header("Game Config")]
        [SerializeField] private int _defaultLevelId = 1;
        [SerializeField] private int _defaultTotalPieces = 10;
        [SerializeField] private int _goldenPiecesPerWin = 5;

        private InGameFlowPresenter _flowPresenter;
        private CancellationTokenSource _runCts;

        public void Initialize(UIFactory uiFactory, ProgressionService progression,
                               GameSessionService session, PopupManager<PopupId> popupManager,
                               IGoldenPieceService goldenPieces = null, IHeartService hearts = null,
                               ICoinsService coins = null, IViewResolver viewResolver = null,
                               ICurrencyOverlay overlay = null,
                               System.Func<UniTask> onSessionEnd = null,
                               IAnalyticsService analytics = null,
                               GameRemoteConfig? remoteConfig = null,
                               IAdService adService = null)
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = null;

            _stage?.SetContext(popupManager);

            // Resolve the CameraController from the main camera (may be null in tests).
            var cameraController = Camera.main?.GetComponent<CameraController>();

            _flowPresenter = new InGameFlowPresenter(
                serializedView:          _inGameView,
                stage:                   _stage,
                uiFactory:               uiFactory,
                progression:             progression,
                session:                 session,
                popupManager:            popupManager,
                goldenPieces:            goldenPieces,
                hearts:                  hearts,
                coins:                   coins,
                viewResolver:            viewResolver,
                overlay:                 overlay,
                onSessionEnd:            onSessionEnd,
                analytics:               analytics,
                adService:               adService,
                defaultLevelId:          _defaultLevelId,
                defaultTotalPieces:      _defaultTotalPieces,
                goldenPiecesPerWin:      _goldenPiecesPerWin,
                cameraController:        cameraController);

            if (remoteConfig.HasValue)
                _flowPresenter.ApplyRemoteConfig(remoteConfig.Value);
        }

        // ── Test seams — delegate to presenter ────────────────────────────

        public void SetViewsForTesting(IInGameView inGameView,
                                       ILevelCompleteView levelCompleteView = null,
                                       ILevelFailedView levelFailedView = null)
            => _flowPresenter.SetViewsForTesting(inGameView, levelCompleteView, levelFailedView);

        public void SetModelFactory(System.Func<SimpleGame.Puzzle.PuzzleModel> factory)
            => _flowPresenter.SetModelFactory(factory);

        public void SetWinPopupDelay(float seconds) => _flowPresenter.SetWinPopupDelay(seconds);

        public void SetDebugOverride(int rows, int cols, int slots)
            => _flowPresenter.SetDebugOverride(rows, cols, slots);

        public void ClearDebugOverride() => _flowPresenter.ClearDebugOverride();

        // ── ISceneController ──────────────────────────────────────────────

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            return await _flowPresenter.RunAsync(_runCts.Token);
        }

        // ── Play-from-editor bootstrap ────────────────────────────────────

        private void Start()
        {
            if (_flowPresenter != null) return;
            WaitForBootOrSelfBootstrap().Forget();
        }

        private async UniTaskVoid WaitForBootOrSelfBootstrap()
        {
            // If GameBootstrapper exists, it will call Initialize() + RunAsync() — never self-bootstrap.
            if (FindObjectOfType<Boot.GameBootstrapper>() != null)
                return;

            for (int i = 0; i < 10; i++)
            {
                await UniTask.DelayFrame(1, cancellationToken: destroyCancellationToken);
                if (_flowPresenter != null) return;
            }
            Debug.Log("[InGameSceneController] Play-from-editor: bootstrapping with stub services.");
            var session     = new GameSessionService();
            var hearts      = new HeartService();
            var progression = new ProgressionService();
            var goldenPieces = new NullGoldenPieceService();
            Initialize(new UIFactory(new GameService(), progression, session, hearts, null, goldenPieces, null),
                       progression, session, null, goldenPieces, hearts);
            RunAsync().Forget();
        }

        private sealed class NullGoldenPieceService : IGoldenPieceService
        {
            public int Balance => 0;
            public void Earn(int amount) { }
            public bool TrySpend(int amount) => false;
            public void Save() { }
            public void ResetAll() { }
        }
    }
}
