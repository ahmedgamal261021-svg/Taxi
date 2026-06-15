using System.ComponentModel.DataAnnotations;
namespace Taxiiii.DtoS
{
	public class LoginDto
	{
		[Required]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
