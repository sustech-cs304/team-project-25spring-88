/// <summary>
/// A struct that stores the result of a player in a multiplayer racing game.
/// <para>
/// Contains the player's name and their finish time for ranking purposes.
/// </para>
/// </summary>
public struct RaceResult
{
    /// <summary>
    /// The name of the player.
    /// </summary>
    public string playerName;

    /// <summary>
    /// The time taken to finish the race in seconds.
    /// </summary>
    public float finishTime;

    /// <summary>
    /// Initializes a new RaceResult with the specified player name and finish time.
    /// </summary>
    /// <param name="name">The player's name.</param>
    /// <param name="time">The player's finish time in seconds.</param>
    public RaceResult(string name, float time)
    {
        playerName = name;
        finishTime = time;
    }
}