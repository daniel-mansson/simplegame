using System;
using SimpleGame.Core.MVP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleGame.Game.Popup
{
    /// <summary>
    /// Unity view implementation of <see cref="IConsentGateView"/>.
    ///
    /// Shows the Terms of Service and Privacy Policy links. The player
    /// must tap Accept to proceed — there is no close or skip button (D094).
    ///
    /// Link buttons open https://simplemagicstudios.com/play in the device browser.
    /// Inherits default bounce-in / scale-out animations from <see cref="PopupViewBase"/>.
    /// </summary>
    public class ConsentGateView : PopupViewBase, IConsentGateView
    {
        private const string PolicyUrl = "https://simplemagicstudios.com/play";

        [SerializeField] private Button   _acceptButton;
        [SerializeField] private Button   _tosLinkButton;
        [SerializeField] private Button   _privacyLinkButton;

        public event Action OnAcceptClicked;

        private void Awake()
        {
            _acceptButton.onClick.AddListener(() => OnAcceptClicked?.Invoke());
            _tosLinkButton.onClick.AddListener(() => Application.OpenURL(PolicyUrl));
            _privacyLinkButton.onClick.AddListener(() => Application.OpenURL(PolicyUrl));
        }

        public void SetAcceptInteractable(bool interactable)
        {
            _acceptButton.interactable = interactable;
        }
    }
}
