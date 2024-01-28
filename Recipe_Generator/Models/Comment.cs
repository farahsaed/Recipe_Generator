using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace Recipe_Generator.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        [Required]
        public string Description { get; set; }
        public ICollection<Reply> Replies { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        public bool IsEdited { get; set; } = false;
        public Recipe Recipe { get; set; }
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }


    }
}
