using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Models
{
    public class Category
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "The name is required")]
        [StringLength(maximumLength: 50, MinimumLength = 4, ErrorMessage = "The name length is not adecuate")]
        public string Name { get; set; }
        [Display(Name = "Operation Type")]
        public OperationType OperationTypeId { get; set; }
        public int UserId { get; set; }

    }
}
