	
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Taxiiii.ApiResponse;
using Taxiiii.Data;
using Taxiiii.DtoS;
using Taxiiii.Interfaces;
using Taxiiii.Migrations;
using Taxiiii.Models;

namespace Taxiiii.Services
{
	public class DriverService : IDriverService
	{

		private readonly AppDbContext _dbcontext;
		private readonly IHubContext<RideHub> _hubContext;
		public DriverService(AppDbContext dbcontext, IHubContext<RideHub> hubContext)
		{
			_dbcontext = dbcontext;
			_hubContext = hubContext;
		}
		public async Task<ApiResponse<string>> ApplyDriver(ApplyDriverDto applyDriverDto, int userId)

		{
			var user = await _dbcontext.RigesterUsers.FirstOrDefaultAsync(a => a.UserId == userId);

			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}
			var existingApplication = await _dbcontext.Drives
			 .FirstOrDefaultAsync(d => d.UserId == userId);

			if (existingApplication != null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver application already exists",

				};
			}

			var driverApplication = new Models.Drive

			{
				UserId = userId,
				LicenseNumber = applyDriverDto.LicenseNumber,
				LicenseExpiryDate = applyDriverDto.LicenseExpiryDate,
				CreatedAt = DateTime.UtcNow,
				DriverStatu = DriverStatus.Pending,
				DriverAvailabilityStatu = DriverAvailabilityStatus.Offline
			};
			_dbcontext.Drives.Add(driverApplication);
			user.Role = UserRole.Driver.ToString();

			await _dbcontext.SaveChangesAsync();


