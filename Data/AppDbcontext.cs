using Microsoft.EntityFrameworkCore;
using Taxiiii.Models;
using System;

namespace Taxiiii.Data
{
	public class AppDbContext : DbContext
	{
		// Fix for CS0246: Add using for Microsoft.EntityFrameworkCore and use correct type name
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
		public DbSet<RigesterUser> RigesterUsers { get; set; }
		public DbSet<UserLocation> UserLocation { get; set; }
		public DbSet<Trip> Trips { get; set; }
		public DbSet<Drive> Drives { get; set; }
		public DbSet<Car> Cars { get; set; }

		public DbSet<DriverCar> DriverCars { get; set; }
		public DbSet<DriverLocation> DriverLocations { get; set; } 


		//public DbSet<Rating> Ratings { get; set; }
		//public DbSet<Notification> Notifications { get; set; }
		//public DbSet<Payment> Payments { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<UserLocation>()
				.HasOne(u => u.User)
				.WithOne()
				.HasForeignKey<UserLocation>(l => l.UserId)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<Trip>().
						HasOne(t => t.User)
			.WithMany()
			.HasForeignKey(t => t.UserId)
			.OnDelete(DeleteBehavior.Restrict);
	//		modelBuilder.Entity<Trip>()
	//.Property(t => t.Status)
	//.HasConversion<string>();
	//		modelBuilder.Entity<Trip>()
 //   .Property(t => t.PaymentStatus)
 //   .HasConversion<string>();
				modelBuilder.Entity<Drive>().HasOne(u => u.User)
					.WithOne()
					.HasForeignKey<Drive>(d => d.UserId)
					.OnDelete(DeleteBehavior.Cascade);

					modelBuilder.Entity<Drive>().HasIndex(d => d.LicenseNumber).IsUnique();
					modelBuilder.Entity<RigesterUser>().HasIndex(d => d.PhoneNumber ).IsUnique();
					modelBuilder.Entity<RigesterUser>().HasIndex(d => d.Email ).IsUnique();
			modelBuilder.Entity<DriverCar>().HasIndex(x => new { x.DriverId, x.CarId }).IsUnique();


			modelBuilder.Entity<DriverCar>()
				.HasOne(d => d.Driver)
				.WithMany(d => d.DriverCars)
				.HasForeignKey(d => d.DriverId)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<DriverCar>().HasOne(d => d.Cars)
			  .WithMany( d => d.DriverCars)
			  .HasForeignKey(d => d.CarId)
			  .OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<DriverCar>()
	.HasIndex(dc => new { dc.DriverId, dc.CarId })
	.IsUnique();
			modelBuilder.Entity<DriverLocation>().HasOne(d=>d.Driver).WithOne().
				HasForeignKey<DriverLocation>(d => d.DriverId).OnDelete(DeleteBehavior.Cascade);


			//		modelBuilder.Entity<Trip>()
			//			.HasOne(t => t.Driver)
			//			.WithMany()
			//			.HasForeignKey(t => t.DriverId)
			//			.OnDelete(DeleteBehavior.Restrict);
			//		
			//		modelBuilder.Entity<Trip>()
			//.HasIndex(t => t.DriverId);

			//		modelBuilder.Entity<Trip>()
			//			.HasIndex(t => t.UserId);


			//		modelBuilder.Entity<Rating>()
			//			.HasOne(r => r.Trip)
			//			.WithMany()
			//			.HasForeignKey(r => r.TripId)
			//			.OnDelete(DeleteBehavior.Cascade);
			//		modelBuilder.Entity<Rating>()
			//			.HasIndex(x => new { x.TripId, x.UserId })
			//			.IsUnique();



	//		modelBuilder.Entity<DriverLocation>()
	//.HasIndex(x => x.DriverId);
			//		modelBuilder.Entity<Notification>()
			//			.HasOne(n => n.User)
			//			.WithMany()
			//			.HasForeignKey(n => n.UserId)
			//		.OnDelete(DeleteBehavior.Cascade);
			//		modelBuilder.Entity<Notification>()
			//.HasIndex(x => x.UserId);


			//		modelBuilder.Entity<Payment>()
			//.HasIndex(x => x.TripId);

		}


	}
}

