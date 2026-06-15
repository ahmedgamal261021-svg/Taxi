using System.ComponentModel.DataAnnotations;

namespace Taxiiii.Models
{
	public class RideRequest
	{
		[Key]
		public int Id { get; set; }

		public int UserId { get; set; }
		public RigesterUser User { get; set; }

		public int? DriverId { get; set; }
		public Drive? Driver { get; set; }

		[Required]
		public string PickupLocation { get; set; }

		[Required]
		public string DestinationLocation { get; set; }

		public decimal EstimatedPrice { get; set; }  // المتوقع للرحلة بناءً على المسافة والوقت المتوقع

		public RequestStatus Status { get; set; } = RequestStatus.Pending;
		public PaymentMethod PaymentStatus { get; set; } = PaymentMethod.Cash;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
	public enum RequestStatus
	{
		Pending,
		Accepted,
		Rejected,
		Cancelled,
		Expired
	}
	public enum PaymentMethod
	{
		Cash,
		Visa,
		Wallet,
		PayPal
	}
}
