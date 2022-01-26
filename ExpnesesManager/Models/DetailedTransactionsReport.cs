namespace ExpnesesManager.Models
{
    public class DetailedTransactionsReport
    {

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<TransactionsByDate> GroupedTransactions { get; set; }

        public decimal IncomeBalance => GroupedTransactions.Sum(x => x.IncomeBalance);
        public decimal ExpensesBalance => GroupedTransactions.Sum(x => x.ExpensesBalance);
        public decimal Total => IncomeBalance - ExpensesBalance;

        public class TransactionsByDate
        {

            public DateTime TransactionDate { get; set; }

            public IEnumerable<Transaction> Transactions { get; set; }

            public decimal IncomeBalance => 
                Transactions.Where(x => x.OperationTypeId == OperationType.Income)
                .Sum(x => x.Amount);

            public decimal ExpensesBalance =>
                Transactions.Where(x => x.OperationTypeId == OperationType.Expense)
                .Sum(x => x.Amount);

        }

    }
}
