using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> DbUser { get; set; } // DbSet per la tabella Users
        public DbSet<Cocktail> DbCocktails { get; set; } // DbSet per la tabella Cocktails

        // public DbSet<Cocktail> DbCocktails { get; set; } // Se necessario, decommenta
        public DbSet<RecommenderSystems> DbRecommenderSystems { get; set; } // Se necessario, decommenta

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

            modelBuilder.Entity<Cocktail>(entity =>
            {
                entity.HasKey(c => c.Id); // Definisci la chiave primaria
                entity.ToTable("Cocktails"); // Nome della tabella nel database
            });

            modelBuilder.Entity<RecommenderSystems>(entity => 
            {
                entity.HasKey(r => r.Id); // Definisci la chiave primaria
                entity.Property(r => r.ProfileText).IsRequired(); // Campo obbligatorio
                entity.Property(r => r.VectorJsonEmbedding).IsRequired(); // Campo obbligatorio
                entity.Property(r => r.LastUpdated).IsRequired(); // Campo obbligatorio
                entity.HasOne(r => r.User) // Definisci la relazione con la tabella Users
                    .WithMany()
                    .HasForeignKey(r => r.UserId) // Chiave esterna
                    .OnDelete(DeleteBehavior.Cascade); // Chiave esterna
            });
        }
    }
}