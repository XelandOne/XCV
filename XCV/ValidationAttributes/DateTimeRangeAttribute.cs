using System;
using System.ComponentModel.DataAnnotations;

namespace XCV.ValidationAttributes
{
    /// <summary>
    /// Checks if the annotated date is within datetime range.
    /// </summary>
    public class DateTimeRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dateTime = (DateTime?) value;
            if (dateTime == null)
                return ValidationResult.Success;

            if (dateTime.Value >= System.Data.SqlTypes.SqlDateTime.MaxValue.Value)
                return new ValidationResult("Das eingegebene Datum ist zu groß.");
            
            if (dateTime.Value <= System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                return new ValidationResult("Das eingegebene Datum ist zu klein.");
            
            return ValidationResult.Success;
        }
    }
}