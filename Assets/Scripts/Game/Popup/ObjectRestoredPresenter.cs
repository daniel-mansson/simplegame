using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the ObjectRestored celebration popup.
    /// Shows the name of the restored object and waits for continue.
    /// </summary>
    public class ObjectRestoredPresenter : Presenter<IObjectRestoredView>
    {
        private UniTaskCompletionSource _continueTcs;

        public ObjectRestoredPresenter(IObjectRestoredView view) : base(view) { }

        public void Initialize(string objectName)
        {
            View.OnContinueClicked += HandleContinue;
            View.UpdateObjectName($"{objectName} Restored!");
        }

        public override void Dispose()
        {
            View.OnContinueClicked -= HandleContinue;
            _continueTcs?.TrySetCanceled();
            _continueTcs = null;
        }

        public UniTask WaitForContinue()
        {
            _continueTcs?.TrySetCanceled();
            _continueTcs = new UniTaskCompletionSource();
            return _continueTcs.Task;
        }

        private void HandleContinue() => _continueTcs?.TrySetResult();
    }
}
