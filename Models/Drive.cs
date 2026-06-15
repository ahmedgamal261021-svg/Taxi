using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taxiiii.Models
{
	public class Drive
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public int UserId { get; set; }
		[ForeignKey("UserId")]
		public RigesterUser User { get; set; }
		public DriverStatus DriverStatu { get; set; } = DriverStatus.Pending;    
		public DriverAvailabilityStatus DriverAvailabilityStatu { get; set; } = DriverAvailabilityStatus.Offline;
		[Required]
		public string LicenseNumber { get; set; }

		public DateTime LicenseExpiryDate { get; set; }
		[Range(0, 5)]
		public int? Rating { get; set; } = 0;

		public int? TotalTrips { get; set; } = 0;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ICollection<DriverCar> DriverCars { get; set; }

	}
	public class DriverLocation
	{
		public int Id { get; set; }

		public int DriverId { get; set; }
		public Drive Driver { get; set; }
		
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public DateTime CreateAt { get; set; }
	}
	public enum DriverStatus
	{
		Pending,
		Approved,
		Rejected,
		Suspended
	}
	public enum DriverAvailabilityStatus
	{
		Offline,
		Online,
		Busy,
		OnTrip
	}
}
