namespace SimpleGame.Game.InGame
{
    /// <summary>
    /// Actions that can occur during in-game play.
    /// PlaceCorrect and PlaceIncorrect are player inputs.
    /// Win and Lose are resolved outcomes (automatic, based on game state).
    /// </summary>
    public enum InGameAction
    {
        PlaceCorrect,
        PlaceIncorrect,
        Win,
        Lose
    }
}
