using Xunit;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Controllers;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recipe_Generator_tests.Data
{
    public class RecipeControllerTest
    {
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;
        private static RecipeContext databasecontext;

        public RecipeControllerTest()
        {
            _mapper = A.Fake<IMapper>();
            _webHostEnvironment = A.Fake<IWebHostEnvironment>();
            _userManager = A.Fake<UserManager<User>>();
        }

        public static async Task<RecipeContext> GetDatabaseContext()
{
    if (databasecontext == null)
    {
        var options = new DbContextOptionsBuilder<RecipeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        databasecontext = new RecipeContext(options);
        if (await databasecontext.Recipes.CountAsync() <= 0)
        {
            var user = new User { Id = "0739e5fb-5b11-43ab-8dd3-ed22256fba32", UserName = "Nour.esmatt", FirstName = "Nour", LastName = "Esmatt" };
            for (int i = 1; i <= 5; i++)
            {
                var recipe = new Recipe()
                {
                    Id = i,
                    Name = "Soup",
                    AverageRating = i % 2 == 0 ? (double?)i : null,
                    PrepareTime = $"{i * 10} minutes",
                    Image = $"image{i}.jpg",
                    CookTime = $"{i * 15} minutes",
                    TotalTime = $"{i * 25} minutes",
                    Ingredients = $"Ingredient {i}",
                    Directions = $"Direction {i}",
                    CategoryId = 1,
                    Nutrition = $"Nutrition {i}",
                    Timing = $"Timing {i}",
                    State = i % 2 == 0 ? RecipeState.Approved : RecipeState.Pending,
                    User = user,
                    Ratings = new List<Rating> { new Rating { RatingValue = i % 5 + 1 } }
                };
                databasecontext.Recipes.Add(recipe);
            }
            await databasecontext.SaveChangesAsync();
        }
    }
    return databasecontext;
}

        [Fact]
        public async Task RecipeContext_GetPagedRecipes_ReturnOK()
        {
            // Arrange
            int pageNumber = 1;
            int pageSize = 10;
            var dbcontext = await GetDatabaseContext();
            var recipeController = new Recipe_Generator.Controllers.RecipeController(dbcontext, _mapper, _webHostEnvironment, _userManager);

            // Act
            var result = await recipeController.GetPagedRecipes(pageNumber, pageSize);

            // Assert
            result.Should().BeOfType<OkObjectResult>(); // Ensure the result is Ok
            result.Should().NotBeNull(); // Ensure the value is not null
        }

    }
}
