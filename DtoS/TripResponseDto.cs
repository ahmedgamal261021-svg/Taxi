using Taxiiii.Models;

namespace Taxiiii.DtoS
{
	public class TripResponseDto
	{
		public int TripId { get; set; }
		 
		public string UserName { get; set; }

		public string StartLocation { get; set; }

		public string EndLocation { get; set; }

		public double DistanceKm { get; set; }
		public int DurationMinutes { get; set; }

		public decimal Price { get; set; }

		public string Status { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}
