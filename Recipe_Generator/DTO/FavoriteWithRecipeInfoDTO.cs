namespace Recipe_Generator.DTO
{
    public class FavoriteWithRecipeInfoDTO
    {
        public int Id { get; set; }
        public string RecipeName  { get; set; }
        public int RecipeId { get; set; }

        public string UserName { get; set; }
        public int UserId { get; set; }

    }
}
