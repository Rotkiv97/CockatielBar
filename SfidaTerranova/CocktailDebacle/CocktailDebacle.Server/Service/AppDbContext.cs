using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> DbUser { get; set; } // DbSet per la tabella Users
        public DbSet<Cocktail> DbCocktails { get; set; } // DbSet per la tabella Cocktails
        
        public DbSet<UserHistorySearch> DbUserHistorySearch { get; set; } // DbSet per la tabella UserHistorySearch

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

            // Configurazione della tabella UserHistorySearch
            modelBuilder.Entity<UserHistorySearch>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasAlternateKey(u => u.UserName);
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.SearchDate).IsRequired();
                entity.Property(u => u.SearchText).IsRequired(false);
            });
        }
    }
}