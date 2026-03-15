using SimpleGame.Core.Services;

namespace SimpleGame.Core.MVP
{
    public class UIFactory
    {
        private readonly GameService _gameService;

        public UIFactory(GameService gameService)
        {
            _gameService = gameService;
        }

        public SamplePresenter CreateSamplePresenter(ISampleView view)
        {
            return new SamplePresenter(view, _gameService);
        }
    }
}
