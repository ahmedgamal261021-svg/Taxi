namespace Taxiiii.DtoS
{
	public class DriverCarDto
	{
		public int DriverCarId { get; set; }

		// Driver Info
		public int DriverId { get; set; }
		public int UserId { get; set; }
		public int? Rating { get; set; }
		public int? TotalTrips { get; set; }

		// Car Info
		public int CarId { get; set; }
		public string Model { get; set; }
		public string Brand { get; set; }
		public int Year { get; set; }
		public string Color { get; set; }
		public string LicensePlate { get; set; }
		public string VehicleLicenseType { get; set; }

		// Assignment Info
		public bool IsAvailableForDriver { get; set; }
		public bool IsActive { get; set; }
		public DateTime AssignedAt { get; set; }
		public DateTime? UnassignedAt { get; set; }
	}
}
