using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        public string Description { get; set; }

    }
}
