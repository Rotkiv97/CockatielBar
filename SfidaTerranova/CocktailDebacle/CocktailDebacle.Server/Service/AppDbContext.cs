using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> DbUser { get; set; } // DbSet per la tabella Users
        // public DbSet<Cocktail> DbCocktails { get; set; } // Se necessario, decommenta
        // public DbSet<RecommenderSystems> DbRecommenderSystems { get; set; } // Se necessario, decommenta

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione della tabella Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Chiave primaria
                entity.Property(u => u.UserName).IsRequired(); // Campo obbligatorio
                entity.Property(u => u.Email).IsRequired(); // Campo obbligatorio
                entity.Property(u => u.PasswordHash).IsRequired(); // Campo obbligatorio
            });

            // Se necessario, aggiungi qui altre configurazioni per Cocktail e RecommenderSystems
            // modelBuilder.Entity<Cocktail>(entity =>
            // {
            //     entity.HasKey(c => c.Id);
            //     // Altre configurazioni
            // });

            // modelBuilder.Entity<RecommenderSystems>(entity =>
            // {
            //     entity.HasKey(r => r.Id);
            //     // Altre configurazioni
            // });
        }
    }
}