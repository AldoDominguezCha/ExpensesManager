namespace ExpnesesManager.Models
{
    public class ObtainByWeekResult
    {

        public int Week { get; set; }
        public decimal Amount { get; set; }
        public OperationType OperationTypeId { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
