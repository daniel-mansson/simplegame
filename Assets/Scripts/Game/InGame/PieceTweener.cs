using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Lightweight tween helpers for puzzle piece movement.
    /// No external tween library — uses UniTask + manual lerp with easing functions.
    ///
    /// <list type="bullet">
    ///   <item><see cref="PlaceOnBoard"/> — anticipation wind-up → fly to board → settle bounce</item>
    ///   <item><see cref="SlideToSlot"/> — smooth ease-out slide to tray slot</item>
    /// </list>
    ///
    /// All methods are fire-and-forget UniTask coroutines. Cancellation via the
    /// GameObject's destroy token keeps things clean on scene unload.
    /// </summary>
    internal static class PieceTweener
    {
        // ── Timing ────────────────────────────────────────────────────────
        private const float WindupSec   = 0.08f;   // anticipation pull-back
        private const float FlySec      = 0.22f;   // main arc to board
        private const float SettleSec   = 0.07f;   // squash-settle at landing

        private const float SlideSec    = 0.14f;   // tray slot slide
        private const float ShakeSec    = 0.40f;   // wrong-tap shake duration

        // ── Scale factors ─────────────────────────────────────────────────
        private const float WindupScale = 1.30f;   // puff up before launch
        private const float OvershootZ  = 0.6f;    // how far forward (−z) the piece pulls during windup

        // ─────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Anticipation wind-up → fly to <paramref name="targetLocal"/> in board space
        /// → settle with a slight squash.
        /// The piece must already be reparented to boardParent before calling.
        /// </summary>
        public static async UniTaskVoid PlaceOnBoard(
            GameObject piece,
            Vector3 targetLocal,
            CancellationToken ct)
        {
            if (piece == null) return;
            var t = piece.transform;

            Vector3 startLocal = t.localPosition;
            Vector3 startScale = t.localScale;
            Vector3 finalScale = Vector3.one;

            // ── Phase 1: Windup — scale up, pull toward camera (−z in world = −z local here) ──
            float windupZ    = startLocal.z - OvershootZ;
            Vector3 windupLocal = new Vector3(startLocal.x, startLocal.y, windupZ);
            Vector3 windupScale = startScale * WindupScale;

            float elapsed = 0f;
            while (elapsed < WindupSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f = Mathf.Clamp01(elapsed / WindupSec);
                float e = EaseOutQuad(f);
                t.localPosition = Vector3.LerpUnclamped(startLocal, windupLocal, e);
                t.localScale    = Vector3.LerpUnclamped(startScale, windupScale, e);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;
            t.localPosition = windupLocal;
            t.localScale    = windupScale;

            // ── Phase 2: Fly — overshoot slightly past target on Z, ease-in-out ──
            // A tiny arc: mid-point lifts toward camera before settling back
            Vector3 flyStart = windupLocal;
            Vector3 flyScale = windupScale;

            elapsed = 0f;
            while (elapsed < FlySec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f = Mathf.Clamp01(elapsed / FlySec);
                float ep = EaseInOutBack(f);   // slight overshoot on arrive
                float es = EaseOutQuad(f);

                // Position: blend with a small z-arc (piece dips forward at peak)
                Vector3 basePos = Vector3.LerpUnclamped(flyStart, targetLocal, ep);
                float   arc     = Mathf.Sin(f * Mathf.PI) * 0.3f; // world-unit forward pop
                basePos.z -= arc;

                t.localPosition = basePos;
                t.localScale    = Vector3.LerpUnclamped(flyScale, finalScale, es);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;

            // ── Phase 3: Settle — quick squash-and-stretch snap ──
            // Squash: flatten XY, elongate Z briefly, then snap to finalScale
            Vector3 squashScale = new Vector3(finalScale.x * 1.15f, finalScale.y * 0.88f, finalScale.z * 1.1f);

            elapsed = 0f;
            while (elapsed < SettleSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f = Mathf.Clamp01(elapsed / SettleSec);
                float e = EaseOutQuad(f);
                // First half squash, second half restore
                Vector3 from = f < 0.5f ? finalScale : squashScale;
                Vector3 to   = f < 0.5f ? squashScale : finalScale;
                float   fn   = f < 0.5f ? f * 2f : (f - 0.5f) * 2f;
                t.localPosition = Vector3.Lerp(t.localPosition, targetLocal, e);
                t.localScale    = Vector3.Lerp(from, to, fn);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;

            // Snap to exact final values
            t.localPosition = targetLocal;
            t.localScale    = finalScale;
        }

        /// <summary>
        /// Smooth ease-out slide from current position/scale to <paramref name="targetWorld"/>
        /// and <paramref name="targetScale"/>.
        /// </summary>
        public static async UniTaskVoid SlideToSlot(
            GameObject piece,
            Vector3 targetWorld,
            Vector3 targetScale,
            CancellationToken ct)
        {
            if (piece == null) return;
            var t = piece.transform;

            Vector3 startPos   = t.position;
            Vector3 startScale = t.localScale;

            float elapsed = 0f;
            while (elapsed < SlideSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f = Mathf.Clamp01(elapsed / SlideSec);
                float e = EaseOutCubic(f);
                t.position   = Vector3.LerpUnclamped(startPos,   targetWorld, e);
                t.localScale = Vector3.LerpUnclamped(startScale, targetScale, e);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;

            t.position   = targetWorld;
            t.localScale = targetScale;
        }

        /// <summary>
        /// Strong lateral shake — 5 oscillations with exponentially decaying amplitude.
        /// Returns the piece to <paramref name="restWorld"/> on completion.
        /// </summary>
        public static async UniTask ShakePiece(
            GameObject piece,
            Vector3    restWorld,
            CancellationToken ct)
        {
            if (piece == null) return;
            var t = piece.transform;

            const float amplitude = 0.28f;  // world-unit horizontal offset at peak
            const float cycles    = 5.5f;   // oscillations (half-cycle extra so it ends back at rest)

            float elapsed = 0f;
            while (elapsed < ShakeSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f       = Mathf.Clamp01(elapsed / ShakeSec);
                float decay   = 1f - EaseOutQuad(f);          // amplitude envelope
                float offset  = Mathf.Sin(f * cycles * Mathf.PI * 2f) * amplitude * decay;
                t.position = restWorld + new Vector3(offset, 0f, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;
            t.position = restWorld;
        }

        // ─────────────────────────────────────────────────────────────────
        // Easing functions (all take t ∈ [0,1], return f ∈ [0,1])
        // ─────────────────────────────────────────────────────────────────

        private static float EaseOutQuad(float t)
            => 1f - (1f - t) * (1f - t);

        private static float EaseOutCubic(float t)
            => 1f - Mathf.Pow(1f - t, 3f);

        /// <summary>Ease-in-out with a small overshoot at the end (Back easing).</summary>
        private static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (2f * t - 2f) + c2) + 2f) / 2f;
        }
    }
}
