namespace Application.Configuration
{
    /// <summary>
    /// Configuration for period durations in matches
    /// </summary>
    public class PeriodDurationConfiguration
    {
        public const string SectionName = "PeriodDurations";
        
        /// <summary>
        /// Duration in seconds for regular periods (1 and 2)
        /// Default: 900 seconds (15 minutes)
        /// </summary>
        public int RegularPeriodSeconds { get; set; } = 900;
        
        /// <summary>
        /// Duration in seconds for overtime period (3)
        /// Default: 300 seconds (5 minutes)
        /// </summary>
        public int OvertimePeriodSeconds { get; set; } = 300;
        
        /// <summary>
        /// Duration in seconds for shootout period (4)
        /// Default: 0 (no limit)
        /// </summary>
        public int ShootoutPeriodSeconds { get; set; } = 0;
    }
}

