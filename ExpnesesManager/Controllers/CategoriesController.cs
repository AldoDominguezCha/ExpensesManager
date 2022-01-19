using ExpnesesManager.Models;
using ExpnesesManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpnesesManager.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IUsersService _usersService;
        public CategoriesController(ICategoriesRepository categoriesRepository, IUsersService usersService)
        {
            _categoriesRepository = categoriesRepository;
            _usersService = usersService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = _usersService.GetUserId();
            IEnumerable<Category> categories = await _categoriesRepository.GetCategoriesForUser(userId);

            return View(categories);
        }
        
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            int userId = _usersService.GetUserId();
            category.UserId = userId;
            await _categoriesRepository.CreateCategory(category);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _usersService.GetUserId();
            Category category = await _categoriesRepository.GetCategoryById(id, userId);

            if (category is null) return RedirectToAction("NotFound", "Home");

            return View(category);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(Category updateCategory)
        {
            if (!ModelState.IsValid)
            {
                return View(updateCategory);
            }

            var userId = _usersService.GetUserId();
            Category category = await _categoriesRepository.GetCategoryById(updateCategory.Id, userId);

            if (category is null) return RedirectToAction("NotFound", "Home");

            updateCategory.UserId = userId;
            await _categoriesRepository.UpdateCategory(updateCategory);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {

            var userId = _usersService.GetUserId();
            Category category = await _categoriesRepository.GetCategoryById(id, userId);

            if (category is null) return RedirectToAction("NotFound", "Home");

            return View(category);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = _usersService.GetUserId();
            Category category = await _categoriesRepository.GetCategoryById(id, userId);

            if (category is null) return RedirectToAction("NotFound", "Home");

            await _categoriesRepository.DeleteCategoryById(id);
            return RedirectToAction("Index");
        }

    }
}
