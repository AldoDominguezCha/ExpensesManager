namespace ExpnesesManager.Models
{
    public class ObtainByMonthResult
    {
        public int Month { get; set; }
        public DateTime ReferenceDate { get; set; }
        public decimal Amount { get; set; }
        public OperationType OperationTypeId { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }

    }
}
