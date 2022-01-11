using ExpnesesManager.Models;
using ExpnesesManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpnesesManager.Controllers
{
    public class AccountsController : Controller
    {
        
        private readonly IAccountTypesRepository _accountTypesRepository;
        private readonly IUsersService _usersService;
        private readonly IAccountsRepository _accountsRepository;
        public AccountsController(IAccountTypesRepository accountTypesRepository, IUsersService usersService, IAccountsRepository accountsRepository)
        {
            _accountTypesRepository = accountTypesRepository;
            _usersService = usersService;
            _accountsRepository = accountsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _usersService.GetUserId();
            var accountsWithType = await _accountsRepository.SearchAccounts(userId);

            var model = accountsWithType
                .GroupBy(x => x.AccountType)
                .Select(group => new AccountsIndexViewModel
                {
                    AccountType = group.Key,
                    Accounts = group.AsEnumerable()
                }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var userId = _usersService.GetUserId();
            var model = new CreateAccountViewModel() { AccountTypes = await GetAccountTypes(userId) };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountViewModel account)
        {
            var userId = _usersService.GetUserId();
            var accountType = await _accountTypesRepository.GetAccountTypeById(account.AccountTypeId, userId);

            if (accountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            if (!ModelState.IsValid)
            {
                account.AccountTypes = await GetAccountTypes(userId);
                return View(account);
            }

            await _accountsRepository.Create(account);
            return RedirectToAction("Index");

        }

        private async Task<IEnumerable<SelectListItem>> GetAccountTypes(int userId)
        {
            var accountTypes = await _accountTypesRepository.GetAccountTypes(userId);
            return accountTypes.Select(x => new SelectListItem(x.Name, x.Id.ToString()));

        }

    }
}
