namespace CocktailDebacle.Server.Models
{
    public class Users
    {
        public ICollection<User> _Users { set; get;} = new List<User>();
    }
}
