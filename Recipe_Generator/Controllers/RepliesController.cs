using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Interface;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]

    public class RepliesController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RecipeContext db;
        private readonly IEmailSender emailSender;

        public RepliesController(UserManager<User> userManager, RecipeContext db , IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.db = db;
            this.emailSender = emailSender;
        }

        [HttpPost("reply/{cid}")]
        public async Task<IActionResult> AddReply(RepliesDTO repliesDTO, Guid cid)
        {
            var comment = db.Comments.SingleOrDefault(c => c.Id == cid);
            var commentUser = await userManager.FindByIdAsync(comment.UserId);
            var userId = userManager.GetUserId(HttpContext.User);
            var replyUser = await userManager.FindByIdAsync(userId);
            if (userId == null)
            {
                return NotFound("Unauthorized user");
            }
            if (comment == null)
            {
                return NotFound("Comment not exist");
            }
            if(ModelState.IsValid)
            {
                var reply = new Reply();
                reply.Id = Guid.NewGuid();
                reply.Text = repliesDTO.Reply;
                reply.CreatedOn = DateTime.Now;
                reply.UserId = userId;
                reply.CommentId = comment.Id;
                reply.RecipeId = comment.RecipeId;
                db.Replies.Add(reply);
                db.SaveChangesAsync();

                if (commentUser.Id != userId)
                {
                    await emailSender.SendEmailNotification(commentUser.Email, commentUser.FirstName + " " + commentUser.LastName,
                                                            reply.Text, reply.CreatedOn, replyUser.UserName, "reply");
                }


                return Ok();

            }
            return BadRequest("Model not valid");

        }

        [HttpGet("GetAllReplies/{id}")]
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
                .Where(c => c.CommentId == comment.Id)
                .OrderBy(r => r.CreatedOn);
            return Ok(replies);
        }

        [HttpPost("EditReply/{rid}/{cid}")]
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

        [HttpDelete("DeleteComment/{rid}/{cid}")]
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
