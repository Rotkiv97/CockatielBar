using CocktailDebacle.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace CocktailDebacle.Server.Service
{
    public class AppDbContext : DbContext
    {
        // Usa nomi chiari e coerenti per i DbSet
        public DbSet<User> Users { get; set; }
        public DbSet<Cocktail> Cocktails { get; set; }
        public DbSet<RecommenderSystems> RecommenderSystems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione per l'entità User
            modelBuilder.Entity<User>(entity =>
            {
                // Chiave primaria
                entity.HasKey(u => u.Id);

                // Relazione uno-a-molti: User -> CocktailsLike
                entity.HasMany(u => u.CocktailsLike)
                      .WithMany() // Se non c'è una navigazione inversa
                      .UsingEntity(j => j.ToTable("UserCocktailsLike")); // Tabella di join

                // Relazione uno-a-molti: User -> CocktailsCreate
                entity.HasMany(u => u.CocktailsCreate)
                      .WithMany() // Se non c'è una navigazione inversa
                      .UsingEntity(j => j.ToTable("UserCocktailsCreate")); // Tabella di join

                // Relazione uno-a-uno: User -> RecommenderSystems
                entity.HasOne(u => u.RecommenderSystems)
                      .WithOne() // Se non c'è una navigazione inversa
                      .HasForeignKey<RecommenderSystems>(r => r.UserId); // Chiave esterna
            });

            // Configurazione per l'entità Cocktail
            modelBuilder.Entity<Cocktail>(entity =>
            {
                entity.HasKey(c => c.Id); // Chiave primaria
            });

            // Configurazione per l'entità RecommenderSystems
            modelBuilder.Entity<RecommenderSystems>(entity =>
            {
                entity.HasKey(r => r.Id); // Chiave primaria
            });
        }
    }
}