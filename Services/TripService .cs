using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using Taxiiii.ApiResponse;
using Taxiiii.Data;
using Taxiiii.DtoS;
using Taxiiii.Interfaces;
using Taxiiii.Migrations;
using Taxiiii.Models;
namespace Taxiiii.Services
{
	public class TripService : ITripService
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;

		public TripService(AppDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ApiResponse<int>> CreaeteTrip(CreateTripDto dto, int userId)
		{
			var user = await _context.RigesterUsers
		.FirstOrDefaultAsync(x => x.UserId == userId);

			if (user == null)
			{
				return new ApiResponse<int>
				{
					Success = false,
					Message = "User not found"
				};
			}

			if (user.Role == UserRole.Driver.ToString())
			{
				return new ApiResponse<int>
				{
					Success = false,
					Message = "Drivers cannot create trips"
				};
			}

			var trip = new Models.Trip
			{

				CreatedAt = DateTime.UtcNow,
				StartLocation = dto.StartLocation,
				EndLocation = dto.EndLocation,
				DistanceKm = dto.DistanceKm,
				Price = (decimal)dto.DistanceKm * 8,
				DurationMinutes = (int)(dto.DistanceKm / 40 * 60),
				UserId = userId,
				StartTime = DateTime.UtcNow

				// Set start time to 10 minutes from now 
			};

			_context.Trips.Add(trip);

			await _context.SaveChangesAsync();

			return new ApiResponse<int>
			{
				Success = true,
				Message = "Trip created successfully",
				Data = trip.TripId
			};
		}
		public async Task<ApiResponse<string>> UpdateTrip(CreateTripDto dto, int userId, int TripId)
		{
			var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == TripId && t.UserId == userId);
			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found or you don't have permission to update it",
					Data = null
				};
			}
			if (trip.Status != TripStatus.Pending)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip cannot be updated"
				};
			}
			trip.StartLocation = dto.StartLocation;
			trip.EndLocation = dto.EndLocation;
			trip.DistanceKm = dto.DistanceKm;
			trip.Price = (decimal)dto.DistanceKm * 8;
			trip.DurationMinutes = (int)(dto.DistanceKm / 40 * 60);
			await _context.SaveChangesAsync();

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip updated  successfully",

			};
		}

		public async Task<ApiResponse<string>> CancelTrip(int TripId, int userId, string reason)
		{
			var trip = await _context.Trips.FirstOrDefaultAsync(t => t.TripId == TripId && t.UserId == userId);
			if (trip == null)
			{
				Console.WriteLine($"TripId: {TripId}");
				Console.WriteLine($"UserId From Token: {userId}");
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found or you don't have permission to cancel it",
					Data = null
				};
			}
			if (trip.Status != TripStatus.Pending)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip cannot be cancelled"
				};
			}
			trip.Status = TripStatus.Cancelled;
			trip.CancelReason = reason;
			await _context.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip cancelled successfully",
			};
		}

		public async Task<ApiResponse<List<TripResponseDto>>> GetUserTripsAsync(int userId)
		{
			var trips = await _context.Trips.Where(t => t.UserId == userId).OrderByDescending(x => x.CreatedAt)
				.Select(t => new TripResponseDto
				{
					TripId = t.TripId,
					StartLocation = t.StartLocation,
					EndLocation = t.EndLocation,
					DistanceKm = t.DistanceKm,
					UserName = t.User.FirstName + " " + t.User.LastName,
					DurationMinutes = t.DurationMinutes,
					Price = t.Price,
					CreatedAt = t.CreatedAt
				}).ToListAsync();

			if (trips == null || trips.Count == 0)
			{
				return new ApiResponse<List<TripResponseDto>>
				{
					Success = false,
					Message = "No trips found for this user",
					Data = null
				};
			}
			return new ApiResponse<List<TripResponseDto>>
			{
				Success = true,
				Message = "Trips retrieved successfully",
				Data = trips
			};

		}

		public async Task<ApiResponse<List<TripResponseDto>>> available_trips()
		{
			var trips = await _context.Trips.Where(t => t.Status == TripStatus.Pending)
				.Select(t => new TripResponseDto
				{
					TripId = t.TripId,
					StartLocation = t.StartLocation,
					EndLocation = t.EndLocation,
					DistanceKm = t.DistanceKm,
					Status = t.Status.ToString(),
					UserName = t.User.FirstName + " " + t.User.LastName,
					DurationMinutes = t.DurationMinutes,
					Price = t.Price,
					CreatedAt = t.CreatedAt
				}).ToListAsync();
			if (trips == null || trips.Count == 0)
			{
				return new ApiResponse<List<TripResponseDto>>
				{
					Success = false,
					Message = "No available trips found",
					Data = null
				};
			}
			return new ApiResponse<List<TripResponseDto>>
			{
				Success = true,
				Message = "Available trips retrieved successfully",
				Data = trips
			};

		}

		public async Task<ApiResponse<Models.Trip>> GetTripsById(int TripId, int UserId)
		{

			var user = await _context.RigesterUsers.FindAsync(UserId);

			if (user == null || user.Role != "Admin")
			{

				return new ApiResponse<Models.Trip>
				{
					Success = false,
					Message = "User not found Or Only admins Get this trips",
					Data = null
				};
			}
			var trip = await _context.Trips.FindAsync(TripId);
			if (trip == null)
			{
				return new ApiResponse<Models.Trip>
				{
					Success = false,
					Message = "No trips found",
					Data = null
				};
			}
			return new ApiResponse<Models.Trip>
			{
				Success = true,
				Message = "Trips retrieved successfully",
				Data = trip
			};
		}

		public async Task<ApiResponse<List<Models.Trip>>> GetTripsByHistory()
		{
			var trips = await _context.Trips.Where(t => t.Status == TripStatus.Completed || t.Status == TripStatus.Cancelled)
				.Select(t => new Models.Trip
				{
					TripId = t.TripId,
					StartLocation = t.StartLocation,
					EndLocation = t.EndLocation,
					DistanceKm = t.DistanceKm,
					Status = t.Status,
					UserId = t.UserId,
					DurationMinutes = t.DurationMinutes,
					Price = t.Price,
					CreatedAt = t.CreatedAt
				}).OrderByDescending(t => t.CreatedAt)
				.ToListAsync();
			if (trips == null || trips.Count == 0)
			{
				return new ApiResponse<List<Models.Trip>>
				{
					Success = false,
					Message = "No trip history found",
					Data = null
				};
			}
			return new ApiResponse<List<Models.Trip>>
			{
				Success = true,
				Message = "Trip history retrieved successfully",
				Data = trips 
			};
		}
	}
}
