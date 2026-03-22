using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.Settings
{
    public class SettingsPresenter : Presenter<ISettingsView>
    {
        private readonly IPlatformLinkService _linkService;
        private UniTaskCompletionSource _backTcs;

        /// <summary>
        /// Constructor with optional platform link service.
        /// If null, link/unlink buttons are wired but no-op silently.
        /// </summary>
        public SettingsPresenter(ISettingsView view, IPlatformLinkService linkService = null)
            : base(view)
        {
            _linkService = linkService;
        }

        public override void Initialize()
        {
            View.OnBackClicked += HandleBackClicked;
            View.OnLinkGameCenterClicked += HandleLinkGameCenter;
            View.OnLinkGooglePlayClicked += HandleLinkGooglePlay;
            View.OnUnlinkGameCenterClicked += HandleUnlinkGameCenter;
            View.OnUnlinkGooglePlayClicked += HandleUnlinkGooglePlay;

            View.UpdateTitle("Settings");
            RefreshLinkStatus();
        }

        public override void Dispose()
        {
            View.OnBackClicked -= HandleBackClicked;
            View.OnLinkGameCenterClicked -= HandleLinkGameCenter;
            View.OnLinkGooglePlayClicked -= HandleLinkGooglePlay;
            View.OnUnlinkGameCenterClicked -= HandleUnlinkGameCenter;
            View.OnUnlinkGooglePlayClicked -= HandleUnlinkGooglePlay;
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

        private async void HandleLinkGameCenter()
        {
            if (_linkService == null) return;
            await _linkService.LinkGameCenterAsync();
            RefreshLinkStatus();
        }

        private async void HandleLinkGooglePlay()
        {
            if (_linkService == null) return;
            await _linkService.LinkGooglePlayAsync();
            RefreshLinkStatus();
        }

        private async void HandleUnlinkGameCenter()
        {
            if (_linkService == null) return;
            await _linkService.UnlinkGameCenterAsync();
            RefreshLinkStatus();
        }

        private async void HandleUnlinkGooglePlay()
        {
            if (_linkService == null) return;
            await _linkService.UnlinkGooglePlayAsync();
            RefreshLinkStatus();
        }

        private void RefreshLinkStatus()
        {
            if (_linkService == null)
            {
                View.UpdateLinkStatus(false, false);
                return;
            }
            View.UpdateLinkStatus(_linkService.IsGameCenterLinked, _linkService.IsGooglePlayLinked);
        }
    }
}
