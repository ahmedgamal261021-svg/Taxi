using System.ComponentModel.DataAnnotations;
using Taxiiii.Models;

namespace Taxiiii.DtoS
{
	public class AssignedCar
	{
	
		
		public string Model { get; set; }
		
		public string Brand { get; set; }
		
		public int Year { get; set; }
		
		public string Color { get; set; }
		
		public string LicensePlate { get; set; }
		
		public string VehicleLicenseType { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	
	}
}
