using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;
using System.Threading.Tasks;
using System.Threading;
using GameService.Models;
using PokerClassLibrary;

namespace GameService.Context
{
    public class GameDbContext : DbContext
    {

        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerIdentity> UsersIdentity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();

                //entity.Property(e=> e.Id).HasDefaultValueSql("newid()"); OR newsequentialid()

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
