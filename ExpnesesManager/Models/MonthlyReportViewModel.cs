namespace ExpnesesManager.Models
{
    public class MonthlyReportViewModel
    {

        public IEnumerable<ObtainByMonthResult> TransactionsByMonth { get; set; }
        public decimal Income => TransactionsByMonth.Sum(x => x.Income);
        public decimal Expenses => TransactionsByMonth.Sum(x => x.Expense);
        public decimal Total => Income - Expenses;
        public int Year { get; set; }

    }
}
