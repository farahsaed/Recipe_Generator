
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Models;
namespace Recipe_Generator.Data
{
    public class RecipeContext :IdentityDbContext<User>
    {
        public RecipeContext  (){}
        public RecipeContext(DbContextOptions options):base(options){ }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Admin> Admins { get; set; }



    }
}
