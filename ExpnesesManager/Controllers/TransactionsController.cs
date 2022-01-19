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
        public TransactionsController(ITransactionsRepository transactionsRepository, 
            IUsersService usersService, IAccountsRepository accountsRepository,
            ICategoriesRepository categoriesRepository,
            IMapper mapper)
        {
            _transactionsRepository = transactionsRepository;
            _usersService = usersService;
            _accountsRepository = accountsRepository;
            _categoriesRepository = categoriesRepository;
            _mapper = mapper;
        }

        public IActionResult Index()
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
        public async Task<IActionResult> Update(int id)
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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = _usersService.GetUserId();
            var transaction = await _transactionsRepository.GetTransactionById(id, userId);

            if (transaction is null) return RedirectToAction("NotFound", "Home");

            await _transactionsRepository.DeleteTransaction(id);

            return RedirectToAction("Index");

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
