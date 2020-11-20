using System.ComponentModel.DataAnnotations;

namespace Sitefinity_CLI.Commands.Validators
{
    internal class UpgradeVersionValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string versionAsString = value as string;

            if (!string.IsNullOrEmpty(versionAsString) && versionAsString.Contains('.'))
            {
                string majorVersion = versionAsString.Substring(0, versionAsString.IndexOf('.'));
                int majorVersionNumber;
                int.TryParse(majorVersion, out majorVersionNumber);

                if (majorVersionNumber >= 12)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("The version you are trying to upgrade to is not valid. Please specify version >= 12.0.7000");
        }
    }
}
