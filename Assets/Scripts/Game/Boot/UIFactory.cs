using SimpleGame.Core.PopupManagement;
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
        private readonly IIAPService _iap;
        private readonly IInputBlocker _inputBlocker;

        public UIFactory(GameService gameService, ProgressionService progression,
                         GameSessionService session, IHeartService hearts = null,
                         MetaProgressionService metaProgression = null,
                         IGoldenPieceService goldenPieces = null,
                         ICoinsService coins = null,
                         IIAPService iap = null,
                         IAPProductCatalog iapCatalog = null,   // kept for call-site compat; not used (Products come from IIAPService)
                         IInputBlocker inputBlocker = null)
        {
            _gameService = gameService;
            _progression = progression;
            _session = session;
            _hearts = hearts;
            _metaProgression = metaProgression;
            _goldenPieces = goldenPieces;
            _coins = coins;
            _iap = iap;
            _inputBlocker = inputBlocker;
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
            var iap = _iap ?? new NullIAPService();
            return new ShopPresenter(view, iap, _coins, _inputBlocker);
        }

        /// <summary>
        /// Creates an IAPPurchasePresenter for the given product index.
        /// Reads from <see cref="IIAPService.Products"/> (runtime-merged PlayFab + local data).
        /// Falls back to null product (presenter shows "Coin Pack / unavailable") if out of range.
        /// </summary>
        public IAPPurchasePresenter CreateIAPPurchasePresenter(IIAPPurchaseView view, int productIndex = 0)
        {
            var iap = _iap ?? new NullIAPService();
            IAPProductInfo product = null;
            if (productIndex >= 0 && productIndex < iap.Products.Count)
                product = iap.Products[productIndex];
            return new IAPPurchasePresenter(view, iap, product, _coins, _inputBlocker);
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
