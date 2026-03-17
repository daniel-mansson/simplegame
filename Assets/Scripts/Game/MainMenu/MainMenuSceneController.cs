using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Core.PopupManagement;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Meta;
using SimpleGame.Game.Popup;
using SimpleGame.Game.Services;
using UnityEngine;

namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// SceneController for the MainMenu scene (now the meta world main screen).
    /// Shows the current environment with restorable objects, golden piece
    /// balance, and play button. Handles object restoration inline with
    /// ObjectRestored celebration popup. Returns ScreenId for navigation.
    /// </summary>
    public class MainMenuSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private ConfirmDialogView _confirmDialogView;

        private IMainMenuView _mainMenuViewOverride;
        private IConfirmDialogView _confirmDialogViewOverride;
        private IObjectRestoredView _objectRestoredViewOverride;

        private IMainMenuView ActiveMainMenuView => _mainMenuViewOverride != null ? _mainMenuViewOverride : _mainMenuView;

        private IConfirmDialogView ActiveConfirmDialogView
        {
            get
            {
                if (_confirmDialogViewOverride != null) return _confirmDialogViewOverride;
                if (_confirmDialogView != null) return _confirmDialogView;
                var found = FindFirstObjectByType<ConfirmDialogView>(FindObjectsInactive.Include);
                if (found == null)
                    Debug.LogError("[MainMenuSceneController] ConfirmDialogView not found in any loaded scene.");
                return found;
            }
        }

        private IObjectRestoredView ActiveObjectRestoredView
        {
            get
            {
                if (_objectRestoredViewOverride != null) return _objectRestoredViewOverride;
                var found = FindFirstObjectByType<ObjectRestoredView>(FindObjectsInactive.Include);
                if (found == null)
                    Debug.LogError("[MainMenuSceneController] ObjectRestoredView not found in any loaded scene.");
                return found;
            }
        }

        private UIFactory _uiFactory;
        private PopupManager<PopupId> _popupManager;
        private MetaProgressionService _metaProgression;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory, PopupManager<PopupId> popupManager,
                               MetaProgressionService metaProgression = null)
        {
            _uiFactory = uiFactory;
            _popupManager = popupManager;
            _metaProgression = metaProgression;
        }

        /// <summary>
        /// For editor / test use: supply mock views that override the serialized fields.
        /// </summary>
        public void SetViewsForTesting(IMainMenuView mainMenuView,
                                        IConfirmDialogView confirmDialogView = null,
                                        IObjectRestoredView objectRestoredView = null)
        {
            _mainMenuViewOverride = mainMenuView;
            _confirmDialogViewOverride = confirmDialogView;
            _objectRestoredViewOverride = objectRestoredView;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            // Determine current environment
            var currentEnv = GetCurrentEnvironment();

            var presenter = _uiFactory.CreateMainMenuPresenter(ActiveMainMenuView, currentEnv);
            presenter.Initialize();
            try
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    var action = await presenter.WaitForAction();

                    if (action == MainMenuAction.Settings)
                        return ScreenId.Settings;

                    if (action == MainMenuAction.Play)
                        return ScreenId.InGame;

                    if (action == MainMenuAction.ObjectRestored)
                    {
                        await HandleObjectRestoredPopupAsync(presenter.LastRestoredObjectName, ct);
                        presenter.RefreshView();
                    }
                }
            }
            finally
            {
                presenter.Dispose();
            }
        }

        private EnvironmentData GetCurrentEnvironment()
        {
            if (_metaProgression == null || _metaProgression.WorldData == null
                || _metaProgression.WorldData.environments == null
                || _metaProgression.WorldData.environments.Length == 0)
            {
                Debug.LogWarning("[MainMenuSceneController] No world data available.");
                return null;
            }

            // Find the first non-complete environment, or fall back to the last one
            var envs = _metaProgression.WorldData.environments;
            for (int i = 0; i < envs.Length; i++)
            {
                if (!_metaProgression.IsEnvironmentComplete(envs[i]))
                    return envs[i];
            }
            return envs[envs.Length - 1]; // All complete — show last
        }

        private async UniTask HandleObjectRestoredPopupAsync(string objectName, CancellationToken ct)
        {
            var view = ActiveObjectRestoredView;
            if (view == null) return;

            var presenter = _uiFactory.CreateObjectRestoredPresenter(view);
            presenter.Initialize(objectName);
            try
            {
                await _popupManager.ShowPopupAsync(PopupId.ObjectRestored, ct);
                await presenter.WaitForContinue();
                await _popupManager.DismissPopupAsync(ct);
            }
            finally
            {
                presenter.Dispose();
            }
        }
    }
}
