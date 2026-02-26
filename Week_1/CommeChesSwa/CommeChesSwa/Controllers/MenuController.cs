using CommeChesSwa.Models;
using CommeChesSwa.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace CommeChesSwa.Controllers
{
    public class MenuController : Controller
    {
        private MenuRepository _menuRepository;

        public MenuController()
        {
            _menuRepository = new MenuRepository();
        }

        public IActionResult Index(string id)
        {
            List<Menu> menuLijst = _menuRepository.GetAll().ToList();

            Menu menu = _menuRepository.GetById(id);

            if (menu == null)
            {
                menu = _menuRepository.GetById("LUNCH");
            }

            DetailViewModel vm = new DetailViewModel();
            vm.Menu = menu;
            vm.AllMenus = _menuRepository.GetAll();
            return View(vm);
        }
    }
}
