using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using Recipe_Generator.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Collections.Generic;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly RecipeContext _db;
        private readonly UserManager<User> userManager;
        private IWebHostEnvironment _environment;

        public ToDoController(RecipeContext db, UserManager<User> userManager,IWebHostEnvironment environment)
        {
            this._environment = environment;
            this.userManager = userManager;
            this._db = db;
        }

        [HttpGet("AllToDoItems")]
        public async Task<IActionResult> GetAll()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos
                .Where(x => x.IsDeleted == false)
                .Where(u => u.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

            var titles = (from t in todo select t.Title).ToList();

            if (todo.Count == 0)
                return NotFound("Todo is empty");

            if (titles == null)
                return BadRequest("No todo was found");
           
            return Ok(titles);
        }

        [HttpGet("Item/id:Guid")]
        public async Task<IActionResult> GetTodo(Guid id)
        {
            var todo = await _db.ToDos.FindAsync(id);
            if (todo.IsDeleted == true || todo == null)
                return NotFound();

            return Ok(todo);
        }

        [HttpGet("DeletedItems")]
        public async Task<IActionResult> GetDeletedItems()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos
                .Where(x => x.IsDeleted == true)
                .Where(u => u.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            if (todo.Count <= 0)
                return NotFound("There is no deleted items");

            return Ok(todo);
        }

        [HttpPost("CreateToDoItem")]
        public async Task<IActionResult> CreateToDo(UserWithToDoDTO toDoDTO)
        {
            ToDo toDo = new ToDo();

            var userId = userManager.GetUserId(HttpContext.User);

            if (userId != null)
            {
                toDo.UserId = userId;
                toDo.Id = Guid.NewGuid();
                toDo.Descriprtion = toDoDTO.Descriprtion;

                if (toDoDTO.Tilte == "" || toDoDTO.Tilte == null)
                    toDo.Title = toDoDTO.Descriprtion;
                else
                    toDo.Title = toDoDTO.Tilte;

                toDo.CreatedDate = DateTime.Now;
                toDo.IsDeleted = false;
                toDo.IsCompleted = false;
                toDo.DeletedDate = null;
                toDo.UpdatedTime = null;
                if (ModelState.IsValid)
                {
                    await _db.ToDos.AddAsync(toDo);
                    await _db.SaveChangesAsync();
                    return Ok("ToDo item created successfully");
                }
                return BadRequest(ModelState);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpPost("UpdateItem/{id:Guid}")]
        public async Task<IActionResult> UpdateToDo(UserWithToDoDTO toDoDTO, Guid id)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            if (userId != null)
            {
                var todo = await _db.ToDos.FindAsync(id);
                
                if (todo == null)
                    return NotFound("Todo not found");

                todo.IsCompleted = toDoDTO.IsCompleted;
                todo.UpdatedTime = DateTime.Now;
                todo.Descriprtion = toDoDTO.Descriprtion;

                await _db.SaveChangesAsync();
                return Ok(todo);
            }
            else { return NotFound("Unauthorized user. You must login first"); }
        }

        [HttpPost("UndoDeletedItem/{id:Guid}")]
        public async Task<IActionResult> UndoDelete(Guid id)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos.FindAsync(id);
            if (todo == null)
                return NotFound();

            todo.UserId = userId;
            todo.IsDeleted = false;
            todo.DeletedDate = null;
            todo.CreatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(todo);
        }
        [HttpDelete("DeleteItem/{id:Guid}")]
        public async Task<IActionResult> DeleteToDo(Guid id)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos.FindAsync(id);
            if (todo == null)
                return NotFound("Todo not found");

            todo.UserId = userId;
            todo.DeletedDate = DateTime.Now;
            todo.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Ok("Todo deleted successfully");
        }

        [HttpPost("AddImage/{id}")]
        public async Task<IActionResult> AddImage(Guid id, IFormFile Image)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            if (userId != null)
            {
                var todo = await _db.ToDos.FindAsync(id);
                if (todo == null)
                    return NotFound("Todo item not found");

                string wwwRootPath = _environment.WebRootPath;
                if (Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var filePath = Path.Combine(wwwRootPath, @"images\TodoImages");
                    var extension = Path.GetExtension(Image.FileName);
                    if (todo.ImagePath != null)
                    {
                        var oldImage = Path.Combine(wwwRootPath, todo.ImagePath.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(filePath, fileName + extension), FileMode.Create))
                    {
                        Image.CopyTo(fileStream);
                    }

                    todo.ImagePath = @"images\TodoImages\" + fileName + extension;
                    await _db.SaveChangesAsync();

                    return Ok("Image uploaded successfuly");
                }
            }
            
            return NotFound("Unauthorized user");
        }
    }
}
