using ExpnesesManager.Validations;
using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Models
{
    public class Account
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The name is required")]
        [StringLength(maximumLength: 50, ErrorMessage = "The maximum length for the account name is 50 characters")]
        [FirstLetterUppercase]
        public string Name { get; set; }
        [Display(Name = "Account Type")]
        public int AccountTypeId { get; set; }
        public decimal Balance { get; set; }
        [StringLength(maximumLength: 1000)]
        public string AccountDescription { get; set; }
        public string AccountType { get; set; }
    }
}
