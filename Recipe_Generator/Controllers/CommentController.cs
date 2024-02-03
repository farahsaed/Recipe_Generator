using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Interface;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly RecipeContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender emailSender;

        public CommentController(RecipeContext db,UserManager<User> userManager,IEmailSender emailSender)
        {
            this._userManager = userManager;
            this.emailSender = emailSender;
            this._db = db;
        }

        [HttpPost("AddComment/{rid}")]
        public async Task<IActionResult> CreateComment(CommentsDTO commentDTO,int rid)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var recipe = _db.Recipes.Include(r => r.User).SingleOrDefault(r => r.Id == rid);
            var user = await _userManager.FindByIdAsync(userId);
            if(userId == null)
            {
                return NotFound("Unauthorized user");

            }
            if (recipe == null)
            {
                return BadRequest("Recipe not found");
            }

            var comment = new Comment();
            comment.Id = Guid.NewGuid();
            comment.Description = commentDTO.Comment;
            comment.CreatedOn = DateTime.Now;
            comment.UserId = userId;
            comment.RecipeId = recipe.Id;
            _db.Comments.Add(comment);
            _db.SaveChanges();
            await emailSender.SendEmailNotification(recipe.User.Email, 
                recipe.User.FirstName + " " + recipe.User.LastName,
                comment.Description,comment.CreatedOn,user.UserName,"comment");
            return Ok(comment);
        }

        [HttpGet("AllComments/{rid}")]
        public IActionResult GetComments(int rid)
        {
            var recipe = _db.Recipes.SingleOrDefault(r => r.Id==rid);
            if(recipe == null)
            {
                return NotFound("Recipe not found");
            }
            var comments = _db.Comments
                .Where(c=> c.RecipeId == recipe.Id)
                .ToList();
            if (comments.Any())
            {
                return Ok(comments);
            }
            return BadRequest("No comments found");
        }

        [HttpPost("Edit/{id}/{rid}")]
        public IActionResult EditComment(Guid id,CommentsDTO commentDTO,int rid)
        {
            var recipe = _db.Recipes.SingleOrDefault(r => r.Id == rid);
            var userId = _userManager.GetUserId(HttpContext.User);
            if(userId == null)
            {
                return NotFound("Unauthorized user");
            }
            if (recipe == null)
            {
                return NotFound("Recipe not found");
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

        [HttpDelete("{id}/{rid}")]
        public IActionResult DeleteComment(Guid id,int rid)
        {
            var recipe = _db.Recipes.SingleOrDefault(r => r.Id == rid);
            if(recipe == null) 
            {
                return NotFound("Recipe not found");
            }
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
