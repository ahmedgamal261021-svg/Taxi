using System.ComponentModel.DataAnnotations;

namespace Taxiiii.DtoS
{
	public class RegisterDto
	{
		[Required]
		[MaxLength(25)]
		public string FirstName { get; set; }

		[Required]
		[MaxLength(25)]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[RegularExpression(@"^01[0-9]{9}$")]
		public string PhoneNumber { get; set; }

		[Required]
		[MinLength(6)]
		public string Password { get; set; }

		[Compare("Password")]
		public string ConfirmPassword { get; set; }
	}
}
