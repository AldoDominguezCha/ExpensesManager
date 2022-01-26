using AutoMapper;
using ExpnesesManager.Models;
using ExpnesesManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpnesesManager.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IUsersService _usersService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IMapper _mapper;
        private readonly IReportsService _reportsService;
        public TransactionsController(ITransactionsRepository transactionsRepository, 
            IUsersService usersService, IAccountsRepository accountsRepository,
            ICategoriesRepository categoriesRepository,
            IMapper mapper,
            IReportsService reportsService)
        {
            _transactionsRepository = transactionsRepository;
            _usersService = usersService;
            _accountsRepository = accountsRepository;
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
            _reportsService = reportsService;
        }

        public async Task<IActionResult> Index(int month, int year)
        {
            int userId = _usersService.GetUserId();

            var model = await _reportsService.ObtainDetailedTransactionsReportByUser(userId, month, year, ViewBag);

            return View(model);
        }

        public async Task<IActionResult> Weekly(int month, int year)
        {
            int userId = _usersService.GetUserId();

            IEnumerable<ObtainByWeekResult> transactionsByWeek = await _reportsService.GetWeeklyReport(userId, month, year, ViewBag);

            var grouped = transactionsByWeek.GroupBy(x => x.Week).
                Select(x => new ObtainByWeekResult()
                {
                    Week = x.Key,
                    Income = x.Where(x => x.OperationTypeId == OperationType.Income)
                        .Select(x => x.Amount).FirstOrDefault(),
                    Expenses = x.Where(x => x.OperationTypeId == OperationType.Expense)
                        .Select(x => x.Amount).FirstOrDefault(),

                }).ToList();

            if (year == 0 || month == 0)
            {
                var today = DateTime.Today;
                year = today.Year;
                month = today.Month;
            }

            var referenceDate = new DateTime(year, month, 1);
            var daysInMonth = Enumerable.Range(1, referenceDate.AddMonths(1).AddDays(-1).Day);
            var segmentedDays = daysInMonth.Chunk(7).ToList();

            for (int i = 0; i < segmentedDays.Count(); i++)
            {
                var week = i + 1;
                var startDate = new DateTime(year, month, segmentedDays[i].First());
                var endDate = new DateTime(year, month, segmentedDays[i].Last());

                var weeklyGroup = grouped.FirstOrDefault(x => x.Week == week);
                if (weeklyGroup is null)
                {
                    grouped.Add(new ObtainByWeekResult()
                    {
                        Week = week,
                        StartDate = startDate,
                        EndDate = endDate,
                    });
                } else
                {
                    weeklyGroup.StartDate = startDate;
                    weeklyGroup.EndDate = endDate;
                }
            }

            grouped = grouped.OrderByDescending(x => x.Week).ToList();

            var model = new WeeklyReportViewModel()
            {
                TransactionsByWeek = grouped,
                ReferenceDate = referenceDate
            };
            
            
            return View(model);

        }

        public async Task<IActionResult> Monthly(int year)
        {
            int userId = _usersService.GetUserId();
            
            year = year == 0 ? DateTime.Today.Year : year;

            var transactions = await _transactionsRepository.ObtainTransactionsByMonth(userId, year);

            var groupedTransactions = transactions.GroupBy(x => x.Month)
                .Select(x => new ObtainByMonthResult()
                {
                    Month = x.Key,
                    Income = x.Where(x => x.OperationTypeId == OperationType.Income)
                        .Select(x => x.Amount).FirstOrDefault(),
                    Expense = x.Where(x => x.OperationTypeId == OperationType.Expense)
                        .Select(x => x.Amount).FirstOrDefault()
                }).ToList();

            for (int month = 1; month <= 12; month++)
            {
                var transaction = groupedTransactions.FirstOrDefault(x => x.Month == month);
                var referenceDate = new DateTime(year, month, 1);

                if (transaction is null)
                    groupedTransactions.Add(new ObtainByMonthResult()
                    {
                        Month = month,
                        ReferenceDate = referenceDate,
                    });
                else
                    transaction.ReferenceDate = referenceDate;
            }

            groupedTransactions = groupedTransactions.OrderByDescending(x => x.Month).ToList();

            var model = new MonthlyReportViewModel()
            {
                TransactionsByMonth = groupedTransactions,
                Year = year
            };

            return View(model);
        }

        public IActionResult ExcelReport()
        {
            return View();
        }

        public IActionResult Calendar()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            int userId = _usersService.GetUserId();

            var model = new CreateTransactionViewModel();
            model.Accounts = await GetAccountsForUser(userId);
            model.Categories = await ObtainCategories(userId, model.OperationTypeId);
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionViewModel model)
        {
            int userId = _usersService.GetUserId();

            if(!ModelState.IsValid)
            {
                model.Accounts = await GetAccountsForUser(userId);
                model.Categories = await ObtainCategories(userId, model.OperationTypeId);
                return View(model);
            }

            var account = await _accountsRepository.GetAccountById(model.AccountId, userId);
            var category = await _categoriesRepository.GetCategoryById(model.CategoryId, userId);

            if (account is null || category is null) return RedirectToAction("NotFound", "Home");

            model.UserId = userId;

            if (model.OperationTypeId == OperationType.Expense)
            {
                model.Amount *= -1;
            }

            await _transactionsRepository.Create(model);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Update(int id, string returnUrl = null)
        {
            int userId = _usersService.GetUserId();

            var transaction = await _transactionsRepository.GetTransactionById(id, userId);

            if (transaction is null) return RedirectToAction("NotFound", "Home");

            var model = _mapper.Map<UpdateTransactionViewModel>(transaction);

            model.PreviousAmount = model.Amount;
            
            if (model.OperationTypeId == OperationType.Expense)
            {
                model.PreviousAmount = model.Amount * -1;
            }

            model.PreviousAccountId = transaction.AccountId;
            model.Categories = await ObtainCategories(userId, transaction.OperationTypeId);
            model.Accounts = await GetAccountsForUser(userId);
            model.ReturnUrl = returnUrl;
            return View(model);

        }
        
        [HttpPost]
        public async Task<IActionResult> Update(UpdateTransactionViewModel model)
        {
            int userId = _usersService.GetUserId();

            if (!ModelState.IsValid)
            {
                model.Accounts = await GetAccountsForUser(userId);
                model.Categories = await ObtainCategories(userId, model.OperationTypeId);
                return View(model);
            }

            var account = await _accountsRepository.GetAccountById(model.AccountId, userId);
            var category = await _categoriesRepository.GetCategoryById(model.CategoryId, userId);

            if (account is null || category is null) return RedirectToAction("NotFound", "Home");

            var transaction = _mapper.Map<Transaction>(model);

            if (model.OperationTypeId == OperationType.Expense)
            {
                transaction.Amount *= -1;
            }

            await _transactionsRepository.Update(transaction, model.PreviousAmount, model.PreviousAccountId);

            if(String.IsNullOrEmpty(model.ReturnUrl)) return RedirectToAction("Index");

            return LocalRedirect(model.ReturnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string returnUrl = null)
        {
            int userId = _usersService.GetUserId();
            var transaction = await _transactionsRepository.GetTransactionById(id, userId);

            if (transaction is null) return RedirectToAction("NotFound", "Home");

            await _transactionsRepository.DeleteTransaction(id);

            if (String.IsNullOrEmpty(returnUrl)) return RedirectToAction("Index");

            return LocalRedirect(returnUrl);

        }

        private async Task<IEnumerable<SelectListItem>> GetAccountsForUser(int userId)
        {
            var accounts = await _accountsRepository.SearchAccounts(userId);
            return accounts.Select(x => new SelectListItem(x.Name, x.Id.ToString()));

        }


        private async Task<IEnumerable<SelectListItem>> ObtainCategories(int userId, OperationType operationType)
        {
            var categories = await _categoriesRepository.GetCategoriesForUserByOperationType(userId, operationType);
            return categories.Select(x => new SelectListItem(x.Name, x.Id.ToString()));

        }

        [HttpPost]
        public async Task<IActionResult> ObtainCategories([FromBody] OperationType operationType)
        {
            int userId = _usersService.GetUserId();
            var categories = await ObtainCategories(userId, operationType);

            return Ok(categories);

        }

    }
}
