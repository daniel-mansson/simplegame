using System;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Settings
{
    public class SettingsPresenter : Presenter<ISettingsView>
    {
        private readonly Func<UniTask> _goBackCallback;

        public SettingsPresenter(ISettingsView view, Func<UniTask> goBackCallback) : base(view)
        {
            _goBackCallback = goBackCallback;
        }

        public override void Initialize()
        {
            View.OnBackClicked += HandleBackClicked;
            View.UpdateTitle("Settings");
        }

        public override void Dispose()
        {
            View.OnBackClicked -= HandleBackClicked;
        }

        private void HandleBackClicked()
        {
            _goBackCallback().Forget();
        }
    }
}
