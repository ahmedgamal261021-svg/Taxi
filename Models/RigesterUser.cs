using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Taxiiii.Models
{
	
	public class RigesterUser
	{
		[Key]
		public int UserId { get; set; }
		[Required]
		[MaxLength(25)]
		public string FirstName { get; set; }
		[Required]
		[MaxLength(25)] 
		public string LastName { get; set; }
		[Required]
		[RegularExpression(@"^01[0-9]{9}$", ErrorMessage = "Phone number must be valid Egyptian number (11 digits)")]

		public string PhoneNumber { get; set; }
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		
		[MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
		[DataType(DataType.Password)]
		public string? PasswordHash { get; set; }
		[NotMapped]
		[Compare("PasswordHash", ErrorMessage = "Passwords do not match")]
		public string? ConfirmPassword { get; set; }

		public string? ProfileImageUrl { get; set; }
		public bool IsActive { get; set; } = true; 
		public bool IsVerified { get; set; } = false;
		public string? PreferredLanguage { get; set; }
		
		public string Role { get; set; } = UserRole.User.ToString();
		public string? ResetPasswordToken { get; set; }
		public DateTime? ResetTokenExpiry { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }



	}
	
	public enum UserRole
	{
		Admin,
		Driver,
	    User
	}
}
