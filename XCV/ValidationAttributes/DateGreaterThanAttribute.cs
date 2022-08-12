using System;
using System.ComponentModel.DataAnnotations;
using XCV.Entities;

namespace XCV.ValidationAttributes
{
    /// <summary>
    /// Validation attribute for checking if a date is greater than another property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private string DateToCompareFieldName { get; set; }
        /// <summary>
        /// Validation attribute. Checks whether given date is greater than date given in the field 'dateToCompareFieldName'
        /// </summary>
        /// <param name="dateToCompareFieldName"></param>
        public DateGreaterThanAttribute(string dateToCompareFieldName)
        {
            DateToCompareFieldName = dateToCompareFieldName;
        }
        /// <summary>
        /// Provides validation logic 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns>True when date is greater than date given in the field 'dateToCompareFieldName'</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectType.GetProperty(DateToCompareFieldName)?.GetValue(validationContext.ObjectInstance, null) == null)
                return ValidationResult.Success;
            
            if (value is not DateTime laterDate) return ValidationResult.Success;
            if (validationContext.ObjectType.GetProperty(DateToCompareFieldName)?.GetValue(validationContext.ObjectInstance, null) is not DateTime earlierDate) 
                return new ValidationResult("Coding Error: Wrong FieldName");

            return laterDate >= earlierDate ? ValidationResult.Success :
                new ValidationResult("Enddatum kann nicht vor dem Startdatum liegen ");
        }
    }
}