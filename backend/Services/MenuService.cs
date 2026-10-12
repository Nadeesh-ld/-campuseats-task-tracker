using CampusEats.Api.Data; 
using CampusEats.Api.Dtos; 
using CampusEats.Api.Models; 
 
namespace CampusEats.Api.Services; 
 
public class MenuService : IMenuService 
{ 
    private readonly AppDbContext _db;      // was: List<MenuItem> + _nextId 
    public MenuService(AppDbContext db) => _db = db; 
 
    private static MenuItemDto ToDto(MenuItem m) => 
        new(m.Id, m.Name, m.Price, m.Category, m.Available); 
 
    public IEnumerable<MenuItemDto> GetAll() => 
        _db.MenuItems.OrderBy(m => m.Id).ToList().Select(ToDto); 
 
    public MenuItemDto? GetById(int id) 
    { 
        var m = _db.MenuItems.Find(id); 
        return m is null ? null : ToDto(m); 
    } 
 
    public MenuItemDto Create(CreateMenuItemDto dto) 
    { 
        var item = new MenuItem 
        { 
            Name = dto.Name,                // no Id — the database assigns it 
            Price = dto.Price, Category = dto.Category, 
            Available = true 
        }; 
        _db.MenuItems.Add(item); 
        _db.SaveChanges();                  // INSERT 
        return ToDto(item); 
    } 
 
    public bool Update(int id, CreateMenuItemDto dto) 
    { 
        var m = _db.MenuItems.Find(id); 
        if (m is null) return false;        // guard clause 
        m.Name = dto.Name;                  // replaces the editable fields 
        m.Price = dto.Price; 
        m.Category = dto.Category; 
        _db.SaveChanges();                  // UPDATE 
        return true; 
    } 
 
    public bool Delete(int id) 
    { 
        var m = _db.MenuItems.Find(id); 
        if (m is null) return false; 
        _db.MenuItems.Remove(m); 
        _db.SaveChanges();                  // DELETE 
        return true; 
    } 
}