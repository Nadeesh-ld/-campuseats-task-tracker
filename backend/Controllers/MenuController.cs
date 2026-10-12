using CampusEats.Api.Dtos;
using CampusEats.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CampusEats.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _svc;

    public MenuController(IMenuService svc) => _svc = svc;


    // Anyone can view menu
    [HttpGet]
    public ActionResult<IEnumerable<MenuItemDto>> GetAll()
        => Ok(_svc.GetAll());



    // Anyone can view single item
    [HttpGet("{id}")]
    public ActionResult<MenuItemDto> GetById(int id)
    {
        var item = _svc.GetById(id);

        return item is null 
            ? NotFound() 
            : Ok(item);
    }



    // Admins only - Create menu item
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public ActionResult<MenuItemDto> Create(
        [FromBody] CreateMenuItemDto dto)
    {
        var created = _svc.Create(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }



    // Admins only - Update menu item
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(
        int id,
        [FromBody] CreateMenuItemDto dto)
    {
        var ok = _svc.Update(id, dto);

        return ok 
            ? NoContent() 
            : NotFound();
    }



    // Admins only - Delete menu item
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var ok = _svc.Delete(id);

        return ok 
            ? NoContent() 
            : NotFound();
    }
}