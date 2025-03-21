using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        public DbSet<Users> DbUsers { get; set; }
        public DbSet<User> DbUser { get; set; }
        public DbSet<Cocktail> DbCocktails { get; set; }
        public DbSet<RecommenderSystems> DbRecommenderSystems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relazione tra Users e User (uno-a-molti)
            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserList)
                .WithOne(u => u.Users)
                .HasForeignKey(u => u.UsersId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relazioni per User
            modelBuilder.Entity<User>()
                .HasMany(u => u.CocktailsLike)
                .WithMany()
                .UsingEntity(j => j.ToTable("UserCocktailsLike"));

            modelBuilder.Entity<User>()
                .HasMany(u => u.CocktailsCreate)
                .WithMany()
                .UsingEntity(j => j.ToTable("UserCocktailsCreate"));

            modelBuilder.Entity<User>()
                .HasOne(u => u.RecommenderSystems)
                .WithOne()
                .HasForeignKey<RecommenderSystems>(r => r.UserId);

            modelBuilder.Entity<Cocktail>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<RecommenderSystems>()
                .HasKey(r => r.Id);
        }
    }
}
