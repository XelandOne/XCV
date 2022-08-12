using System.Collections.Generic;

namespace XCV.Entities.Enums
{
    /// <summary>
    /// RateCardLevel Enum for the employee profiles
    /// </summary>
    public enum RateCardLevel
    {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Level6 = 6,
        Level7 = 7,
        Level8 = 8
    }
    
    /// <summary>
    /// Class to format RateCardLevel into a nicer format
    /// </summary>
    public static class RateCardLevelHelper
    {
        /// <summary>
        /// Display enum as a string.
        /// </summary>
        /// <param name="rateCardLevel"></param>
        /// <returns></returns>
        public static string ToFriendlyString(RateCardLevel rateCardLevel)
        {
            return "Level " + (int) rateCardLevel;
        }
    }
}