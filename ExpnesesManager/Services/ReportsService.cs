using ExpnesesManager.Models;

namespace ExpnesesManager.Services
{

    public interface IReportsService
    {
        Task<IEnumerable<ObtainByWeekResult>> GetWeeklyReport(int userId, int month, int year, dynamic ViewBag);
        Task<DetailedTransactionsReport> ObtainDetailedTransactionsReportByAccount(int userId, int accountId, int month, int year, dynamic ViewBag);
        Task<DetailedTransactionsReport> ObtainDetailedTransactionsReportByUser(int userId, int month, int year, dynamic ViewBag);
    }

    public class ReportsService : IReportsService
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly HttpContext _httpContext;
        public ReportsService(ITransactionsRepository transactionsRepository, IHttpContextAccessor httpContextAccessor)
        {
            _transactionsRepository = transactionsRepository;
            _httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<DetailedTransactionsReport> ObtainDetailedTransactionsReportByUser(int userId, int month, int year, dynamic ViewBag)
        {

            (DateTime startDate, DateTime endDate) = GenerateSartEndDate(month, year);

            var parameter = new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            var transactions = await _transactionsRepository.ObtainTransactionsByUserId(parameter);

            var model = GenerateDetailedTransactionsReport(startDate, endDate, transactions);
            ViewBagAssignValues(ViewBag, startDate);

            return model;

        }

        public async Task<IEnumerable<ObtainByWeekResult>> GetWeeklyReport(int userId, int month, int year, dynamic ViewBag)
        {
            (DateTime startDate, DateTime endDate) = GenerateSartEndDate(month, year);

            var parameter = new GetTransactionsByUserParameter()
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            ViewBagAssignValues(ViewBag, startDate);

            var model = await _transactionsRepository.ObtainTransactionsByWeek(parameter);

            return model;

        }


        public async Task<DetailedTransactionsReport> ObtainDetailedTransactionsReportByAccount(int userId, int accountId, int month, int year, dynamic ViewBag)
        {

            (DateTime startDate, DateTime endDate) = GenerateSartEndDate(month, year);

            var obtainTransactionsByAccount = new ObtainTransactionsByAccount()
            {
                AccountId = accountId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            var transactions = await _transactionsRepository.ObtainTransactionsByAccountId(obtainTransactionsByAccount);

            var model = GenerateDetailedTransactionsReport(startDate, endDate, transactions);
            ViewBagAssignValues(ViewBag, startDate);

            return model;

        }

        private (DateTime startDate, DateTime endDate) GenerateSartEndDate(int month, int year)
        {
            DateTime StartDate;
            DateTime EndDate;

            if (month <= 0 || month > 12 || year <= 1900)
            {
                var today = DateTime.Today;
                StartDate = new DateTime(today.Year, today.Month, 1);
            }
            else
            {
                StartDate = new DateTime(year, month, 1);
            }

            EndDate = StartDate.AddMonths(1).AddDays(-1);

            return (StartDate, EndDate);
        }

        private DetailedTransactionsReport GenerateDetailedTransactionsReport(DateTime startDate, DateTime endDate, IEnumerable<Transaction> transactions)
        {
            var model = new DetailedTransactionsReport();

            var transactionsByDate = transactions.OrderByDescending(x => x.OperationDate)
                .GroupBy(x => x.OperationDate).Select(group => new DetailedTransactionsReport.TransactionsByDate()
                {
                    TransactionDate = group.Key,
                    Transactions = group.AsEnumerable()
                });

            model.GroupedTransactions = transactionsByDate;
            model.StartDate = startDate;
            model.EndDate = endDate;

            return model;

        }

        private void ViewBagAssignValues(dynamic ViewBag, DateTime startDate)
        {
            ViewBag.PreviousMonth = startDate.AddMonths(-1).Month;
            ViewBag.PreviousYear = startDate.AddMonths(-1).Year;
            ViewBag.NextMonth = startDate.AddMonths(1).Month;
            ViewBag.NextYear = startDate.AddMonths(1).Year;
            ViewBag.ReturnUrl = _httpContext.Request.Path + _httpContext.Request.QueryString;
        }

    }
}
