using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Taxiiii.Models
{
	public class Trip
	{
		[Key]
		public int TripId { get; set; }

		public int? DriverId { get; set; }
		public Drive? Driver { get; set; }

		[Required]
		public int UserId { get; set; }
		public RigesterUser User { get; set; }

		[Required]
		public string StartLocation { get; set; }

		[Required]
		public string EndLocation { get; set; }

		public double DistanceKm { get; set; }

		public int DurationMinutes { get; set; }

		[Required]
		[Precision(18, 2)]
		public decimal Price { get; set; }

		public DateTime? StartTime { get; set; }

		public DateTime? EndTime { get; set; }
		[Range(0, 5)]
		public int? DriverRate{ get; set; } = 0;

		public TripStatus Status { get; set; } = TripStatus.Pending;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public PaymentStatus0 PaymentStatus { get; set; } = PaymentStatus0.Pending;

		public string? CancelReason { get; set; }
		public DateTime? CancelledAt { get; set; }
	}

	public enum TripStatus
	{
		Pending,
		Accepted,
		Started,
		Completed,
		Cancelled
	}

	public enum PaymentStatus0
	{
		Pending,
		Paid,
		Failed,
		Refunded
	}
}