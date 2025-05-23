﻿﻿using CocktailDebacle.Server.Models;
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
                entity.Property(u => u.UserName).IsRequired();
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // configurazione della tabella Cocktail
            modelBuilder.Entity<Cocktail>(entity =>
            {
                entity.HasKey(c => c.Id); 
                entity.ToTable("Cocktails");
            });

            // Configurazione della tabella UserHistorySearch
            modelBuilder.Entity<UserHistorySearch>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.SearchDate).IsRequired();
                entity.Property(u => u.SearchText).IsRequired(false);

                entity.HasOne(u => u.User)
                    .WithMany()
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // configurazione della tabella UserCocktailsLike
            modelBuilder.Entity<User>()
                .HasMany(u => u.CocktailsLike)
                .WithMany(c => c.UserLikes)
                .UsingEntity(j => j.ToTable("UserCocktailsLike"));

            // configurazione della tabella UserUser
            modelBuilder.Entity<User>()
                .HasMany(u => u.Followed_Users)
                .WithMany(u => u.Followers_Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserUser",
                    r => r.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("FollowerId")  // chi segue
                        .OnDelete(DeleteBehavior.Restrict),
                    l => l.HasOne<User>()
                        .WithMany()
                        .HasForeignKey("FollowedId") // chi è seguito
                        .OnDelete(DeleteBehavior.Restrict),
                    je =>
                    {
                        je.HasKey("FollowerId", "FollowedId");
                    });

            }
    }
}