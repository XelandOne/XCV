using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using XCV.Data;

namespace XCV.ValidationAttributes
{
    /// <summary>
    /// Check if user name is already in use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class UniqueUsernameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Der Username muss darf nicht leer sein.");
            }

            var username = (string) value;

            var employeeService = validationContext.GetService<IEmployeeService>()!;

            var usernameAlreadyExists = employeeService.GetEmployee(username).Result != null;
            
            return usernameAlreadyExists ? new ValidationResult("Der Username ist bereits vergeben.") : ValidationResult.Success;
        }
    }
}