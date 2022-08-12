using System.Collections.Generic;

namespace XCV.Entities.Enums
{
    /// <summary>
    /// HardSkillLevel Enum to indicate a person´s experience of a hardSkill
    /// </summary>
    public enum HardSkillLevel
    {
        HobbyUse = 1,
        ProductiveUse = 2,
        RegularUse = 3,
        Expert = 4
    }

    /// <summary>
    /// Class to translate the HardSkillLevel for the UI 
    /// </summary>
    public static class HardSkillLevelHelper
    {
        private static readonly Dictionary<HardSkillLevel, string> FriendlyNamesGerman =
            new Dictionary<HardSkillLevel, string>
            {
                {HardSkillLevel.HobbyUse, "Hobby Projekt"},
                {HardSkillLevel.ProductiveUse, "Produktiv eingesetzt"},
                {HardSkillLevel.RegularUse, "Regelmäßige Nutzung"},
                {HardSkillLevel.Expert, "Experte"},
            };

        /// <summary>
        /// Display enum as a string.
        /// </summary>
        /// <param name="hardSkillLevel"></param>
        /// <returns></returns>
        public static string ToFriendlyString(HardSkillLevel hardSkillLevel)
        {
            return FriendlyNamesGerman[hardSkillLevel];
        }
    }
}