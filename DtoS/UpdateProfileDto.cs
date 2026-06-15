namespace Taxiiii.DtoS
{
	public class UpdateProfileDto
	{
		public string? FirstName { get; set; }

		public string? LastName { get; set; }

		public string? PhoneNumber { get; set; }

		public IFormFile? ProfileImage { get; set; }
	}
}
