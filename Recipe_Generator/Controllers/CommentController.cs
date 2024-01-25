using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly RecipeContext _db;
        private readonly UserManager<User> _userManager;

        public CommentController(RecipeContext db,UserManager<User> userManager)
        {
            this._userManager = userManager;
            this._db = db;
        }

        [HttpPost("Add comment")]
        public IActionResult CreateComment(CommentsDTO commentDTO)
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            if(userId == null)
            {
                return NotFound("Unauthorized user");

            }
            var comment = new Comment();
            comment.Id = Guid.NewGuid();
            comment.Description = commentDTO.Comment;
            comment.CreatedOn = DateTime.Now;
            comment.UserId = userId;
            _db.Comments.Add(comment);
            _db.SaveChanges();
            return Ok(comment);
        }

        [HttpGet]
        public IActionResult GetComments()
        {
            var comments = _db.Comments.ToList();
            if (comments.Any())
            {
                return Ok(comments);
            }
            return BadRequest("No comments found");
        }

        [HttpPost("Edit/{id}")]
        public IActionResult EditComment(Guid id,CommentsDTO commentDTO)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            if(userId == null)
            {
                return NotFound("Unauthorized user");
            }
            var comment = _db.Comments.SingleOrDefault(c => c.Id == id);
            if (comment != null)
            {
                comment.Description = commentDTO.Comment;
                comment.IsEdited = true;
                _db.Comments.Update(comment);
                _db.SaveChanges();
                return Ok(comment);
            }
            return BadRequest("comment not found");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteComment(Guid id)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            if (userId == null)
            {
                return NotFound("Unauthorized user");
            }
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
