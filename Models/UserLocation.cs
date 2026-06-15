using System.ComponentModel.DataAnnotations;

namespace Taxiiii.Models
{
	public class UserLocation
	{
		[Key]
		public int UserLocationId { get; set; }

		[Required]
		public int UserId { get; set; }

		public RigesterUser User { get; set; }

		[Required]
		public double Latitude { get; set; }

		[Required]
		public double Longitude { get; set; }

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
