namespace Domain.Enums.Hockey
{
    /// <summary>
    /// Represents the types of penalties in hocke
    /// </summary>
    public enum HockeyPenaltyType
    {
        /// <summary>
        /// No penalty assigned
        /// </summary>
        None = 0,

        /// <summary>
        /// 2-minute minor penalty
        /// </summary>
        Minor = 1,

        //Onko tarpeellinen?
        /// <summary>
        /// 4-minute double minor penalty
        /// </summary>
        DoubleMinor = 2,

        /// <summary>
        /// 5-minute major penalty
        /// </summary>
        Major = 3,

        /// <summary>
        /// 10-minute misconduct penalty
        /// </summary>
        Misconduct = 4,

        /// <summary>
        /// Player is ejected from the game
        /// </summary>
        GameMisconduct = 5,

        /// <summary>
        /// Technical penalty (e.g., too many players on the field)
        /// </summary>
        Technical = 6,

        /// <summary>
        /// Penalty shot awarded
        /// </summary>
        PenaltyShot = 7


    }
}
