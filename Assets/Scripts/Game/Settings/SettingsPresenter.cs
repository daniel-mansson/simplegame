using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Settings
{
    public class SettingsPresenter : Presenter<ISettingsView>
    {
        private UniTaskCompletionSource _backTcs;

        public SettingsPresenter(ISettingsView view) : base(view) { }

        public override void Initialize()
        {
            View.OnBackClicked += HandleBackClicked;
            View.UpdateTitle("Settings");
        }

        public override void Dispose()
        {
            View.OnBackClicked -= HandleBackClicked;
            _backTcs?.TrySetCanceled();
            _backTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the user presses back.
        /// Each call resets the completion source — any previous pending task is cancelled.
        /// </summary>
        public UniTask WaitForBack()
        {
            _backTcs?.TrySetCanceled();
            _backTcs = new UniTaskCompletionSource();
            return _backTcs.Task;
        }

        private void HandleBackClicked() => _backTcs?.TrySetResult();
    }
}
