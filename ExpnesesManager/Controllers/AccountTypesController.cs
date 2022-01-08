using Dapper;
using ExpnesesManager.Models;
using ExpnesesManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Controllers
{
    public class AccountTypesController : Controller
    {
        private readonly IAccountTypesRepository _accountTypesRepository;
        private readonly IUsersService _usersService;   
        public AccountTypesController(IAccountTypesRepository accountTypesRepository, IUsersService usersService)
        {
            _accountTypesRepository = accountTypesRepository;
            _usersService = usersService;
        }

        public async Task<IActionResult> Index() 
        {
            int userId = _usersService.GetUserId();
            var accountTypes = await _accountTypesRepository.GetAccountTypes(userId);
            return View(accountTypes);
        }
        
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AccountType accountType)
        {

            accountType.UserId = _usersService.GetUserId();

            if (await _accountTypesRepository.AccountTypeExists(accountType.Name, accountType.UserId))
            {
                ModelState.AddModelError(nameof(accountType.Name), $"This value is already registered as an account type");
            }


            if (!ModelState.IsValid)
            {
                return View(accountType);
            }

            await _accountTypesRepository.Create(accountType);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int userId = _usersService.GetUserId();
            var accountType = await _accountTypesRepository.GetAccountTypeById(id, userId);
            
            if (accountType is null)
            {
                return RedirectToAction("NotFound", "Home");

            }

            return View(accountType);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(AccountType accountType)
        {
            var userId = _usersService.GetUserId();
            var accountTypeExists = await _accountTypesRepository.GetAccountTypeById(accountType.Id, userId);

            if (accountTypeExists is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _accountTypesRepository.UpdateAccountType(accountType);
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = _usersService.GetUserId();
            var accountType = await _accountTypesRepository.GetAccountTypeById(id, userId);
            
            if (accountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            return View(accountType);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccountType(int id)
        {
            int userId = _usersService.GetUserId();
            var accountType = await _accountTypesRepository.GetAccountTypeById(id, userId);

            if (accountType is null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            await _accountTypesRepository.DeleteAccountType(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyAccountTypeExists(string name)
        {
            int userId = _usersService.GetUserId();
            bool alreadyExists = await _accountTypesRepository.AccountTypeExists(name, userId);

            if (alreadyExists)
            {
                return Json($"'{name}' is already registered as an account type name for this user");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> OrderAccountTypes([FromBody] int[] ids)
        {
            int userId = _usersService.GetUserId();
            var accountTypes = await _accountTypesRepository.GetAccountTypes(userId);
            var accountTypesIds = accountTypes.Select(x => x.Id);

            var IdsNotFromUser = ids.Except(accountTypesIds).ToList();

            if (IdsNotFromUser.Count > 0)
            {
                return Forbid();
            }

            var orderedAccountTypes = ids.Select((value, index) => new AccountType() { Id = value, SortOrder = index + 1 }).AsEnumerable();

            await _accountTypesRepository.SetAccountTypesOrder(orderedAccountTypes);

            return Ok();
        }

    }
}
