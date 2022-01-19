using System.ComponentModel.DataAnnotations;

namespace ExpnesesManager.Models
{
    public class Transaction
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Transaction date")]
        [DataType(DataType.Date)]
        public DateTime OperationDate { get; set; } = DateTime.Today;
        public decimal Amount { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "The category id must be in the range of the int type")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [StringLength(maximumLength: 1000, ErrorMessage = "The description must be {1} characters long at most")]
        public string Description { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "The account id must be in the range of the int type")]
        [Display(Name = "Account")]
        public int AccountId { get; set; }
        [Display(Name = "Operation Type")]
        public OperationType OperationTypeId { get; set; } = OperationType.Income;
    }
}
