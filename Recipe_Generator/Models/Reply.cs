using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Reply
    {
        
        public Guid Id { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime CreatedOn { get; set; }
        public Comment Comment { get; set; }
        [ForeignKey("Comment")]
        public Guid CommentId { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public bool IsEdited { get; set; } = false;
        public Recipe Recipe { get; set; }
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }
    }
}
