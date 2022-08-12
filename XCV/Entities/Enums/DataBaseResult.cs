namespace XCV.Entities.Enums
{
    /// <summary>
    /// Database services return what action they did to the database,
    /// so the Managers and UI can react appropriately
    /// </summary>
    public enum DataBaseResult
    {
        /// <summary>
        /// Successfully updated in database.
        /// </summary>
        Updated = 0,
        /// <summary>
        /// Database execution failed.
        /// </summary>
        Failed = 1,
        /// <summary>
        /// Inserted data to database.
        /// </summary>
        Inserted = 2
    }
}