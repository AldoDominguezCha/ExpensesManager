using AutoMapper;
using ExpnesesManager.Models;
using ExpnesesManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace ExpnesesManager.Controllers
{
    public class AccountsController : Controller
    {
        
        private readonly IAccountTypesRepository _accountTypesRepository;
        private readonly IUsersService _usersService;
        private readonly IAccountsRepository _accountsRepository;
        private readonly IMapper _mapper;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IReportsService _reportsService;
        public AccountsController(IAccountTypesRepository accountTypesRepository, IUsersService usersService, IAccountsRepository accountsRepository, IMapper mapper, ITransactionsRepository transactionsRepository, IReportsService reportsService)
        {
            _accountTypesRepository = accountTypesRepository;
            _usersService = usersService;
            _accountsRepository = accountsRepository;
            _mapper = mapper;
            _transactionsRepository = transactionsRepository;
            _reportsService = reportsService;
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

        public async Task<IActionResult> Detail(int id, int month, int year)
        {

            var userId = _usersService.GetUserId();

            var account = await _accountsRepository.GetAccountById(id, userId);
            if (account is null) return RedirectToAction("NotFound", "Home");

            ViewBag.Account = account.Name;

            var model = await _reportsService.ObtainDetailedTransactionsReportByAccount(userId, account.Id, month, year, ViewBag);

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

        public async Task<IActionResult> Edit(int id)
        {
            int userId = _usersService.GetUserId();
            var account = await _accountsRepository.GetAccountById(id, userId);

            if (account is null)
                return RedirectToAction("NotFound", "Home");

            var model = _mapper.Map<CreateAccountViewModel>(account);

            model.AccountTypes = await GetAccountTypes(userId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateAccountViewModel editAccount)
        {
            int userId = _usersService.GetUserId();
            var account = await _accountsRepository.GetAccountById(editAccount.Id, userId);
            var accountType = await _accountTypesRepository.GetAccountTypeById(editAccount.AccountTypeId, userId);

            if (account is null || accountType is null) return RedirectToAction("NotFound", "Home");

            await _accountsRepository.UpdateAccount(editAccount);

            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = _usersService.GetUserId();
            var account = await _accountsRepository.GetAccountById(id, userId);

            if (account is null) return RedirectToAction("NotFound", "Home");

            return View(account);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            int userId = _usersService.GetUserId();
            var account = await _accountsRepository.GetAccountById(id, userId);

            if (account is null) return RedirectToAction("NotFound", "Home");

            await _accountsRepository.DeleteAccount(id);

            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> GetAccountTypes(int userId)
        {
            var accountTypes = await _accountTypesRepository.GetAccountTypes(userId);
            return accountTypes.Select(x => new SelectListItem(x.Name, x.Id.ToString()));

        }

    }
}
