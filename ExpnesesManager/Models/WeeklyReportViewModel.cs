namespace ExpnesesManager.Models
{
    public class WeeklyReportViewModel
    {

        public decimal Income => TransactionsByWeek.Sum(x => x.Income);
        public decimal Expenses => TransactionsByWeek.Sum(x => x.Expenses);
        public decimal Total => Income - Expenses;
        public DateTime ReferenceDate { get; set; }
        public IEnumerable<ObtainByWeekResult> TransactionsByWeek { get; set; }
    }
}
