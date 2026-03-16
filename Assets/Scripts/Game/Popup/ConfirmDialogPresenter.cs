using System;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    public class ConfirmDialogPresenter : Presenter<IConfirmDialogView>
    {
        private readonly Func<UniTask> _dismissCallback;

        public ConfirmDialogPresenter(IConfirmDialogView view, Func<UniTask> dismissCallback) : base(view)
        {
            _dismissCallback = dismissCallback;
        }

        public override void Initialize()
        {
            View.OnConfirmClicked += HandleDismiss;
            View.OnCancelClicked += HandleDismiss;
            View.UpdateMessage("Are you sure?");
        }

        public override void Dispose()
        {
            View.OnConfirmClicked -= HandleDismiss;
            View.OnCancelClicked -= HandleDismiss;
        }

        private void HandleDismiss()
        {
            _dismissCallback().Forget();
        }
    }
}
