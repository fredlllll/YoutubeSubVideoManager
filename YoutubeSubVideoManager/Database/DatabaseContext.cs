using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UUIDNext;
using YoutubeSubVideoManager.Database.Models;

namespace YoutubeSubVideoManager.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<Channel> Channels { get; set; }

        public string DbPath { get; }

        public DatabaseContext()
        {
            DbPath = Util.GetApplicationFilePath("cache.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Video>().ToTable(nameof(Videos));
            modelBuilder.Entity<Channel>().ToTable(nameof(Channels));
        }

        public static string GetNewId<T>()
        {
            var t = typeof(T);
            var typeId = t.Name.ToLower();
            var id = Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
            return $"{typeId}_{id}";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        public static DatabaseContext Instance { get; } = new DatabaseContext();
    }
}
