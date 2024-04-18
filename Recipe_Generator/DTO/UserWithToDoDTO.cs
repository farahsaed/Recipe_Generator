namespace Recipe_Generator.DTO
{
    public class UserWithToDoDTO
    {
        public String Descriprtion { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; } = null;
        public DateTime? UpdatedTime { get; set; } = null;
        //public IFormFile? Image { get; set; }
        public string Tilte { get; set; }
    }
}
