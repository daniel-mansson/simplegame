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
    ///   <item><see cref="PlaceOnBoard"/> — linear lerp to board position</item>
    ///   <item><see cref="SlideToSlot"/> — smooth ease-out slide to tray slot</item>
    /// </list>
    ///
    /// All methods are fire-and-forget UniTask coroutines. Cancellation via the
    /// GameObject's destroy token keeps things clean on scene unload.
    /// </summary>
    internal static class PieceTweener
    {
        // ── Timing ────────────────────────────────────────────────────────
        private const float PlaceSec    = 0.25f;   // linear lerp to board
        private const float SlideSec    = 0.14f;   // tray slot slide
        private const float ShakeSec    = 0.40f;   // wrong-tap shake duration

        // ─────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Linear lerp from current position/scale/rotation to
        /// <paramref name="targetLocal"/> in board space.
        /// The piece must already be reparented to boardParent before calling.
        /// </summary>
        public static async UniTaskVoid PlaceOnBoard(
            GameObject piece,
            Vector3    targetLocal,
            Quaternion targetRotation,
            CancellationToken ct)
        {
            if (piece == null) return;
            var t = piece.transform;

            Vector3    startPos   = t.localPosition;
            Vector3    startScale = t.localScale;
            Quaternion startRot   = t.localRotation;
            Vector3    finalScale = Vector3.one;

            float elapsed = 0f;
            while (elapsed < PlaceSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f = Mathf.Clamp01(elapsed / PlaceSec);
                t.localPosition = Vector3.Lerp(startPos, targetLocal, f);
                t.localScale    = Vector3.Lerp(startScale, finalScale, f);
                t.localRotation = Quaternion.Slerp(startRot, targetRotation, f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (piece == null) return;

            t.localPosition = targetLocal;
            t.localScale    = finalScale;
            t.localRotation = targetRotation;
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

            const float ampFraction = 0.5f;   // shake distance as fraction of piece size
            const float cycles      = 5.5f;

            // Scale amplitude by piece size in XY
            float pieceSize = Mathf.Max(t.localScale.x, t.localScale.y);
            float amplitude = ampFraction * pieceSize;

            float elapsed = 0f;
            while (elapsed < ShakeSec)
            {
                if (piece == null) return;
                elapsed += Time.deltaTime;
                float f       = Mathf.Clamp01(elapsed / ShakeSec);
                float decay   = 1f - EaseOutQuad(f);
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
    }
}
