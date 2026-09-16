	
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using StackExchange.Redis;
using System.Text.Json;
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
		private readonly IHubContext<NotificationHub> _hub;
		private readonly IConnectionMultiplexer _redis;
		public DriverService(IConnectionMultiplexer redis,  AppDbContext dbcontext, IHubContext<RideHub> hubContext , IHubContext<NotificationHub> hub)
		{
			_dbcontext = dbcontext;
			_hubContext = hubContext;
			_hub = hub;
			_redis = redis;
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
		public async Task<ApiResponse<string>> LocationDriveAsync(UpdateLocationDto LocDto,int userId)
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
			var trip = await _dbcontext.Trips.FirstOrDefaultAsync(t => t.DriverId == driver.Id &&
		t.Status == TripStatus.Accepted);
			
			Console.WriteLine($"Driver ID: {driver.Id}");
			Console.WriteLine($"Trip ID: {trip?.TripId}");
			Console.WriteLine($"Trip Status: {trip?.Status}");
			Console.WriteLine($"Trip UserId: {trip?.UserId}");
			
			if (trip != null)
			{
				Console.WriteLine($"Sending location to trip-{trip.TripId}");
				await _hubContext.Clients
					.Group($"trip-{trip.TripId}")
					.SendAsync("DriverLocationUpdated", new
					{
						DriverId = driver.Id,
						TripId = trip.TripId,
						Latitude = LocDto.Latitude,
						Longitude = LocDto.Longitude
					});
				Console.WriteLine("Location sent successfully");
			}

			var location = new
			{
				DriverId = driver.Id,
				Latitude = LocDto.Latitude,
				Longitude = LocDto.Longitude,
				CreateAt = DateTime.UtcNow
			};

			 
			var db = _redis.GetDatabase();

			await db.StringSetAsync(
				$"driver:{driver.Id}",
				JsonSerializer.Serialize(location), 
				TimeSpan.FromMinutes(2)
			);
			await db.GeoAddAsync(
	"drivers:locations",
	LocDto.Longitude,
	LocDto.Latitude,
	driver.Id.ToString()
);


			// ⚡ 2. Real-time update via SignalR (NO DB load)
			await _hubContext.Clients
				.Group($"driver-{driver.Id}")
				.SendAsync("DriverLocationUpdated", new
				{
					Latitude = LocDto.Latitude,
					Longitude = LocDto.Longitude
				});

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Location updated in Redis + SignalR"
			};

		}
		public async Task<ApiResponse<string>> GetDriverLocation(int driverId)
		{
			var db = _redis.GetDatabase();

			var key = $"driver:{driverId}";

			var location = await db.StringGetAsync(key);

			if (location.IsNullOrEmpty)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "Driver location not found"
				};
			}

			return new ApiResponse<string>
			{
				Success = true,
				Message = "Driver location" ,
				Data = location.ToString()
			};
		}
		//		await _hubContext.Clients.All.SendAsync(
		//	"DriverLocationUpdated",
		//driver.Id,
		//LocDto.Latitude,
		//LocDto.Longitude);
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
			var user = await _dbcontext.RigesterUsers.FirstOrDefaultAsync(x => x.UserId == userId);


			if (user == null)
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User not found"
				};
			}

			if (user.Role == UserRole.User.ToString() || user.Role == UserRole.Admin.ToString())
			{
				return new ApiResponse<string>
				{
					Success = false,
					Message = "User Or Admin cannot Accept trips"
				};
			}
			var driver = await _dbcontext.Drives.FirstOrDefaultAsync(d => d.UserId == userId &&
			 d.DriverAvailabilityStatu ==
							DriverAvailabilityStatus.Online);
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
			//trip.UserId = driver.UserId;
			driver.DriverAvailabilityStatu = DriverAvailabilityStatus.Busy;
			var result = await _dbcontext.SaveChangesAsync();
			var driverName = driver.User.FirstName + driver.User.LastName;
			var driverPhone = driver.User.PhoneNumber;
			var driverCar = await _dbcontext.DriverCars.Include(dc => dc.Cars)
	.FirstOrDefaultAsync(dc => dc.DriverId == driver.Id);


			var modelCar = driverCar?.Cars?.Brand;
			var carNumber = driverCar?.Cars?.Color;

			if (result > 0)
			{
				Console.WriteLine(trip.UserId.ToString());

				var message =
					$"تم قبول الرحلة بواسطة السائق {driverName}\n" +
					$"📞 الهاتف: {driverPhone}\n" +
					$"🚗 السيارة: {modelCar}\n" +
					$"🔢 رقم السيارة: {carNumber}";

			
				var notification = new Notification
				{
					UserId = trip.UserId,
					TripId = trip.TripId,
					Title = "Ride Accepted",
					Message = message,
					Type = NotificationType.RideAccepted,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_dbcontext.Notifications.Add(notification);
				await _dbcontext.SaveChangesAsync();
				await _hubContext.Clients
	.Group($"user-{trip.UserId}")
	.SendAsync(
		      "TripAccepted", message);
	
				await _hub.Clients.Group(trip.UserId.ToString())
					.SendAsync("ReceiveNotification", message);
			}
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
			var result = await _dbcontext.SaveChangesAsync();

			var driverName = driver.User.FirstName + " " + driver.User.LastName;
			var driverPhone = driver.User.PhoneNumber;

			if (result > 0)
			{
				Console.WriteLine(Trip.UserId.ToString());

				var message =
					$"تم إلغاء الرحلة بواسطة السائق {driverName}\n" +
					$"📞 الهاتف: {driverPhone}\n" +
					$"🚗 السبب : {Reason}\n";

		
				var notification = new Notification
				{
					UserId = Trip.UserId,
					TripId = Trip.TripId,
					Title = "Ride Cancelled",
					Message = message,
					Type = NotificationType.RideCancelled,
					IsRead = false,
					CreatedAt = DateTime.UtcNow
				};

				_dbcontext.Notifications.Add(notification);
				await _dbcontext.SaveChangesAsync();

			
				await _hub.Clients.Group(Trip.UserId.ToString())
					.SendAsync("ReceiveNotification", message);
			}

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
//public async Task SyncRedisToSql()
//{
//	var db = _redis.GetDatabase();

//	var keys = _redis.GetServer("localhost:6379")
//		.Keys(pattern: "driver:*");

//	foreach (var key in keys)
//	{
//		var data = await db.StringGetAsync(key);

//		var location = JsonSerializer.Deserialize<DriverLocation>(data);

//		await _dbcontext.DriverLocations
//			.Where(x => x.DriverId == location.DriverId)
//			.ExecuteUpdateAsync(s => s
//				.SetProperty(x => x.Latitude, location.Latitude)
//				.SetProperty(x => x.Longitude, location.Longitude)
//				.SetProperty(x => x.CreateAt, DateTime.UtcNow)
//			);
//	}
//}
