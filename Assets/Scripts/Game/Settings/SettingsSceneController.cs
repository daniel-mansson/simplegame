using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleGame.Game.Boot;
using SimpleGame.Game.Settings;
using UnityEngine;

namespace SimpleGame.Game.Settings
{
    /// <summary>
    /// SceneController for the Settings scene. Owns the SettingsPresenter
    /// lifetime for the duration of one RunAsync() call. Returns ScreenId.MainMenu
    /// when the user presses back.
    /// </summary>
    public class SettingsSceneController : MonoBehaviour, ISceneController
    {
        [SerializeField] private SettingsView _settingsView;

        // Allows test/editor code to supply a mock view without a Unity scene.
        private ISettingsView _viewOverride;

        private ISettingsView ActiveView => _viewOverride != null ? _viewOverride : _settingsView;

        private UIFactory _uiFactory;

        /// <summary>Inject dependencies. Called by the boot loop before RunAsync.</summary>
        public void Initialize(UIFactory uiFactory)
        {
            _uiFactory = uiFactory;
        }

        /// <summary>
        /// For editor / test use: supply a mock view that overrides the serialized field.
        /// </summary>
        public void SetViewForTesting(ISettingsView view)
        {
            _viewOverride = view;
        }

        public async UniTask<ScreenId> RunAsync(CancellationToken ct = default)
        {
            var presenter = _uiFactory.CreateSettingsPresenter(ActiveView);
            presenter.Initialize();
            try
            {
                await presenter.WaitForBack();
                return ScreenId.MainMenu;
            }
            finally
            {
                presenter.Dispose();
            }
        }
    }
}
