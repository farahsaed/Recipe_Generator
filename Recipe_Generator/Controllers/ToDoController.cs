using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using Recipe_Generator.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly RecipeContext _db;
        private readonly UserManager<User> userManager;

        public ToDoController(RecipeContext db, UserManager<User> userManager)
        {
            this.userManager = userManager;
            this._db = db;
        }

        [HttpGet("All ToDo items")]
        public async Task<IActionResult> GetAll()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos
                .Where(x => x.IsDeleted == false)
                .Where(u => u.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            if (todo.Count == 0)
                return NotFound();

            return Ok(todo);
        }

        [HttpGet("Item/id:Guid")]
        public async Task<IActionResult> GetTodo(Guid id)
        {
            var todo = await _db.ToDos.FindAsync(id);
            if (todo.IsDeleted == true || todo == null)
                return NotFound();

            return Ok(todo);
        }

        [HttpGet("Deleted items")]
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

        [HttpPost("Create a ToDo item")]
        public async Task<IActionResult> CreateToDo(UserWithToDoDTO toDoDTO)
        {
            ToDo toDo = new ToDo();

            var userId = userManager.GetUserId(HttpContext.User);

            if (userId != null)
            {
                toDo.UserId = userId;
                toDo.Id = Guid.NewGuid();
                toDo.Descriprtion = toDoDTO.Descriprtion;
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
                return BadRequest();
            }
            else
            {
                return NotFound();
            }

        }


        [HttpPost("Update item/id:Guid")]
        public async Task<IActionResult> UpdateToDo(UserWithToDoDTO toDoDTO, Guid id)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            if (userId != null)
            {
                var todo = await _db.ToDos.FindAsync(id);
                if (todo == null)
                    return NotFound();

                todo.UserId = userId;
                todo.IsCompleted = toDoDTO.IsCompleted;
                todo.UpdatedTime = DateTime.Now;
                todo.Descriprtion = toDoDTO.Descriprtion;

                await _db.SaveChangesAsync();
                return Ok(todo);
            }
            else { return NotFound(); }
            

        }

        [HttpPost("Undo Deleted item/id:Guid")]
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

        [HttpDelete("Delete item/id:Guid")]
        public async Task<IActionResult> DeleteToDo(Guid id)
        {
            var userId = userManager.GetUserId(HttpContext.User);

            var todo = await _db.ToDos.FindAsync(id);
            if (todo == null)
                return NotFound();

            todo.UserId = userId;
            todo.DeletedDate = DateTime.Now;
            todo.IsDeleted = true;
            await _db.SaveChangesAsync();
            return Ok("Todo deleted successfully");
        }
    }
}
