using PizzaPlace.Models;

namespace PizzaPlace.Repositories
{
    public interface IMenuRepository
    {
        Task<long> AddMenu(Menu menu);
        Task<Menu> GetMenu(long menuId);
        Task<List<Menu>> GetAllMenus();
        Task<MenuItem> GetMenuItemById(long itemId);
        Task<Menu> UpdateMenu(Menu menu);
        Task DeleteMenu(long menuId);

        Task<long> AddMenuItem(MenuItem item);
        Task<MenuItem> UpdateMenuItem(MenuItem item);
        Task DeleteMenuItem(long itemId);

    }
}
