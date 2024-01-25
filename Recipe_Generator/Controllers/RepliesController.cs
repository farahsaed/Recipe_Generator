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
    public class RepliesController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RecipeContext db;
        public RepliesController(UserManager<User> userManager, RecipeContext db)
        {
            this.userManager = userManager;
            this.db = db;
        }

        [HttpPost("reply/{id}")]
        public IActionResult AddReply(RepliesDTO repliesDTO, Guid id)
        {
            var comment = db.Comments.SingleOrDefault(c => c.Id == id);
            var userId = userManager.GetUserId(HttpContext.User);
            if (userId == null)
            {
                return NotFound("Unauthorized user");
            }
            if (comment == null)
            {
                return NotFound("Comment not exist");
            }
            var reply = new Reply();
            reply.Text = repliesDTO.Reply;
            reply.CreatedOn = DateTime.Now;
            reply.UserId = userId;
            reply.CommentId = comment.Id;
            db.Replies.Add(reply);
            db.SaveChanges();
            return Ok();
        }

        [HttpGet("Get all replies/{id}")]
        public IActionResult GetAllReplies(Guid id)
        {
            var comment = db.Comments.SingleOrDefault(c => c.Id == id);
            if (comment == null)
            {
                return NotFound("Comment doesn't exist");
            }
            var userId = userManager.GetUserId(HttpContext.User);
            if (userId == null)
            {
                return NotFound("Unauthorized user");
            }
            var replies = db.Replies.ToList()
                .OrderBy(r => r.CreatedOn);
            return Ok(replies);
        }

        [HttpPost("Edit reply/{rid}/{cid}")]
        public IActionResult EditReply(Guid rid, Guid cid, RepliesDTO repliesDTO)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            if (userId == null)
            {
                return NotFound("Unauthorized user");
            }
            var comment = db.Comments.SingleOrDefault(c => c.Id == cid);
            if (comment == null)
            {
                return NotFound("Comment has been removed");
            }
            var reply = db.Replies.SingleOrDefault(r => r.Id == rid);
            if(reply == null)
            {
                return NotFound("Comment not fount");
            }
            reply.Text = repliesDTO.Reply;
            reply.IsEdited = true;
            db.Replies.Update(reply);
            db.SaveChanges();
            return Ok(reply);
        }

        [HttpDelete("Delete comment/{rid}/{cid}")]
        public IActionResult DeleteReply(Guid rid,Guid cid) 
        {
            var userId = userManager.GetUserId(HttpContext.User);
            if(userId == null)
            {
                return NotFound("Unauthorized user");
            }
            var comment = db.Comments.SingleOrDefault(c => c.Id==cid);
            if(comment == null)
            {
                return NotFound("Comment has been removed");
            }
            var reply = db.Replies.SingleOrDefault(l => l.Id == rid);
            if( reply == null)
            {
                return NotFound("Reply not found");
            }
            db.Replies.Remove(reply);
            db.SaveChanges();
            return Ok("Reply delted successfully");
        }
    }
}
