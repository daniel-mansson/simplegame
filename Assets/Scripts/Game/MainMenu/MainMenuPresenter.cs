using Cysharp.Threading.Tasks;
using SimpleGame.Core.MVP;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// Presenter for the main screen. Shows the current environment with
    /// restorable objects, golden piece balance, and play button.
    /// Tap an object to spend golden pieces and restore it one step.
    /// Resolves ObjectRestored action when an object completes.
    /// </summary>
    public class MainMenuPresenter : Presenter<IMainMenuView>
    {
        private readonly MetaProgressionService _metaProgression;
        private readonly IGoldenPieceService _goldenPieces;
        private readonly ProgressionService _progression;
        private readonly GameSessionService _session;
        private readonly EnvironmentData _currentEnvironment;
        private readonly bool _hasNextEnvironment;

        private UniTaskCompletionSource<MainMenuAction> _actionTcs;
        private UniTaskCompletionSource<bool> _closeShopTcs;
        private string _lastRestoredObjectName;

        public MainMenuPresenter(IMainMenuView view,
                                 MetaProgressionService metaProgression,
                                 IGoldenPieceService goldenPieces,
                                 ProgressionService progression,
                                 GameSessionService session,
                                 EnvironmentData currentEnvironment,
                                 bool hasNextEnvironment = false)
            : base(view)
        {
            _metaProgression = metaProgression;
            _goldenPieces = goldenPieces;
            _progression = progression;
            _session = session;
            _currentEnvironment = currentEnvironment;
            _hasNextEnvironment = hasNextEnvironment;
        }

        /// <summary>Name of the last object that was fully restored (for popup).</summary>
        public string LastRestoredObjectName => _lastRestoredObjectName;

        public override void Initialize()
        {
            View.OnSettingsClicked += HandleSettingsClicked;
            View.OnPlayClicked += HandlePlayClicked;
            View.OnObjectTapped += HandleObjectTapped;
            View.OnResetProgressClicked += HandleResetProgressClicked;
            View.OnNextEnvironmentClicked += HandleNextEnvironmentClicked;
            View.OnShopClicked += HandleShopClicked;
            View.OnShopBackClicked += HandleShopBackClicked;

            RefreshView();
        }

        public override void Dispose()
        {
            View.OnSettingsClicked -= HandleSettingsClicked;
            View.OnPlayClicked -= HandlePlayClicked;
            View.OnObjectTapped -= HandleObjectTapped;
            View.OnResetProgressClicked -= HandleResetProgressClicked;
            View.OnNextEnvironmentClicked -= HandleNextEnvironmentClicked;
            View.OnShopClicked -= HandleShopClicked;
            View.OnShopBackClicked -= HandleShopBackClicked;
            _actionTcs?.TrySetCanceled();
            _actionTcs = null;
            _closeShopTcs?.TrySetCanceled();
            _closeShopTcs = null;
        }

        /// <summary>
        /// Returns a task that resolves with the action the user took.
        /// </summary>
        public UniTask<MainMenuAction> WaitForAction()
        {
            _actionTcs?.TrySetCanceled();
            _actionTcs = new UniTaskCompletionSource<MainMenuAction>();
            return _actionTcs.Task;
        }

        /// <summary>
        /// Returns a task that resolves when the Back/CloseShop button is pressed
        /// while the shop screen is open. Used by HandleShopScreenAsync to race
        /// against ShopPresenter.WaitForResult().
        /// </summary>
        public async UniTask WaitForCloseShopAsync()
        {
            _closeShopTcs?.TrySetCanceled();
            _closeShopTcs = new UniTaskCompletionSource<bool>();
            await _closeShopTcs.Task;
        }

        /// <summary>Refresh view with current state. Call after returning from popups.</summary>
        public void RefreshView()
        {
            View.UpdateEnvironmentName(_currentEnvironment.environmentName);
            View.UpdateBalance($"{_goldenPieces.Balance} Golden Pieces");
            View.UpdateLevelDisplay($"Level {_progression.CurrentLevel}");

            var envComplete = _metaProgression.IsEnvironmentComplete(_currentEnvironment);
            View.SetNextEnvironmentVisible(envComplete && _hasNextEnvironment);

            var objects = _currentEnvironment.objects;
            var displayData = new ObjectDisplayData[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                var current = _metaProgression.GetCurrentSteps(obj);
                var isComplete = _metaProgression.IsObjectComplete(obj);
                var isBlocked = _metaProgression.IsBlocked(obj);

                displayData[i] = new ObjectDisplayData
                {
                    Name = obj.displayName,
                    Progress = isComplete ? "Complete" : $"{current}/{obj.totalSteps}",
                    IsBlocked = isBlocked,
                    IsComplete = isComplete,
                    CostPerStep = obj.costPerStep
                };
            }
            View.UpdateObjects(displayData);
        }

        private void HandleSettingsClicked() => _actionTcs?.TrySetResult(MainMenuAction.Settings);
        private void HandleShopClicked()     => _actionTcs?.TrySetResult(MainMenuAction.OpenShop);
        private void HandleShopBackClicked()
        {
            // Resolves the dedicated close-shop awaiter (used while shop screen is open)
            // so HandleShopScreenAsync exits cleanly.
            _closeShopTcs?.TrySetResult(true);
        }

        private void HandlePlayClicked()
        {
            _session.ResetForNewGame(_progression.CurrentLevel);
            _actionTcs?.TrySetResult(MainMenuAction.Play);
        }

        private void HandleResetProgressClicked()
        {
            _actionTcs?.TrySetResult(MainMenuAction.ResetProgress);
        }

        private void HandleNextEnvironmentClicked()
        {
            _actionTcs?.TrySetResult(MainMenuAction.NextEnvironment);
        }

        private void HandleObjectTapped(int index)
        {
            if (_currentEnvironment.objects == null || index < 0 || index >= _currentEnvironment.objects.Length)
            {
                Debug.LogWarning($"[MainMenuPresenter] Invalid object index: {index}");
                return;
            }

            var obj = _currentEnvironment.objects[index];

            if (_metaProgression.IsObjectComplete(obj))
            {
                Debug.Log($"[MainMenuPresenter] '{obj.displayName}' is already complete.");
                return;
            }

            if (_metaProgression.IsBlocked(obj))
            {
                Debug.Log($"[MainMenuPresenter] '{obj.displayName}' is blocked.");
                return;
            }

            if (!_goldenPieces.TrySpend(obj.costPerStep))
            {
                Debug.Log($"[MainMenuPresenter] Not enough golden pieces for '{obj.displayName}'.");
                return;
            }

            _metaProgression.TryRestoreStep(obj);
            _metaProgression.Save();
            _goldenPieces.Save();

            RefreshView();

            if (_metaProgression.IsObjectComplete(obj))
            {
                _lastRestoredObjectName = obj.displayName;
                _actionTcs?.TrySetResult(MainMenuAction.ObjectRestored);
            }
        }
    }
}
