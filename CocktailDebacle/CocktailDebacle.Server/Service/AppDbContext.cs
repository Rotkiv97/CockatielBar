using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        // Usa nomi chiari e coerenti per i DbSet
        public DbSet<Users> Users { get; set; }
        public DbSet<User> UserList { get; set; }
        public DbSet<Cocktail> Cocktails { get; set; }
        public DbSet<RecommenderSystems> RecommenderSystems { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione per l'entità Users
            // modelBuilder.Entity<Users>(entity =>
            // {
            //     // Configurazione della relazione uno-a-molti tra Users e User
            //     entity.HasMany(u => u.UserList)
            //           .WithMany()
            //           .UsingEntity(j => j.ToTable("Users"));
            //         //   .OnDelete(DeleteBehavior.Cascade);
            //         //Configurazione per l'entità User
            // });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.HasMany(u => u.CocktailsLike)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("UserCocktailsLike"));

                entity.HasMany(u => u.CocktailsCreate)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("UserCocktailsCreate"));

                entity.HasOne(u => u.RecommenderSystems)
                    .WithOne()
                    .HasForeignKey<RecommenderSystems>(r => r.UserId);
            });

            modelBuilder.Entity<Cocktail>(entity =>
            {
                entity.HasKey(c => c.Id);
            });

            modelBuilder.Entity<RecommenderSystems>(entity =>
            {
                entity.HasKey(r => r.Id);
            });
        }
    }
}
