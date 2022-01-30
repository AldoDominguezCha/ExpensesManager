using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "You must provide an email")]
        [EmailAddress(ErrorMessage = "You must provide a valid email address for your new account")]
        public string Email { get; set; }
        [Required(ErrorMessage = "You must provide a password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
