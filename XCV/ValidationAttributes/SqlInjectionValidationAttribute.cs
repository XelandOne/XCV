using System;
using System.ComponentModel.DataAnnotations;

namespace XCV.ValidationAttributes
{
    /// <summary>
    /// Check if input is a sql query. Not working!! :)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SqlInjectionValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Darf nicht null sein.");
            }

            var input = (string) value;

            if (input.Contains("drop table", StringComparison.OrdinalIgnoreCase) ||
                input.Contains("select *", StringComparison.OrdinalIgnoreCase))
            {
                var random = new Random();
                return new ValidationResult($"({random.Next(0, 156)} row(s) affected) (nothing happened - nice try)");
            }
            
            return ValidationResult.Success;
        }
    }
}