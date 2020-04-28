using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;
using System.Threading.Tasks;
using System.Threading;
using GameService.Models;
using PokerClassLibrary;
using GameService.Utilities;
using Microsoft.Extensions.Configuration;
using System.Data.Entity.ModelConfiguration.Configuration;

namespace GameService.Context
{
    public class GameDbContext : DbContext
    {

        public GameDbContext(DbContextOptions<GameDbContext> options, IConfiguration config) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
            //Database.Migrate();

            if (Players.Count() == 0) { 
                try
                {
                    var hash = new Guid().ToString();
                    Players.Add(new Player()
                    {
                        UserName = "ThePapaX",
                        Name = "Armando Lozada",
                        Email = "arloznav.sis@gmail.com",
                        Identity = new PlayerIdentity()
                        {
                            Hash= hash,
                            Password = Encryption.EncryptPassword("123", hash)
                        }

                    });
                    this.SaveChanges();
                }
                catch (Exception ex)
                {
                }
            }

        }

        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerIdentity> PlayerIdentity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(player => player.Email)
                .IsUnique();

                entity.HasOne(u => u.Identity)
                .WithOne(ui => ui.Player)
                .HasForeignKey<PlayerIdentity>(ui => ui.PlayerId);
            });
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }


        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is ITrackable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.LastUpdatedAt = now;
                            break;

                        case EntityState.Added:
                            trackable.CreatedAt = now;
                            trackable.LastUpdatedAt = now;
                            break;
                    }
                }
            }
        }

    }
}
