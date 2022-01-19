namespace ExpnesesManager.Models
{
    public class UpdateTransactionViewModel : CreateTransactionViewModel
    {

        public decimal PreviousAmount { get; set; }
        public int PreviousAccountId { get; set; }

    }
}
