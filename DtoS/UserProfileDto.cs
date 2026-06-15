namespace Taxiiii.DtoS
{
	public class UserProfileDto
	{
		public int UserId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Email { get; set; }

		public string PhoneNumber { get; set; }

		public string? ProfileImageUrl { get; set; }
	}
}
