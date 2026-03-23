using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SimpleGame.Game.Popup;
using UnityEngine;

namespace SimpleGame.Tests.Game
{
    /// <summary>
    /// Edit-mode tests for ConsentGatePresenter flag logic and accept flow.
    /// </summary>
    public class ConsentGateTests
    {
        private const string Key = ConsentGatePresenter.HasAcceptedKey;

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(Key);
        }

        // ── ShouldShow ──────────────────────────────────────────────────────

        [Test]
        public void ShouldShow_KeyAbsent_ReturnsTrue()
        {
            PlayerPrefs.DeleteKey(Key);
            Assert.IsTrue(ConsentGatePresenter.ShouldShow());
        }

        [Test]
        public void ShouldShow_AfterMarkAccepted_ReturnsFalse()
        {
            ConsentGatePresenter.MarkAccepted();
            Assert.IsFalse(ConsentGatePresenter.ShouldShow());
        }

        // ── MarkAccepted ────────────────────────────────────────────────────

        [Test]
        public void MarkAccepted_WritesKeyToOne()
        {
            PlayerPrefs.DeleteKey(Key);
            ConsentGatePresenter.MarkAccepted();
            Assert.AreEqual(1, PlayerPrefs.GetInt(Key, 0));
        }

        [Test]
        public void MarkAccepted_IdempotentOnRepeatCall()
        {
            ConsentGatePresenter.MarkAccepted();
            ConsentGatePresenter.MarkAccepted();
            Assert.AreEqual(1, PlayerPrefs.GetInt(Key, 0));
        }

        // ── WaitForAccept ───────────────────────────────────────────────────

        [Test]
        public void WaitForAccept_ResolvesWhenAcceptClicked()
        {
            PlayerPrefs.DeleteKey(Key);
            var view = new MockConsentGateView();
            var presenter = new ConsentGatePresenter(view);
            presenter.Initialize();

            bool resolved = false;
            presenter.WaitForAccept().ContinueWith(() => resolved = true).Forget();

            view.SimulateAccept();

            Assert.IsTrue(resolved, "WaitForAccept should resolve when OnAcceptClicked fires.");
        }

        [Test]
        public void Accept_SetsInteractableToFalse()
        {
            PlayerPrefs.DeleteKey(Key);
            var view = new MockConsentGateView();
            var presenter = new ConsentGatePresenter(view);
            presenter.Initialize();
            presenter.WaitForAccept().Forget();

            view.SimulateAccept();

            Assert.IsFalse(view.IsAcceptInteractable, "Accept button should be disabled after tapping.");
        }

        [Test]
        public void Accept_MarksAccepted_SoShouldShowReturnsFalse()
        {
            PlayerPrefs.DeleteKey(Key);
            var view = new MockConsentGateView();
            var presenter = new ConsentGatePresenter(view);
            presenter.Initialize();
            presenter.WaitForAccept().Forget();

            view.SimulateAccept();

            Assert.IsFalse(ConsentGatePresenter.ShouldShow());
        }

        // ── NoDismissPath ───────────────────────────────────────────────────

        [Test]
        public void IConsentGateView_HasNoCloseOrSkipEvents()
        {
            // IConsentGateView must expose only OnAcceptClicked — no dismiss path.
            var type = typeof(IConsentGateView);
            var events = type.GetEvents();
            Assert.AreEqual(1, events.Length,
                "IConsentGateView must have exactly one event (OnAcceptClicked). Found: " +
                string.Join(", ", System.Array.ConvertAll(events, e => e.Name)));
            Assert.AreEqual("OnAcceptClicked", events[0].Name);
        }
    }

    // ── Mock ────────────────────────────────────────────────────────────────

    internal class MockConsentGateView : IConsentGateView
    {
        public event Action OnAcceptClicked;

        public bool IsAcceptInteractable { get; private set; } = true;

        public void SetAcceptInteractable(bool interactable) => IsAcceptInteractable = interactable;

        public void SimulateAccept() => OnAcceptClicked?.Invoke();

        // IPopupView stubs
        public UniTask AnimateInAsync(CancellationToken ct = default)  => UniTask.CompletedTask;
        public UniTask AnimateOutAsync(CancellationToken ct = default) => UniTask.CompletedTask;
    }
}
