using ExpnesesManager.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Models
{
    public class AccountType //: IValidatableObject
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "You must provide a name for the account type")]
        [StringLength(maximumLength : 50, MinimumLength = 4, ErrorMessage = "The length for the account type name must be between {2} and {1} characters")]
        [Display(Name = "Account type name")]
        [FirstLetterUppercase]
        [Remote(action: "VerifyAccountTypeExists", controller: "AccountTypes")]
        public string Name { get; set; }
        public int UserId { get; set; }
        public int SortOrder { get; set; }

        
        /* Example on how to create a validation method for the entire model, here it's only applying to the name
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name != null && Name.Length > 0)
            {
                var firstLetter = Name[0].ToString();
                var secondLetter = Name[1].ToString();

                if (firstLetter != firstLetter.ToUpper() || secondLetter != secondLetter.ToUpper())
                    yield return new ValidationResult(
                        "The first two letters must be uppercase because of important reasons",
                        new[] { nameof(Name) });

            }
        }*/
    }
}
