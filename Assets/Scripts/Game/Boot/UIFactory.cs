using SimpleGame.Game.InGame;
using SimpleGame.Game.MainMenu;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using SimpleGame.Game.Settings;
using SimpleGame.Puzzle;

namespace SimpleGame.Game.Boot
{
    public class UIFactory
    {
        private readonly GameService _gameService;
        private readonly ProgressionService _progression;
        private readonly GameSessionService _session;
        private readonly IHeartService _hearts;
        private int _initialHearts = 3;
        private readonly MetaProgressionService _metaProgression;
        private readonly IGoldenPieceService _goldenPieces;
        private readonly ICoinsService _coins;

        public UIFactory(GameService gameService, ProgressionService progression,
                         GameSessionService session, IHeartService hearts = null,
                         MetaProgressionService metaProgression = null,
                         IGoldenPieceService goldenPieces = null,
                         ICoinsService coins = null)
        {
            _gameService = gameService;
            _progression = progression;
            _session = session;
            _hearts = hearts;
            _metaProgression = metaProgression;
            _goldenPieces = goldenPieces;
            _coins = coins;
        }

        public MainMenuPresenter CreateMainMenuPresenter(IMainMenuView view, EnvironmentData currentEnvironment,
                                                          bool hasNextEnvironment = false)
        {
            return new MainMenuPresenter(view, _metaProgression, _goldenPieces, _progression, _session,
                                         currentEnvironment, hasNextEnvironment);
        }

        public SettingsPresenter CreateSettingsPresenter(ISettingsView view, IPlatformLinkService linkService = null)
        {
            return new SettingsPresenter(view, linkService);
        }

        public ConfirmDialogPresenter CreateConfirmDialogPresenter(IConfirmDialogView view)
        {
            return new ConfirmDialogPresenter(view);
        }

        public InGamePresenter CreateInGamePresenter(IInGameView view, PuzzleModel model)
        {
            return new InGamePresenter(view, _session, _hearts, model, _initialHearts);
        }

        /// <summary>Override initial heart count from remote config.</summary>
        public void SetInitialHearts(int count) => _initialHearts = count > 0 ? count : _initialHearts;

        public LevelCompletePresenter CreateLevelCompletePresenter(ILevelCompleteView view)
        {
            return new LevelCompletePresenter(view);
        }

        public LevelFailedPresenter CreateLevelFailedPresenter(ILevelFailedView view)
        {
            return new LevelFailedPresenter(view);
        }

        public ObjectRestoredPresenter CreateObjectRestoredPresenter(IObjectRestoredView view)
        {
            return new ObjectRestoredPresenter(view);
        }

        public ShopPresenter CreateShopPresenter(IShopView view)
        {
            return new ShopPresenter(view, _coins);
        }

        public PlatformLinkPresenter CreatePlatformLinkPresenter(IPlatformLinkView view, IPlatformLinkService linkService, IAnalyticsService analytics = null)
        {
            return new PlatformLinkPresenter(view, linkService, analytics);
        }

        public ConsentGatePresenter CreateConsentGatePresenter(IConsentGateView view)
        {
            return new ConsentGatePresenter(view);
        }
    }
}
