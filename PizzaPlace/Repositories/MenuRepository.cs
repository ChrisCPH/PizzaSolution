using Microsoft.EntityFrameworkCore;
using PizzaPlace.DB;
using PizzaPlace.Models;

namespace PizzaPlace.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<long> AddMenu(Menu menu)
        {
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
            return menu.Id;
        }

        public async Task<Menu> GetMenu(long menuId)
        {
            var menu = await _context.Menus
                .Include(m => m.Items)
                    .ThenInclude(i => i.PizzaRecipe)
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (menu == null)
                throw new PizzaException($"Menu {menuId} not found.");

            return menu;
        }

        public async Task<List<Menu>> GetAllMenus()
        {
            return await _context.Menus
                .Include(m => m.Items)
                    .ThenInclude(i => i.PizzaRecipe)
                .ToListAsync();
        }

        public async Task<Menu> UpdateMenu(Menu menu)
        {
            var existing = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == menu.Id);

            if (existing == null)
                throw new PizzaException($"Menu {menu.Id} not found.");

            existing.Title = menu.Title;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteMenu(long menuId)
        {
            var existing = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == menuId);

            if (existing == null)
                throw new PizzaException($"Menu {menuId} not found.");

            _context.Menus.Remove(existing);
            await _context.SaveChangesAsync();
        }

        public async Task<long> AddMenuItem(MenuItem item)
        {
            if (!await _context.Menus.AnyAsync(m => m.Id == item.MenuId))
                throw new PizzaException($"Menu {item.MenuId} not found.");

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            return item.Id;
        }

        public async Task<MenuItem> UpdateMenuItem(MenuItem item)
        {
            var existing = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existing == null)
                throw new PizzaException($"MenuItem {item.Id} not found.");

            existing.Description = item.Description;
            existing.Price = item.Price;
            existing.SoldOut = item.SoldOut;
            existing.PizzaRecipeId = item.PizzaRecipeId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteMenuItem(long itemId)
        {
            var existing = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (existing == null)
                throw new PizzaException($"MenuItem {itemId} not found.");

            _context.MenuItems.Remove(existing);
            await _context.SaveChangesAsync();
        }

        public async Task<MenuItem> GetMenuItemById(long itemId)
        {
            var item = await _context.MenuItems
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
                throw new PizzaException($"MenuItem {itemId} not found.");

            return item;
        }
    }
}