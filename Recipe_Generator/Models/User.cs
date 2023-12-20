using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class User :Person
    {

        public List<Favourite> Favourites { get; set; }


    }
}
