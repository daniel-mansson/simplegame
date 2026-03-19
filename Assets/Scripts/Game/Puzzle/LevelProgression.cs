namespace SimpleGame.Game.Puzzle
{
    /// <summary>
    /// Maps a 1-based level ID to a grid layout.
    ///
    /// Progression pattern — alternate between widening columns and rows,
    /// growing one dimension at a time:
    ///
    ///   L01  3×3  =  9 pieces
    ///   L02  4×3  = 12 pieces
    ///   L03  4×4  = 16 pieces
    ///   L04  5×4  = 20 pieces
    ///   L05  5×5  = 25 pieces
    ///   L06  6×5  = 30 pieces
    ///   L07  6×6  = 36 pieces
    ///   L08  7×6  = 42 pieces
    ///   L09  7×7  = 49 pieces
    ///   L10  8×7  = 56 pieces
    ///   ...and so on (cols=floor((level+4)/2)+2, rows=floor((level+3)/2)+2)
    ///
    /// Levels beyond the explicit table keep growing by one dimension per step.
    /// </summary>
    public static class LevelProgression
    {
        public readonly struct GridSize
        {
            public readonly int Rows;
            public readonly int Cols;
            public GridSize(int rows, int cols) { Rows = rows; Cols = cols; }
        }

        /// <summary>Returns the grid dimensions for a given 1-based level ID.</summary>
        public static GridSize GetGridSize(int levelId)
        {
            // Clamp negative/zero to level 1
            if (levelId < 1) levelId = 1;

            // Level 1 starts at 3×3.
            // Each level adds one dimension, alternating: cols first, then rows.
            // So even step → cols grows, odd step → rows grows.
            //
            // Step 0 (L1): rows=3, cols=3
            // Step 1 (L2): rows=3, cols=4
            // Step 2 (L3): rows=4, cols=4
            // Step 3 (L4): rows=4, cols=5
            // Step 4 (L5): rows=5, cols=5
            // ...
            int step = levelId - 1;           // 0-based
            int base_ = 3;
            int rows = base_ + step / 2;      // grows every 2 steps
            int cols = base_ + (step + 1) / 2; // cols leads by one step

            return new GridSize(rows, cols);
        }

        /// <summary>Total piece count for a level (rows × cols).</summary>
        public static int PieceCount(int levelId) { var g = GetGridSize(levelId); return g.Rows * g.Cols; }
    }
}
