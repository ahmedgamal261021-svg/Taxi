namespace Taxiiii.DtoS
{
	public class DriverInfoDto
	{
		public int Id { get; set; }
		public string LicenseNumber { get; set; }
		public DateTime LicenseExpiryDate { get; set; }
		public string DriverStatus { get; set; }
		public string DriverAvailabilityStatu { get; set; }
		public int? Rating { get; set; }
		public int? TotalTrips { get; set; }
	}
}
