using SimpleGame.Core.Services;

namespace SimpleGame.Core.MVP
{
    public class SamplePresenter : Presenter<ISampleView>
    {
        private readonly GameService _gameService;

        public SamplePresenter(ISampleView view, GameService gameService) : base(view)
        {
            _gameService = gameService;
        }

        public override void Initialize()
        {
            View.OnButtonClicked += HandleButtonClicked;
            View.UpdateLabel(_gameService.GetWelcomeMessage());
        }

        public override void Dispose()
        {
            View.OnButtonClicked -= HandleButtonClicked;
        }

        private void HandleButtonClicked()
        {
            View.UpdateLabel(_gameService.GetWelcomeMessage());
        }
    }
}
