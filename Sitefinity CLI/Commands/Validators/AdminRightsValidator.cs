using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Sitefinity_CLI.Commands.Validators
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class AdminRightsValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var isAdmin = (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                           .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                return new ValidationResult("You must be administrator in order to run this command!");
            }

            return ValidationResult.Success;
        }
    }
}
