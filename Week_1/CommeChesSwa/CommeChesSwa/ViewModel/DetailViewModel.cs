using CommeChesSwa.Models;

namespace CommeChesSwa.ViewModel
{
    public class DetailViewModel
    {
        public Menu Menu { get; set; }
        public IEnumerable<Menu> AllMenus { get; set; }
    }
}
