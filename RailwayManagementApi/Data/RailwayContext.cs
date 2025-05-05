using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Data
{
    public class RailwayContext : IdentityDbContext<ApplicationUser>
    {
        public RailwayContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.Ticket)
                .WithMany(t => t.WaitingLists)
                .HasForeignKey(w => w.TicketID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.Train)
                .WithMany()
                .HasForeignKey(w => w.TrainID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.ClassType)
                .WithMany()
                .HasForeignKey(w => w.ClassTypeID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WaitingList>()
                .HasOne(w => w.Passenger)
                .WithMany()
                .HasForeignKey(w => w.PassengerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.SourceStation)
                .WithMany()
                .HasForeignKey(t => t.SourceID)
                .OnDelete(DeleteBehavior.Restrict); // or NoAction

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.DestinationStation)
                .WithMany()
                .HasForeignKey(t => t.DestinationID)
                .OnDelete(DeleteBehavior.Restrict);
        }


        // public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Passenger> Passengers { get; set; }
        public DbSet<ClassType> ClassTypes { get; set; }
        public DbSet<TrainSchedule> TrainSchedules { get; set; }
        public DbSet<SeatAvailability> SeatAvailabilities { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<WaitingList> WaitingLists { get; set; }

    }
}