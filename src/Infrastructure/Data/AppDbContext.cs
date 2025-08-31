using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhatsAppBridgeSD.src.Core.Models;

namespace WhatsAppBridgeSD.src.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.WhatsAppMessageId)
                .IsUnique(false); // si WhatsAppId es único, poner true
            base.OnModelCreating(modelBuilder);
    }
}
