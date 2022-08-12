using System.Collections.Generic;

namespace XCV.Entities.Enums
{
    /// <summary>
    /// LanguageLevel Enum to indicate how well you can speak the language
    /// </summary>
    public enum LanguageLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Fluent = 4,
        Native = 5
    }

    /// <summary>
    /// Class to translate the LanguageLevels for the UI
    /// </summary>
    public static class LanguageLevelHelper
    {
        private static readonly Dictionary<LanguageLevel, string> FriendlyNamesGerman = new()
        {
            {LanguageLevel.Beginner, "Anfänger"},
            {LanguageLevel.Intermediate, "Fortgeschritter Anfänger"},
            {LanguageLevel.Advanced, "Fortgeschritten"},
            {LanguageLevel.Fluent, "Fließend"},
            {LanguageLevel.Native, "Muttersprache"}
        };

        /// <summary>
        /// Display enum as a string.
        /// </summary>
        /// <param name="languageLevel"></param>
        /// <returns></returns>
        public static string ToFriendlyString(LanguageLevel languageLevel)
        {
            return FriendlyNamesGerman[languageLevel];
        }
    }
}