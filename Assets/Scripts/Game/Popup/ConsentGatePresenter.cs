using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the first-launch consent gate popup.
    ///
    /// The popup is shown on every launch until the player taps Accept.
    /// There is no dismiss path — Accept is the only exit (R158, D094).
    ///
    /// PlayerPrefs key <see cref="HasAcceptedKey"/> is written to 1 on Accept
    /// and read by <see cref="ShouldShow"/> to gate future launches.
    /// </summary>
    public class ConsentGatePresenter : Presenter<IConsentGateView>
    {
        public const string HasAcceptedKey = "ConsentGate_Accepted";

        private UniTaskCompletionSource _completionTcs;

        public ConsentGatePresenter(IConsentGateView view) : base(view) { }

        public override void Initialize()
        {
            View.OnAcceptClicked += HandleAccept;
        }

        public override void Dispose()
        {
            View.OnAcceptClicked -= HandleAccept;
            _completionTcs?.TrySetCanceled();
            _completionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the player taps Accept.
        /// </summary>
        public UniTask WaitForAccept()
        {
            _completionTcs?.TrySetCanceled();
            _completionTcs = new UniTaskCompletionSource();
            return _completionTcs.Task;
        }

        /// <summary>
        /// Whether the consent gate should be shown this launch.
        /// Returns true if the player has not yet accepted.
        /// </summary>
        public static bool ShouldShow()
        {
            return PlayerPrefs.GetInt(HasAcceptedKey, 0) == 0;
        }

        /// <summary>
        /// Marks consent as permanently accepted. Never shows again after this.
        /// </summary>
        public static void MarkAccepted()
        {
            PlayerPrefs.SetInt(HasAcceptedKey, 1);
            PlayerPrefs.Save();
        }

        private void HandleAccept()
        {
            View.SetAcceptInteractable(false);
            MarkAccepted();
            _completionTcs?.TrySetResult();
        }
    }
}
