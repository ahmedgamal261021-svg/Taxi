using System.ComponentModel.DataAnnotations;

namespace Taxiiii.DtoS
{
	public class ApplyDriverDto
	{
		[Required]
		public string LicenseNumber { get; set; }

		[Required]
		public DateTime LicenseExpiryDate { get; set; }
	}
}
