using System.ComponentModel.DataAnnotations;

namespace Taxiiii.Models
{
	public class Rating
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public int TripId { get; set; }
		public Trip Trip { get; set; }
		[Required]
		public int UserId { get; set; }
		public RigesterUser User { get; set; }
		[Required]
		public int DriverId { get; set; }
		public Drive Driver { get; set; }

		[Range(1, 5)]
		public int Stars { get; set; }
		public string? Comment { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}