			await Task.Delay(1000);
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Driver application submitted successfully.",
				Data = null
			};
		}
		public async Task<ApiResponse<string>> AssignedCarToDriver(AssignedCar AssCar, int userId)
		{
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			var car = await _dbcontext.Cars.FirstOrDefaultAsync(c => c.LicensePlate == AssCar.LicensePlate);
			if (car == null)
			{
				car = new Models.Car
				{
					Brand = AssCar.Brand,
					Model = AssCar.Model,
					Year = AssCar.Year,
					Color = AssCar.Color,
					LicensePlate = AssCar.LicensePlate,
					VehicleLicenseType = AssCar.VehicleLicenseType,
					CreatedAt = DateTime.UtcNow
				};

				_dbcontext.Cars.Add(car);
				await _dbcontext.SaveChangesAsync();
			}
			var existingRelation = await _dbcontext.DriverCars
			.AnyAsync(dc =>
				dc.DriverId == driver.Id &&
				dc.CarId == car.CarId &&
				dc.UnassignedAt == null);

			if (existingRelation)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Car already assigned to this driver"
				};
			}
			var driverCar = new DriverCar
			{
				DriverId = driver.Id,
				CarId = car.CarId,
				IsActive = false,
				AssignedAt = DateTime.UtcNow,
				UnassignedAt = null
			};
			await _dbcontext.DriverCars.AddAsync(driverCar);

			await _dbcontext.SaveChangesAsync();

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Driver assigned successfully.",
				Data = null
			};

		}
		public async Task<ApiResponse<DriverInfoDto>> InformationDrive(int userId)
		{
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (driver == null)
			{
				return new ApiResponse<DriverInfoDto>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			var driverInfo = new DriverInfoDto
			{
				Id = driver.Id,
				LicenseNumber = driver.LicenseNumber,
				LicenseExpiryDate = driver.LicenseExpiryDate,
				DriverStatus = driver.DriverStatu.ToString(),
				DriverAvailabilityStatu = driver.DriverAvailabilityStatu.ToString(),
				Rating = driver.Rating,
				TotalTrips = driver.TotalTrips,

			};
			return new ApiResponse<DriverInfoDto>
			{
				Success = true,
				Message = "Driver information retrieved successfully.",
				Data = driverInfo
			};
		}
		public async Task<ApiResponse<string>> LocationDriveAsync(UpdateLocationDto LocDto, int userId)
		{
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			var location = await _dbcontext.DriverLocations
	 .FirstOrDefaultAsync(x => x.DriverId == driver.Id);


			if (location == null)
			{
				location = new DriverLocation
				{
					DriverId = driver.Id,
					Latitude = LocDto.Latitude,
					Longitude = LocDto.Longitude,
					CreateAt = DateTime.UtcNow
				};

				_dbcontext.DriverLocations.Add(location);
			}
			else
			{
				location.Latitude = LocDto.Latitude;
				location.Longitude = LocDto.Longitude;
				location.CreateAt = DateTime.UtcNow;
			}

			await _dbcontext.SaveChangesAsync();

			await _hubContext.Clients.All.SendAsync(
	"DriverLocationUpdated",
	driver.Id,
	LocDto.Latitude,
	LocDto.Longitude);
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Location updated successfully.",
				Data = null
			};
		}
		public async Task<ApiResponse<string>> SetStautueDriverByDriver(string status, int userId)
		{
			var driver = await _dbcontext.Drives
				.FirstOrDefaultAsync(d => d.UserId == userId);

			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}

			if (!Enum.TryParse(status, true, out DriverAvailabilityStatus availabilityStatus))
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = $"Invalid status. Allowed values:" +
					$" {string.Join(", ", Enum.GetNames(typeof(DriverAvailabilityStatus)))}"
				};
			}

			driver.DriverAvailabilityStatu = availabilityStatus;

			await _dbcontext.SaveChangesAsync();

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Driver status updated successfully."
			};
		}
		public async Task<ApiResponse<string>> AcceptTrip(int tripId, int userId)
		{
			var user = await _dbcontext.RigesterUsers
	.FirstOrDefaultAsync(x => x.UserId == userId);

			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}

			if (user.Role == UserRole.User.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User cannot Accept trips"
				};
			}
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			var trip = await _dbcontext.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found"
				};
			}
			if (trip.Status != TripStatus.Pending)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip is not available for acceptance"
				};
			}
			trip.Status = TripStatus.Accepted;
			trip.DriverId = driver.Id;
			trip.UserId = driver.UserId;
			driver.DriverAvailabilityStatu = DriverAvailabilityStatus.Busy;
			await _dbcontext.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip accepted successfully."
			};

		}
		public async Task<ApiResponse<string>> StartTrip(int tripId, int userId)
		{
			var user = await _dbcontext.RigesterUsers
	.FirstOrDefaultAsync(x => x.UserId == userId);

			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}

			if (user.Role == UserRole.User.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User cannot Start trips"
				};
			}
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (user.Role != UserRole.Driver.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found Must Be Apply On The jop "
				};
			}
			var trip = await _dbcontext.Trips.FirstOrDefaultAsync(t => t.TripId == tripId);
			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found"
				};
			}
			if (trip.Status != TripStatus.Accepted)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip is not in a state to be started"
				};
			}
			trip.Status = TripStatus.Started;
			driver.DriverAvailabilityStatu = DriverAvailabilityStatus.OnTrip;
			trip.StartTime = DateTime.UtcNow;
			await _dbcontext.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip started successfully."
			};

		}

		public async Task<ApiResponse<string>> completeTrip(int tripId, int userId)
		{
			var user = await _dbcontext.RigesterUsers
	.FirstOrDefaultAsync(x => x.UserId == userId);

			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}

			if (user.Role == UserRole.User.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User cannot Complet trips"
				};
			}
			var drive = await _dbcontext.Drives.FirstOrDefaultAsync(s => s.UserId == userId);
			if (drive == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Drive Not found"
				};

			}
			var trip = await _dbcontext.Trips.FirstOrDefaultAsync(d => d.TripId == tripId);
			if (drive == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found"
				};
			}
			if (trip.Status != TripStatus.Started)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip is not in a state to be started"
				};
			}
			trip.Status = TripStatus.Completed;
			trip.EndTime = DateTime.UtcNow;
			drive.DriverAvailabilityStatu = DriverAvailabilityStatus.Online;
			await _dbcontext.SaveChangesAsync();
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip started successfully."
			};



		}

		public async Task<ApiResponse<string>> CancelTripByDriver(int tripId, string Reason, int userId)
		{
			var Trip = await _dbcontext.Trips.FirstOrDefaultAsync(x => x.TripId == tripId);
			if (Trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip not found"
				};
			}
			var driver = await _dbcontext.Drives
		.FirstOrDefaultAsync(d => d.UserId == userId);

			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			Console.WriteLine("driver.Id =" + driver.Id);
			Console.WriteLine("Trip.DriverId=" + Trip.DriverId);
			if (Trip.DriverId != driver.Id)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "You don't have permission to cancel this trip"
				};
			}
			var userRole = await _dbcontext.RigesterUsers.FirstOrDefaultAsync(d => d.UserId == userId);
			if (userRole.Role != UserRole.Driver.ToString())
			{
				return new ApiResponse<string>

				{
					Success = false,
					Message = "You don't have permission to cancel this tripsssssss"
				};
			}
			if (Trip.Status != TripStatus.Accepted)
			{

				return new ApiResponse<string>
				{
					Success = false,
					Message = "Trip cannot be cancelled at this stage"
				};

			}
			Trip.Status = TripStatus.Cancelled;
			Trip.CancelReason = Reason;
			await _dbcontext.SaveChangesAsync();

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Trip started successfully."
			};
		}

		public async Task<ApiResponse<string>> currentTrip(int userId)
		{
			var user = await _dbcontext.RigesterUsers.FirstOrDefaultAsync(s => s.UserId == userId);
			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}
			if (user.Role == UserRole.User.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User cannot view current trip"
				};
			}
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(s => s.UserId == userId);
			if (driver == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver not found"
				};
			}
			var trip = await _dbcontext.Trips.FirstOrDefaultAsync(s => s.DriverId == driver.Id &&
			s.Status == TripStatus.Started &&
			s.Driver.DriverAvailabilityStatu == DriverAvailabilityStatus.OnTrip);
			if (trip == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "No current trip found"
				};
			}
			return new ApiResponse<string>
			{
				Success = true,
				Message = "Current trip retrieved successfully.",
				Data = $"NameUser {user.FirstName + user.LastName},Trip ID: {trip.TripId}, Status: {trip.Status}, Start Time: {trip.StartTime} , " +
				$" DistanceKm {trip.DistanceKm} ,StartLocation {trip.StartLocation} , EndLocation {trip.EndLocation},"


			};
		}

		public async Task<ApiResponse<List<Models.Trip>>> GetTripTheDriveByHistory(int userId)
		{
			var response = new ApiResponse<List<Models.Trip>>();

			var User = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (User == null)
			{
				response.Success = false;
				response.Message = "UserNotFound";
				return response;

			}
			var trips = await _dbcontext.Trips
				.Where(t => t.Driver.Id == User.Id)
				.Include(t => t.Driver).OrderByDescending(t => t.CreatedAt)
				.ToListAsync();
			if (trips == null)
			{
				response.Success = false;
				response.Message = "Trip Not Found";
				return response;

			}

			response.Success = true;
			response.Message = "Trips retrieved successfully.";
			response.Data = trips;
			return response;
		}


		public async Task<ApiResponse<List<DriverCarDto>>> GetAllCarsByDriver(int userId)
		{
			var response = new ApiResponse<List<DriverCarDto>>();

			var User = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId);
			if (User == null)
			{
				response.Success = false;
				response.Message = "UserNotFound";
				return response;
			}
			var driverCars = await _dbcontext.DriverCars
	.Where(dc => dc.DriverId == User.Id)
	.Select(dc => new DriverCarDto
	{
		DriverCarId = dc.DriverCarId,

		DriverId = dc.DriverId,
		UserId = dc.Driver.UserId,
		Rating = dc.Driver.Rating,
		TotalTrips = dc.Driver.TotalTrips,

		CarId = dc.CarId,
		Model = dc.Cars.Model,
		Brand = dc.Cars.Brand,
		Year = dc.Cars.Year,
		Color = dc.Cars.Color,
		LicensePlate = dc.Cars.LicensePlate,
		VehicleLicenseType = dc.Cars.VehicleLicenseType,

		IsAvailableForDriver = dc.IsAvailableForDriver,
		IsActive = dc.IsActive,
		AssignedAt = dc.AssignedAt,
		UnassignedAt = dc.UnassignedAt
	})
	.ToListAsync();
			if (driverCars == null || driverCars.Count == 0)
			{
				response.Success = false;
				response.Message = "No cars found for this driver.";
				return response;
			}

			response.Success = true;
			response.Message = "Cars retrieved successfully.";
			response.Data = driverCars;
			return response;


		}
	}
}
