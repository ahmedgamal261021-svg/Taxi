using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taxiiii.ApiResponse;
using Taxiiii.Data;
using Taxiiii.DtoS;
using Taxiiii.Interfaces;
using Taxiiii.Models;
using Google.Apis.Auth;
namespace Taxiiii.Services
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;

		public UserService(AppDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ApiResponse<string>> RegisterAsync(RegisterDto dto)
		{

			if (await _context.RigesterUsers.AnyAsync(x => x.Email == dto.Email))
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Email already exists."
				};
			}

			_context.RigesterUsers.Add(new Models.RigesterUser
			{
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				Email = dto.Email,
				PhoneNumber = dto.PhoneNumber,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
			});
			await _context.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "User registered successfully."
			};

			throw new NotImplementedException();
		}

		public async Task<ApiResponse<string>> LoginAsync(LoginDto dto)
		{
			var user = await _context.RigesterUsers.FirstOrDefaultAsync(x => x.Email == dto.Email);
			if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash) || user.Email != dto.Email)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Invalid email or password."
				};
			}
			var token = GenerateJwtToken(user);
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Login successful.",
				Data = token
			};

		}

		public Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
		{
			var user = _context.RigesterUsers.FirstOrDefault(x => x.UserId == userId);
			if (user == null)
			{
				return Task.FromResult(new ApiResponse<UserProfileDto>
				{
					Success = false,
					Message = "User not found."
				});
			}
			var profile = new UserProfileDto
			{
				UserId = user.UserId,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
				PhoneNumber = user.PhoneNumber,
				ProfileImageUrl = user.ProfileImageUrl
			};


			return Task.FromResult(new ApiResponse<UserProfileDto>
			{
				Success = true,
				Message = "User profile retrieved successfully.",
				Data = profile
			});

		}
		private string GenerateJwtToken(RigesterUser user)
		{
			var claims = new[]
			{
			new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Role, user.Role)
		};

			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

			var creds = new SigningCredentials(
				key,
				SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:Issuer"],
				audience: _configuration["JWT:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddDays(7),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
		public async Task<ApiResponse<string>> GoogleLoginAsync(GoogleLoginDto dto)
		{
			try
			{
				var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
				var user = await _context.RigesterUsers.FirstOrDefaultAsync(x => x.Email == payload.Email);
				if (user == null)
				{
					user = new RigesterUser
					{
						FirstName = payload.GivenName ?? payload.Email.Split('@')[0],
						LastName = payload.FamilyName ?? "A",
						Email = payload.Email,
						PhoneNumber = "01000000000", // Default phone number, can be updated later
						ProfileImageUrl = payload.Picture,
						Role = "User"

					};
					_context.RigesterUsers.Add(user);
					await _context.SaveChangesAsync();
				}
				var token = GenerateJwtToken(user);
				return new ApiResponse<string>
				{
					Success = true,
					Message = "Google login successful.",
					Data = token
				};
			}
			catch (Exception ex)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = $"Google login failed: {ex.InnerException?.Message}"
				};

			}
		}
		public async Task<ApiResponse<string>> UserCanncelTrip(string Reason, int userId, int tripId)
		{
			var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.UserId == userId);

			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found or you don't have permission to cancel it",
					Data = null
				};
			}
			if (trip.Status == TripStatus.Started ||
		trip.Status == TripStatus.Completed ||
		trip.Status == TripStatus.Cancelled)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip cannot be canceled"
				};
			}
			trip.Status = TripStatus.Cancelled;
			trip.CancelledAt = DateTime.UtcNow;
			trip.CancelReason = Reason;
			await _context.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip canceled successfully",
				Data = null
			};
		}
		public async Task<ApiResponse<string>> MakeRateToDrive(int tripId, int userId, int Rate)
		{

			var trip = await _context.Trips.Include(t => t.Driver) 
					.FirstOrDefaultAsync(t => t.TripId == tripId);
			

			if (trip == null || trip.UserId != userId)
			{
				Console.WriteLine($"Trip UserId = {trip?.UserId}");
Console.WriteLine($"Current UserId = {userId}");
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found or you don't have permission to rate this trip"
				};
			}
		
			var existingRating = await _context.Trips
				.Where(t => t.TripId == tripId && t.UserId == userId && t.DriverRate != null)
				.Select(t => t.DriverRate)
				.FirstOrDefaultAsync();
			Console.WriteLine(existingRating+"+س+س+5+5+5++");
			if (existingRating != null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "You have already rated this trip"
				};
			}
			if (Rate < 1 || Rate > 5)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Rate must be between 1 and 5"
				};
			}
			

			trip.DriverRate = Rate;
			Console.WriteLine(trip.DriverRate);
			await _context.SaveChangesAsync();
			var avgRate = await _context.Trips
	        .Where(t => t.DriverId == trip.DriverId && t.DriverRate != null)
	        .AverageAsync(t => (double?)t.DriverRate) ?? 0;
			
			trip.Driver.Rating = (int)Math.Round(avgRate);
			
			await _context.SaveChangesAsync();

			trip.DurationMinutes =
	trip.DurationMinutes = trip.EndTime.HasValue && trip.StartTime.HasValue
		? (int)(trip.EndTime.Value - trip.StartTime.Value).TotalMinutes
		: 0;
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Rating submitted successfully"

			};

		} //فيها تكلها 
		public async Task<ApiResponse<string>> GetNearbyDrivers(int UserId, double radiusKm)
		{
			var user = await _context.RigesterUsers.FirstOrDefaultAsync(s => s.UserId == UserId);
			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}
			var locationUser = await _context.UserLocation.FirstOrDefaultAsync(s => s.User.UserId == UserId);
			if (locationUser == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User location not found"
				};
			}
			//double latitudeUser, double longitudeUser, double radiusKm
			var drivers = await _context.DriverLocations
			.Include(d => d.Driver)
				.ThenInclude(driver => driver.User)
			.Where(d => d.Driver.DriverAvailabilityStatu == DriverAvailabilityStatus.Online &&
			d.Driver.User.Role == UserRole.Driver.ToString())
			.ToListAsync();
			Console.WriteLine(drivers);

			var nearbyDrivers = drivers
				.Where(d => CalculateDistance(
					locationUser.Latitude,
					locationUser.Longitude,
					d.Latitude,
					d.Longitude) <= radiusKm)

				.ToList();
			//System.Text.Json.JsonSerializer.Serialize(nearbyDrivers)
			var result = nearbyDrivers.Select(d => new
			{
				d.DriverId,
				d.Latitude,
				d.Longitude,
				DriverName = d.Driver.User.FirstName + " " + d.Driver.User.LastName,
				d.Driver.Rating
			});
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Nearby drivers retrieved successfully.",
				Data = result != null ? System.Text.Json.JsonSerializer.Serialize(result) : "[]"
			};
		}
		private double CalculateDistance(
	double lat1,
	double lon1,
	double lat2,
	double lon2)
		{
			const double R = 6371; // km

			var dLat = DegreesToRadians(lat2 - lat1);
			Console.WriteLine("///////dLat" + dLat);

			var dLon = DegreesToRadians(lon2 - lon1);
			Console.WriteLine("//////////dLon" + dLon);
			var a =
				Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Cos(DegreesToRadians(lat1)) *
				Math.Cos(DegreesToRadians(lat2)) *
				Math.Sin(dLon / 2) *
				Math.Sin(dLon / 2);
			Console.WriteLine("/////////a" + a);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c;
		}

		private double DegreesToRadians(double degrees)
		{
			return degrees * Math.PI / 180;
		}


	}



}


