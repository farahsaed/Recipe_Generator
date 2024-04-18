using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private readonly RecipeContext _db;

        public ToDoController(RecipeContext db)
        {
            this._db = db;
        }

        [HttpGet("All ToDo items")]
        public async Task<IActionResult> GetAll()
        {
            var todo = await _db.ToDos
                .Where(x => x.IsDeleted == false)
                .OrderByDescending(x=>x.CreatedDate)
                .ToListAsync();

            if(todo.Count == 0)
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
            var todo = await _db.ToDos
                .Where(x => x.IsDeleted == true)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            if (todo.Count <= 0)
                return NotFound("There is no deleted items");

            return Ok(todo);
        }

        [HttpPost("Create a ToDo item")] 
        public async Task<IActionResult> CreateToDo(ToDo toDo)
        {
            toDo.Id = Guid.NewGuid();
            toDo.CreatedDate = DateTime.Now;
            toDo.IsDeleted = false;
            toDo.IsCompleted = false;
            toDo.DeletedDate = null;
            toDo.UpdatedTime = null;
            if(ModelState.IsValid)
            {
                await _db.ToDos.AddAsync(toDo);
                await _db.SaveChangesAsync();
                return Ok("ToDo item created successfully");
            }
            return BadRequest();
        }

        [HttpPost("Update item/id:Guid")]
        public async Task<IActionResult> UpdateToDo(ToDo toDo,Guid id)
        {
            var todo = await _db.ToDos.FindAsync(id);
            if (todo == null)
                return NotFound();

            todo.IsCompleted = toDo.IsCompleted;
            todo.UpdatedTime = DateTime.Now;
            todo.Descriprtion = toDo.Descriprtion;

            await _db.SaveChangesAsync();
            return Ok(todo);

        }

        [HttpPost("Undo Deleted item/id:Guid")]
        public async Task<IActionResult> UndoDelete(Guid id)
        {
            var todo = await _db.ToDos.FindAsync(id);
            if (todo == null)
                return NotFound();

            todo.IsDeleted = false;
            todo.DeletedDate = null;
            todo.CreatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(todo);
        }

        [HttpDelete("Delete item/id:Guid")]
        public async Task<IActionResult> DeleteToDo(Guid id)
        {
            var todo = await _db.ToDos.FindAsync(id);
            if(todo == null)
                return NotFound();

            todo.DeletedDate = DateTime.Now;
            todo.IsDeleted = true;
            todo.
            await _db.SaveChangesAsync();
            return Ok("Todo deleted successfully");
        }

        

    }
}
