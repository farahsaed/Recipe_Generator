using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recipe_Generator.Data;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly RecipeContext _db;

        public CommentController(RecipeContext db)
        {
            this._db = db;
        }

        //[HttpPost]
        //public IActionResult CreateComment(Comment comment)
        //{
        //    comment.Description = 
        //}

        [HttpGet]
        public IActionResult GetComments() 
        {
            var comments = _db.Comments.ToList();
            if(comments.Any())
            {
                return Ok(comments);
            }
            return BadRequest("No comments found");
        }

        [HttpPost("{id}")]
        public IActionResult EditComment(int id)
        {
            var comment = _db.Comments.SingleOrDefault(c => c.Id == id);
            if (comment != null)
            {
                _db.Comments.Update(comment);
                _db.SaveChanges();
                return Ok(comment);
            }
            return BadRequest("comment not found");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteComment(int id)
        {
            var comment = _db.Comments.SingleOrDefault(c => c.Id == id);
            if (comment != null)
            {
                _db.Comments.Remove(comment);
                _db.SaveChanges();
                return Ok("Comment deleted successfully");
            }
            return BadRequest("comment not found");
        }
    }
}
