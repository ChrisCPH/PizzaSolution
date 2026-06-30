using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    private const string StandardMenuTitle = "Standard Menu";
    private const string LunchMenuTitle = "Lunch Menu";

    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<Menu> GetMenu(DateTimeOffset menuDate)
    {
        var hour = menuDate.UtcDateTime.Hour;
        var title = hour >= 11 && hour < 14 ? LunchMenuTitle : StandardMenuTitle;

        var menus = await _menuRepository.GetAllMenus();
        var menu = menus.FirstOrDefault(m => m.Title == title);

        if (menu == null)
            throw new PizzaException($"Menu '{title}' not found.");

        return menu;
    }

    public Task<long> AddMenu(Menu menu) => _menuRepository.AddMenu(menu);
    public Task<Menu> GetMenuById(long menuId) => _menuRepository.GetMenu(menuId);
    public Task<List<Menu>> GetAllMenus() => _menuRepository.GetAllMenus();
    public Task<Menu> UpdateMenu(Menu menu) => _menuRepository.UpdateMenu(menu);
    public Task DeleteMenu(long menuId) => _menuRepository.DeleteMenu(menuId);

    public Task<long> AddMenuItem(MenuItem item) => _menuRepository.AddMenuItem(item);
    public Task<MenuItem> UpdateMenuItem(MenuItem item) => _menuRepository.UpdateMenuItem(item);
    public Task DeleteMenuItem(long itemId) => _menuRepository.DeleteMenuItem(itemId);
}