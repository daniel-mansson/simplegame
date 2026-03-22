using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Presenter for the first-launch platform link popup.
    /// Handles link/unlink actions and the skip-forever flag.
    ///
    /// The popup is shown once: after the first successful PlayFab login,
    /// before the main menu, if the player has not yet linked any platform account
    /// and has not previously skipped. Skipping writes a PlayerPrefs flag.
    /// </summary>
    public class PlatformLinkPresenter : Presenter<IPlatformLinkView>
    {
        public const string HasSeenLinkPromptKey = "PlayFab_HasSeenLinkPrompt";

        private readonly IPlatformLinkService _linkService;
        private UniTaskCompletionSource<bool> _completionTcs;

        public PlatformLinkPresenter(IPlatformLinkView view, IPlatformLinkService linkService)
            : base(view)
        {
            _linkService = linkService;
        }

        public override void Initialize()
        {
            View.OnLinkGameCenterClicked += HandleLinkGameCenter;
            View.OnLinkGooglePlayClicked += HandleLinkGooglePlay;
            View.OnSkipClicked += HandleSkip;
            RefreshView();
        }

        public override void Dispose()
        {
            View.OnLinkGameCenterClicked -= HandleLinkGameCenter;
            View.OnLinkGooglePlayClicked -= HandleLinkGooglePlay;
            View.OnSkipClicked -= HandleSkip;
            _completionTcs?.TrySetCanceled();
            _completionTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves when the player links, unlinks, or skips.
        /// Returns true if a link was performed; false if skipped.
        /// </summary>
        public UniTask<bool> WaitForResult()
        {
            _completionTcs?.TrySetCanceled();
            _completionTcs = new UniTaskCompletionSource<bool>();
            return _completionTcs.Task;
        }

        /// <summary>
        /// Whether this prompt should be shown.
        /// Returns true if the player has not yet seen the prompt AND has no linked platform.
        /// </summary>
        public static bool ShouldShow(IPlatformLinkService linkService)
        {
            bool hasSeen = UnityEngine.PlayerPrefs.GetInt(HasSeenLinkPromptKey, 0) == 1;
            if (hasSeen) return false;
            return !linkService.IsGameCenterLinked && !linkService.IsGooglePlayLinked;
        }

        /// <summary>Marks the prompt as permanently seen so it never shows again.</summary>
        public static void MarkSeen()
        {
            UnityEngine.PlayerPrefs.SetInt(HasSeenLinkPromptKey, 1);
            UnityEngine.PlayerPrefs.Save();
        }

        private async void HandleLinkGameCenter()
        {
            var success = await _linkService.LinkGameCenterAsync();
            RefreshView();
            if (success)
            {
                MarkSeen();
                _completionTcs?.TrySetResult(true);
            }
            else
            {
                Debug.Log("[PlatformLinkPresenter] Game Center link failed or unavailable on this platform.");
            }
        }

        private async void HandleLinkGooglePlay()
        {
            var success = await _linkService.LinkGooglePlayAsync();
            RefreshView();
            if (success)
            {
                MarkSeen();
                _completionTcs?.TrySetResult(true);
            }
            else
            {
                Debug.Log("[PlatformLinkPresenter] Google Play link failed or unavailable on this platform.");
            }
        }

        private void HandleSkip()
        {
            MarkSeen();
            _completionTcs?.TrySetResult(false);
        }

        private void RefreshView()
        {
            View.UpdateLinkStatus(_linkService.IsGameCenterLinked, _linkService.IsGooglePlayLinked);
        }
    }
}
