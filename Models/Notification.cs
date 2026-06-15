using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Taxiiii.Models
{
	public class Notification
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public int UserId { get; set; }
		public RigesterUser User { get; set; }

		[Required]
		[MaxLength(300)]
		public string Message { get; set; }
		public bool IsRead { get; set; } = false;
		public NotificationType Type { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
	public enum NotificationType
	{
		RideRequest,
		RideAccepted,
		RideStarted,
		RideCompleted,
		RideCancelled,
		PaymentSuccess,
		PaymentFailed
	}
}
