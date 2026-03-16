using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public class ConfirmDialogPresenter : Presenter<IConfirmDialogView>
    {
        private UniTaskCompletionSource<bool> _confirmTcs;

        public ConfirmDialogPresenter(IConfirmDialogView view) : base(view) { }

        public override void Initialize()
        {
            View.OnConfirmClicked += HandleConfirm;
            View.OnCancelClicked += HandleCancel;
            View.UpdateMessage("Are you sure?");
        }

        public override void Dispose()
        {
            View.OnConfirmClicked -= HandleConfirm;
            View.OnCancelClicked -= HandleCancel;
            _confirmTcs?.TrySetCanceled();
            _confirmTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves true when the user confirms, false when they cancel.
        /// Each call resets the completion source — any previous pending task is cancelled.
        /// </summary>
        public UniTask<bool> WaitForConfirmation()
        {
            _confirmTcs?.TrySetCanceled();
            _confirmTcs = new UniTaskCompletionSource<bool>();
            return _confirmTcs.Task;
        }

        private void HandleConfirm() => _confirmTcs?.TrySetResult(true);
        private void HandleCancel() => _confirmTcs?.TrySetResult(false);
    }
}
