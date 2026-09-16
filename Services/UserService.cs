using Google.Apis.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using Taxiiii.ApiResponse;
using Taxiiii.Data;
using Taxiiii.DtoS;
using Taxiiii.Interfaces;
using Taxiiii.Models;

namespace Taxiiii.Services
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly IConnectionMultiplexer _redis;
		private readonly IConfiguration _configuration;
		private readonly IHubContext<NotificationHub> _hub;
		

		public UserService(IConnectionMultiplexer redis, AppDbContext context, IConfiguration configuration , IHubContext<NotificationHub> hub)
		{
			_context = context;
			_configuration = configuration;
			_hub = hub;
			_redis = redis;
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
			var result = await _context.SaveChangesAsync();

			var User = await _context.RigesterUsers.FindAsync(userId);
			var UserName = User.FirstName + User.LastName; 
			var UserPhone = User.PhoneNumber;

			var driver = await _context.Drives
				.FirstOrDefaultAsync(d => d.Id == trip.DriverId);

			if (result > 0 && driver != null)
			{
				Console.WriteLine(driver.UserId.ToString());

				var message =
					$"تم إلغاء الرحلة بواسطة المستخدم {UserName}\n" +
					$"📞 الهاتف: {UserPhone}\n" +
					$"🚗 السبب : {Reason}\n";

		
				var notification = new Notification
				{
					UserId = driver.UserId,
					TripId = trip.TripId,
					Title = "Ride Cancelled",
					Message = message,
					Type = NotificationType.RideCancelled,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_context.Notifications.Add(notification);
				await _context.SaveChangesAsync();

			
				await _hub.Clients.Group(driver.UserId.ToString())
					.SendAsync("ReceiveNotification", message);
			}
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
		public async Task<ApiResponse<string>> GetNearbyDrivers(int UserId)
		{
			const double radiusKm = 5;
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
			var db = _redis.GetDatabase();

			var nearbyDrivers = await db.GeoRadiusAsync(
				"drivers:locations",
				locationUser.Longitude,
				locationUser.Latitude,
				radiusKm,
				GeoUnit.Kilometers,
				order: Order.Ascending
				);
			Console.WriteLine($"Nearby Drivers Count: {nearbyDrivers.Length}");

			foreach (var driver in nearbyDrivers)
			{
				Console.WriteLine(
					$"DriverId: {driver.Member}, Distance: {driver.Distance} KM"
				);
			}

			if (nearbyDrivers.Length == 0)
			{
				return new ApiResponse<string>
				{
					Success = true,
					Message = "No nearby drivers found",
					Data = "[]"
				};
			}

			var driverIds = nearbyDrivers.Select(x => int.Parse(x.Member.ToString())).ToList();

			var driverinfo  = await _context.Drives.Include(d=>d.User)
				.Where(d =>
			driverIds.Contains(d.Id) &&
			d.DriverAvailabilityStatu == DriverAvailabilityStatus.Online &&
			d.User.Role == UserRole.Driver.ToString()).ToListAsync();


			var result = nearbyDrivers.Where(d => driverinfo.Any(x => x.Id == int.Parse(d.Member.ToString()))).
				Select(d=>
			{
				var driverId = int.Parse(d.Member.ToString());
				var driver = driverinfo.First(x => x.Id == driverId);

				return new
				{
					DriverId = driverId,
					DistanceKm = d.Distance,
					DriverName = driver.User.FirstName + " " + driver.User.LastName,
					Rating = driver.Rating
				};
			}).Take(10)
		         .ToList();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Nearby drivers retrieved successfully.",
				Data = result != null ? System.Text.Json.JsonSerializer.Serialize(result) : "[]"
			};
		}
		public async Task<ApiResponse<string>> SelectDriver (int driverId, int tripId)
		{
			var trip = await _context.Trips.FirstOrDefaultAsync(d => d.TripId == tripId &&
			 d.Status == TripStatus.Pending
			  );
			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found or not Pending"
				};
			}
			var DriverInfo = await	_context.Drives
				.Where
				(d => d.Id == driverId && d.DriverAvailabilityStatu ==
			DriverAvailabilityStatus.Online).ExecuteUpdateAsync(
				  s => s.SetProperty( d=> d.DriverAvailabilityStatu , DriverAvailabilityStatus.Busy)); 
			   
			if (DriverInfo == 0) 
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver is not available"
				};
			}
			trip.DriverId = driverId; 
			
			await _context.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Driver selected successfully",
				Data = driverId.ToString()
			};

		}
	}



}


