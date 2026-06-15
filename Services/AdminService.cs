using Azure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using Taxiiii.ApiResponse;
using Taxiiii.Data;
using Taxiiii.Interfaces;
using Taxiiii.Models;
namespace Taxiiii.Services
{

	public class AdminService
	{


		protected readonly AppDbContext _Context;
		public AdminService(AppDbContext context)
		{
			_Context = context;
		}

		public async Task<ApiResponse<string>> RigesterAsAdmin(int userId)
		{
			var response = new ApiResponse<string>();

			var user = await _Context.RigesterUsers.FindAsync(userId);
			if (user == null)
			{
				response.Success = false;
				response.Message = "User not found.";
				return response;
			}
			if (user.Role == "Admin")
			{
				response.Success = false;
				response.Message = "User is already an admin.";
				return response;
			}

			user.Role = "Admin";

			try
			{
				await _Context.SaveChangesAsync();
				response.Success = true;
				response.Message = "User promoted to admin successfully.";
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"Error promoting user to admin: {ex.Message}";
			}

			return response;
		}


		public async Task<ApiResponse<List<RigesterUser>>> GetUsers()
		{
			var response = new ApiResponse<List<RigesterUser>>();
			var users = await _Context.RigesterUsers.ToListAsync();
			if (users == null || users.Count == 0)
			{
				response.Success = false;
				response.Message = "No users found.";
				return response;
			}

			response.Success = true;
			response.Message = "Users retrieved successfully.";
			response.Data = users;
			Console.WriteLine(response.Data);
			return response;


		}
		public async Task<ApiResponse<List<Drive>>> GetAllDrivers()
		{
			var response = new ApiResponse<List<Drive>>();
			var users = await _Context.Drives.Include(d => d.User).ToListAsync();
			if (users == null || users.Count == 0)
			{
				response.Success = false;
				response.Message = "No users found.";
				return response;
			}
			response.Success = true;
			response.Message = "Users retrieved successfully.";
			response.Data = users;
			return response;
		}
		public async Task<ApiResponse<string>> ApprovedDriver(int userId, int driveId)
		{
			var response = new ApiResponse<string>();
			var user = await _Context.RigesterUsers.FindAsync(userId);
			if (user == null)
			{
				response.Success = false;
				response.Message = "User not found.";
				return response;
			}
			if (user.Role != "Admin")
			{
				response.Success = false;
				response.Message = "Unauthorized. Only admins can approve drivers.";
				return response;
			}
			var driver = await _Context.Drives.FindAsync(driveId);
			if (driver == null)
			{
				response.Success = false;
				response.Message = "Driver not found.";
				return response;
			}
			if (driver.DriverStatu == DriverStatus.Approved)
			{
				response.Success = false;
				response.Message = "Driver is already approved.";
				return response;
			}
			driver.DriverStatu = DriverStatus.Approved;

			try
			{
				await _Context.SaveChangesAsync();
				response.Success = true;
				response.Message = "Driver approved successfully.";
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"Error approving driver: {ex.Message}";
			}
			return response;


		}
		public async Task<ApiResponse<string>> RejectedDriver(int userId, int driveId)
		{
			var response = new ApiResponse<string>();
			var user = await _Context.RigesterUsers.FindAsync(userId);
			if (user == null)
			{
				response.Success = false;
				response.Message = "User not found.";
				return response;
			}
			if (user.Role != "Admin")
			{
				response.Success = false;
				response.Message = "Unauthorized. Only admins can Rejected drivers.";
				return response;
			}
			var driver = await _Context.Drives.FindAsync(driveId);
			if (driver == null)
			{
				response.Success = false;
				response.Message = "Driver not found.";
				return response;
			}
			if (driver.DriverStatu == DriverStatus.Rejected)
			{
				response.Success = false;
				response.Message = "Driver is already Rejected.";
				return response;
			}
			driver.DriverStatu = DriverStatus.Rejected;

			try
			{
				await _Context.SaveChangesAsync();
				response.Success = true;
				response.Message = "Driver Rejected successfully.";
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = $"Error Rejecting driver: {ex.Message}";
			}
			return response;
		}
		public async Task<ApiResponse<List<Drive>>> GetAllDriverIsPandind(int userId)
		{
			var response = new ApiResponse<List<Drive>>();
			var user = await _Context.RigesterUsers.FindAsync(userId);
			if (user == null)
			{
				response.Success = false;
				response.Message = "User not found.";
				return response;
			}
			if (user.Role != "Admin")
			{
				response.Success = false;
				response.Message = "Unauthorized. Only admins can view pending drivers.";
				return response;
			}
			var pendingDrivers = await _Context.Drives.Where(d => d.DriverStatu == DriverStatus.Pending).ToListAsync();

			if (pendingDrivers == null)
			{
				response.Success = false;
				response.Message = "No pending drivers found.";
				return response;
			}
			response.Success = true;
			response.Message = "Drivers retrieved successfully.";
			response.Data = pendingDrivers;
			return response;

		}

		public async Task<ApiResponse<List<Trip>>> GetAlltrip(int userId)
		{
			var response = new ApiResponse<List<Trip>>();
			var user = await _Context.RigesterUsers.FindAsync(userId);
			if (user == null)
			{
				response.Success = false;
				response.Message = "User not found.";
				return response;
			}
			if (user.Role != "Admin")
			{
				response.Success = false;
				response.Message = "Unauthorized. Only admins can view trips.";
				return response;
			}
			var trips = await _Context.Trips.ToListAsync();
			if (trips == null)
			{
				response.Success = false;
				response.Message = "No trips found.";
				return response;
			}
			response.Success = true;
			response.Message = "Trips retrieved successfully.";
			response.Data = trips;
			return response;



		}
		public async Task<ApiResponse<List<int>>> DashboardToAdmin()
		{
			var res = new ApiResponse<List<int>>();
			var CountUser = await _Context.RigesterUsers.CountAsync();
			var CountDrive = await _Context.Drives.CountAsync();
			var CountPandingDrive = await _Context.Drives.Where(d => d.DriverStatu == DriverStatus.Pending)
				.CountAsync();
			var CountTrips = await _Context.Trips.CountAsync();
			var CountcompletedTrips = await _Context.Trips.Where(d => d.Status == TripStatus.Completed).CountAsync();
			var CountCancelledTrips = await _Context.Trips.Where(d => d.Status == TripStatus.Cancelled).CountAsync();
			if (CountUser == 0 || CountDrive == 0 || CountPandingDrive == 0 || CountTrips == 0 ||
				CountcompletedTrips == 0 || CountCancelledTrips == 0)
			{
				res.Success = false;
				res.Message = "No data found.";
				return res;
			}
			res.Success = true;
			res.Message = "Data retrieved successfully.";
			res.Data = new List<int>
{
	CountUser,
	CountDrive,
	CountPandingDrive,
	CountTrips,
	CountcompletedTrips,
	CountCancelledTrips
};
			return res;

		}

	}
}
