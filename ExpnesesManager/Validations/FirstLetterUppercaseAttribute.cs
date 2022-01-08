using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Validations
{
    public class FirstLetterUppercaseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()) || String.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            var firstLetter = value.ToString()[0].ToString();

            if (firstLetter != firstLetter.ToUpper()) return new ValidationResult("The first letter must be uppercase");

            return ValidationResult.Success;
        }
    }
}
