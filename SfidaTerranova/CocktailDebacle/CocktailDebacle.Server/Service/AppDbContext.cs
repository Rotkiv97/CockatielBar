using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> DbUser { get; set; } // DbSet per la tabella Users
        public DbSet<Cocktail> DbCocktails { get; set; } // DbSet per la tabella Cocktails
        public DbSet<RecommenderSystems> DbRecommenderSystems { get; set; } // Se necessario, decommenta

        public DbSet<UserSearchHistory> DbUserSearchHistory { get; set; } // Se necessario, decommenta
        // public DbSet<Cocktail> DbCocktails { get; set; } // Se necessario, decommenta

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione della tabella Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasAlternateKey(u => u.UserName);
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<Cocktail>(entity =>
            {
                entity.HasKey(c => c.Id); 
                entity.ToTable("Cocktails");
            });

            modelBuilder.Entity<RecommenderSystems>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ProfileText).IsRequired();
                entity.Property(r => r.VectorJsonEmbedding).IsRequired();
                entity.Property(r => r.LastUpdated).IsRequired();

                entity.HasOne(r => r.User)
                    .WithOne(u => u.RecommenderSystems) // relazione uno-a-uno
                    .HasForeignKey<RecommenderSystems>(r => r.UserId) // corretto: usa int UserId
                    .HasPrincipalKey<User>(u => u.Id) // esplicito, corretto
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserSearchHistory>(entity =>
            {
                entity.HasKey(ush => ush.Id);
                entity.Property(ush => ush.UserName).IsRequired();
                entity.Property(ush => ush.SearchText).IsRequired();
                entity.Property(ush => ush.DateCreated).IsRequired();
                entity.HasOne(ush => ush.User)
                    .WithMany(u => u.UserSearchHistory)
                    .HasForeignKey(ush => ush.UserName)
                    .HasPrincipalKey(u => u.UserName) // Match con AlternateKey
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}