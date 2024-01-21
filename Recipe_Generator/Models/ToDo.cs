using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class ToDo
    {
        public Guid Id { get; set; }
        public String Descriprtion { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; } = null;
        public DateTime? UpdatedTime { get; set; } = null;
        [ForeignKey("UserId")]
        public User User { get; set; }
        public string UserId { get; set; }
    }
}
