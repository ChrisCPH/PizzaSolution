using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers
{
    [Route("api/menu")]
    public class MenuController(TimeProvider timeProvider, IMenuService menuService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            return Ok(await menuService.GetMenu(timeProvider.GetUtcNow()));
        }

        [HttpGet("{menuId}")]
        public async Task<IActionResult> GetMenuById(long menuId)
        {
            return Ok(await menuService.GetMenuById(menuId));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllMenus()
        {
            return Ok(await menuService.GetAllMenus());
        }

        [HttpPost]
        public async Task<IActionResult> AddMenu([FromBody] Menu menu)
        {
            var id = await menuService.AddMenu(menu);
            return CreatedAtAction(nameof(GetMenuById), new { menuId = id }, id);
        }

        [HttpPut("{menuId}")]
        public async Task<IActionResult> UpdateMenu(long menuId, [FromBody] Menu menu)
        {
            if (menuId != menu.Id)
                return BadRequest("Route menuId does not match body Id.");

            var updated = await menuService.UpdateMenu(menu);
            return Ok(updated);
        }

        [HttpDelete("{menuId}")]
        public async Task<IActionResult> DeleteMenu(long menuId)
        {
            await menuService.DeleteMenu(menuId);
            return NoContent();
        }

        [HttpPost("item")]
        public async Task<IActionResult> AddMenuItem([FromBody] MenuItem item)
        {
            var id = await menuService.AddMenuItem(item);
            return CreatedAtAction(nameof(GetMenuById), new { menuId = item.MenuId }, id);
        }

        [HttpPut("item/{itemId}")]
        public async Task<IActionResult> UpdateMenuItem(long itemId, [FromBody] MenuItem item)
        {
            if (itemId != item.Id)
                return BadRequest("Route itemId does not match body Id.");

            var updated = await menuService.UpdateMenuItem(item);
            return Ok(updated);
        }

        [HttpDelete("item/{itemId}")]
        public async Task<IActionResult> DeleteMenuItem(long itemId)
        {
            await menuService.DeleteMenuItem(itemId);
            return NoContent();
        }
    }
}