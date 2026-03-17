namespace SimpleGame.Game.MainMenu
{
    /// <summary>
    /// Data struct for displaying a restorable object on the main screen.
    /// Passed from presenter to view — view has no service references.
    /// </summary>
    public struct ObjectDisplayData
    {
        public string Name;
        public string Progress; // e.g. "2/3" or "Complete"
        public bool IsBlocked;
        public bool IsComplete;
        public int CostPerStep;
    }
}
