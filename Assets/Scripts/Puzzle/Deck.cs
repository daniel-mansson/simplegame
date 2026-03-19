using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// An ordered sequence of piece IDs. Maintains a cursor that advances on successful placement.
    /// </summary>
    public sealed class Deck : IDeck
    {
        private readonly IReadOnlyList<int> _pieceIds;
        private int _index;

        public Deck(IReadOnlyList<int> pieceIds)
        {
            _pieceIds = pieceIds ?? new List<int>();
            _index = 0;
        }

        /// <inheritdoc/>
        public int? Peek() => _index < _pieceIds.Count ? (int?)_pieceIds[_index] : null;

        /// <inheritdoc/>
        public int? PeekAt(int offset)
        {
            var i = _index + offset;
            return i < _pieceIds.Count ? (int?)_pieceIds[i] : null;
        }

        /// <inheritdoc/>
        public bool Advance()
        {
            if (_index < _pieceIds.Count)
                _index++;
            return _index < _pieceIds.Count;
        }

        /// <inheritdoc/>
        public bool IsEmpty => _index >= _pieceIds.Count;

        /// <inheritdoc/>
        public int Count => _pieceIds.Count;

        /// <inheritdoc/>
        public int RemainingCount => System.Math.Max(0, _pieceIds.Count - _index);
    }
}
