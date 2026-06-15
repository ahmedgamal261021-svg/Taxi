using Microsoft.EntityFrameworkCore;
using Taxiiii.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Taxiiii.Models
{
	public class Car
	{
		[Key]
		public int CarId { get; set; }
		[Required]
		public string Model { get; set; }
		[Required]
		public string Brand { get; set; }
		[Required]
		public int Year { get; set; }
		[Required]
		public string Color { get; set; }
		[Required]
		public string LicensePlate { get; set; }
		[Required]
		public string VehicleLicenseType { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public ICollection<DriverCar> DriverCars { get; set; }
	}
	public class DriverCar
	{
			[Key]
			public int DriverCarId { get; set; }

			public int DriverId { get; set; }

			[ForeignKey("DriverId")]
			public Drive Driver { get; set; }

			public int CarId { get; set; }

			[ForeignKey("CarId")]
			public Car Cars { get; set; }
	 //  العربيه مع مين  دلوقتي 
		public bool IsAvailableForDriver { get; set; } = false;

		// هل العربيه دي شغاله دلوقتي ولا لأ
		public bool IsActive { get; set; } = false;
		// وقت ما السواق اتربط بالعربية
		public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
		// وقت ما السواق سيب العربية
		public DateTime? UnassignedAt { get; set; }

	}
}

