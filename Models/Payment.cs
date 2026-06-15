using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Taxiiii.Models
{
	public class Payment

	{
		[Key]
		public int PaymentId { get; set; }

		[Required]
		public int TripId { get; set; }
		public Trip Trip { get; set; } = new Trip();
		[Required]
		public decimal Price { get; set; }
		[Required]
		public WayPay pay { get; set; }
		public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? PaidAt { get; set; }

		public string? TransactionId { get; set; }

	}

	public enum WayPay
	{
		Cash,
		Card,
		Wallet
	}
	public enum PaymentStatus
	{
		Pending,
		Paid,
		Failed,
		Refunded

	}
}
