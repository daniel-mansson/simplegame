using System.Collections.Generic;

namespace SimpleGame.Puzzle
{
    /// <summary>
    /// Runtime board state. Tracks placed pieces and enforces the neighbor-presence placement rule.
    /// Uses a HashSet for O(1) neighbor lookup on every CanPlace call.
    /// </summary>
    public sealed class PuzzleBoard : IPuzzleBoard
    {
        private readonly HashSet<int> _placedIds = new HashSet<int>();
        private readonly Dictionary<int, IReadOnlyList<int>> _neighborMap;

        /// <summary>
        /// Construct a board from the level's piece list.
        /// Builds an internal neighbor map for fast lookup.
        /// </summary>
        public PuzzleBoard(IReadOnlyList<IPuzzlePiece> pieces)
        {
            _neighborMap = new Dictionary<int, IReadOnlyList<int>>(pieces.Count);
            foreach (var piece in pieces)
                _neighborMap[piece.Id] = piece.NeighborIds;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<int> PlacedIds => _placedIds;

        /// <inheritdoc/>
        /// <remarks>
        /// A piece is placeable when at least one of its declared neighbors is already on the board.
        /// Pieces with no neighbors (isolated) can never be placed via this rule — use seed placement instead.
        /// </remarks>
        public bool CanPlace(int pieceId)
        {
            if (!_neighborMap.TryGetValue(pieceId, out var neighbors))
                return false;

            foreach (var neighborId in neighbors)
            {
                if (_placedIds.Contains(neighborId))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Place(int pieceId)
        {
            return _placedIds.Add(pieceId);
        }
    }
}
