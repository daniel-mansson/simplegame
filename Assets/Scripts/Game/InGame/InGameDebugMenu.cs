using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Developer-only IMGUI overlay drawn in the top-left corner of the InGame scene.
    /// Lets you restart the current level with a custom grid size and slot count without
    /// leaving the editor or modifying any assets.
    ///
    /// Add this component to any GameObject in the InGame scene.
    /// It auto-discovers InGameSceneController on Start.
    /// </summary>
    public class InGameDebugMenu : MonoBehaviour
    {
        // ── State ─────────────────────────────────────────────────────────
        private InGameSceneController _ctrl;
        private bool  _visible = false;
        private string _rowsStr  = "3";
        private string _colsStr  = "3";
        private string _slotsStr = "3";
        private string _status   = "";

        // ── Layout constants ──────────────────────────────────────────────
        private const float PanelW = 180f;
        private const float PanelH = 170f;
        private const float Pad    = 8f;

        private static readonly GUIStyle _boxStyle   = new GUIStyle();
        private static readonly GUIStyle _labelStyle = new GUIStyle();
        private static bool _stylesInit = false;

        // ── Unity ─────────────────────────────────────────────────────────

        private void Start()
        {
            _ctrl = FindObjectOfType<InGameSceneController>();
            if (_ctrl == null)
                Debug.LogWarning("[InGameDebugMenu] InGameSceneController not found.");
        }

        private void OnGUI()
        {
            InitStyles();

            // Toggle button — always visible in top-left corner
            var toggleRect = new Rect(4, 4, 28, 22);
            if (GUI.Button(toggleRect, _visible ? "▲" : "▼"))
                _visible = !_visible;

            if (!_visible) return;

            // Panel background
            var panel = new Rect(4, 28, PanelW, PanelH);
            GUI.Box(panel, GUIContent.none);

            float x = panel.x + Pad;
            float y = panel.y + Pad;
            float fw = PanelW - Pad * 2f;
            float lh = 20f;
            float fh = 22f;

            GUI.Label(new Rect(x, y, fw, lh), "── Debug Restart ──");
            y += lh + 2f;

            // Rows
            GUI.Label(new Rect(x, y, 50f, lh), "Rows");
            _rowsStr = GUI.TextField(new Rect(x + 52f, y, fw - 52f, fh), _rowsStr);
            y += fh + 3f;

            // Cols
            GUI.Label(new Rect(x, y, 50f, lh), "Cols");
            _colsStr = GUI.TextField(new Rect(x + 52f, y, fw - 52f, fh), _colsStr);
            y += fh + 3f;

            // Slots
            GUI.Label(new Rect(x, y, 50f, lh), "Slots");
            _slotsStr = GUI.TextField(new Rect(x + 52f, y, fw - 52f, fh), _slotsStr);
            y += fh + 6f;

            // Restart button
            if (GUI.Button(new Rect(x, y, fw, 26f), "Restart"))
                Restart();
            y += 30f;

            // Status
            if (!string.IsNullOrEmpty(_status))
                GUI.Label(new Rect(x, y, fw, lh), _status);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void Restart()
        {
            if (_ctrl == null) { _status = "No controller!"; return; }

            if (!int.TryParse(_rowsStr,  out int rows)  || rows  < 2) { _status = "Bad rows (min 2)";  return; }
            if (!int.TryParse(_colsStr,  out int cols)  || cols  < 2) { _status = "Bad cols (min 2)";  return; }
            if (!int.TryParse(_slotsStr, out int slots) || slots < 1) { _status = "Bad slots (min 1)"; return; }

            _status = $"Restarting {rows}×{cols} s={slots}…";
            _ctrl.SetDebugOverride(rows, cols, slots);
            _ctrl.RunAsync().Forget();
        }

        private static void InitStyles()
        {
            if (_stylesInit) return;
            _stylesInit = true;
            // Minimal styling — relies on default Unity IMGUI skin
        }
    }
}
