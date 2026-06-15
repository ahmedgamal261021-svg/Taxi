using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taxiiii.ApiResponse;
using Taxiiii.DtoS;
// Remove or comment out the following line since 'Taxiiii.Data' does not exist or is not needed
// using Taxiiii.Data;
using Taxiiii.Data;
using Taxiiii.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using Taxiiii.EmailService;

namespace Taxiiii.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly Services.UserService _userService;
		private readonly IEmailService _emailService;

		private readonly AppDbContext _context;
		// 1. Add a private readonly field for IEmailService

		public UserController(Services.UserService userService, AppDbContext _context, IEmailService _emailService)
		{
			_userService = userService;
			this._context = _context;
			this._emailService = _emailService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDto dto)
		{
			var result = await _userService.RegisterAsync(dto);

			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDto dto)
		{
			var result = await _userService.LoginAsync(dto);

			return Ok(result);
		}

		[Authorize]
		[HttpGet("profile")]
		public async Task<IActionResult> Profile()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

			var result = await _userService.GetProfileAsync(userId);

			return Ok(result);
		}
		[HttpPost("google-login")]
		public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
		{
			var result = await _userService.GoogleLoginAsync(dto);

			return Ok(result);
		}
		[HttpPost("logout")]
		public IActionResult Logout()
		{
			return Ok(new ApiResponse<string>
			{
				Success = true,
				Message = "Logged out successfully"
			});
		}
		[HttpPost("forgetPassword")]
		public async Task<IActionResult> ForgetPassword(ForgotPasswordDto dto)

		{
			try
			{

				var result = await _context.RigesterUsers.FirstOrDefaultAsync(d => d.Email == dto.Email);
				if (result == null)
				{
					return NotFound(new ApiResponse<string>
					{
						Success = false,
						Message = "Email not found."
					});
				}
				var otp = new Random().Next(100000, 999999).ToString();

				result.ResetPasswordToken = otp;
				result.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

				await _context.SaveChangesAsync();

				await _emailService.SendAsync(
					result.Email,
					"Password Reset OTP",
					$"Your OTP code is: {otp}"
				);

				return Ok(new ApiResponse<string>
				{
					Success = true,
					Message = "OTP sent to email"
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return StatusCode(500, new ApiResponse<string>
				{
					Success = false,
					Message = "An error occurred while processing your request." + ex.Message
				});
			}
		}
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
		{
			var user = await _context.RigesterUsers
				.FirstOrDefaultAsync(x => x.Email == dto.Email);

			if (user == null ||
				user.ResetPasswordToken != dto.Token ||
				user.ResetTokenExpiry < DateTime.UtcNow)
			{
				return BadRequest(new ApiResponse<string>
				{
					Success = false,
					Message = "Invalid or expired token"
				});
			}

			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

			user.ResetPasswordToken = null;
			user.ResetTokenExpiry = null;

			await _context.SaveChangesAsync();

			return Ok(new ApiResponse<string>
			{
				Success = true,
				Message = "Password reset successfully"
			});
		}
		[Authorize]
		[HttpPost("LocationUser")]
		public async Task<IActionResult> UpdateLocation(UpdateLocationDto dto)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			var userLocation = await _context.UserLocation.FirstOrDefaultAsync(ul => ul.UserId == userId);
			if (userLocation == null)
			{
				userLocation = new UserLocation
				{
					UserId = userId,
					Latitude = dto.Latitude,
					Longitude = dto.Longitude,
					UpdatedAt = DateTime.UtcNow
				};
				_context.UserLocation.Add(userLocation);
			}
			else
			{
				userLocation.Latitude = dto.Latitude;
				userLocation.Longitude = dto.Longitude;
				userLocation.UpdatedAt = DateTime.UtcNow;
			}
			await _context.SaveChangesAsync();
			return Ok(new ApiResponse<string>
			{
				Success = true,
				Message = "Location updated successfully"
			});


		}
		[Authorize]
		[HttpPut("profile")]
		public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

			var user = await _context.RigesterUsers.FindAsync(userId);

			if (user == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrEmpty(dto.FirstName))
				user.FirstName = dto.FirstName;

			if (!string.IsNullOrEmpty(dto.LastName))
				user.LastName = dto.LastName;

			if (!string.IsNullOrEmpty(dto.PhoneNumber))
				user.PhoneNumber = dto.PhoneNumber;

			// image upload هنا

			await _context.SaveChangesAsync();

			return Ok(new
			{
				success = true,
				message = "Profile updated successfully"
			});
		}
		[Authorize]
		[HttpPut]
		[Route("CanncelTripByUser")]
		public async Task<IActionResult> CanncelTrip(string reason, int TripId)
		{
			var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (UserId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _userService.UserCanncelTrip(reason, UserId, TripId);
			return Ok(result);

		}
		[Authorize]
		[HttpPut]
		[Route("MakeRateToDrive")]
		public async Task<IActionResult> MakeRateToDrive(int Rate, int TripId)
		{
			var Userid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (Userid == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _userService.MakeRateToDrive(TripId, Userid, Rate);
			return Ok(result);
		}
		[HttpGet]
		[Route("GetNearbyDrivers")]
		public async Task<IActionResult> GetNearbyDrivers(int userId, double radiusKm)
		{
			var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (UserId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}

			var result = await _userService.GetNearbyDrivers(userId, radiusKm);
			return Ok(result);
		}
	}
}

