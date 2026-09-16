using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis; 
using Taxiiii.Data;
using Taxiiii.Interfaces;
using Taxiiii.Models;

using Microsoft.AspNetCore.SignalR;
namespace Taxiiii.Services

{
	public class PendingTripService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IConnectionMultiplexer _ride;
		private readonly IPendingTripQueue _queue;
		private readonly IHubContext<RideHub> _hub;
		public PendingTripService(IHubContext<RideHub> hub, IPendingTripQueue queue, IConnectionMultiplexer ride, IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory;
			_ride = ride;
			_queue = queue;
			_hub = hub;
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var tripId = await _queue.DequeueAsync(stoppingToken);

					Console.WriteLine(
						$"📥 Received Trip: {tripId}"
					);
				_ = MonitorTripAsync(tripId, stoppingToken);
				}
				catch (Exception ex)
				{
					Console.WriteLine(
						$"❌ PendingTripService Error: {ex.Message}"
					);
				}


			}

		}
		private async Task MonitorTripAsync(int tripId,CancellationToken stoppingToken)
		{

			HashSet<int> previousDriverIds = new();
			using var scop = _scopeFactory.CreateScope();

			var context = scop.ServiceProvider.GetRequiredService<AppDbContext>();

			var pendingTrip = await context.Trips
			  .FirstOrDefaultAsync(
				t => t.Status == TripStatus.Pending && t.TripId == tripId
			  , stoppingToken);

			if (pendingTrip == null)
			{
				Console.WriteLine(
					$"❌ Trip {tripId} not found or not Pending"
				);

				return;
			}
			var Locationuser = await context.UserLocation
					   .FirstOrDefaultAsync(d => d.UserId == pendingTrip.UserId,
					   stoppingToken);
			if (Locationuser == null)
			{
				Console.WriteLine($"❌ Location not found for User: {pendingTrip.UserId}"
				  );
				return;

			}
			while (!stoppingToken.IsCancellationRequested)
			{
				var currentTrip = await context.Trips.AsNoTracking()
				  .FirstOrDefaultAsync(
					  t => t.TripId == tripId,
					  stoppingToken);

				if (currentTrip == null)
				{
					Console.WriteLine(
						$"❌ Trip {tripId} no longer exists"
					);

					return;
				}

				if (currentTrip.DriverId != null)
				{
					Console.WriteLine(
						$"🛑 Driver {currentTrip.DriverId} selected for Trip {tripId}"
					);

					return;
				}
				var db = _ride.GetDatabase();

				var driverRad = await db.GeoRadiusAsync(
					"drivers:locations",
					Locationuser.Longitude,
					Locationuser.Latitude,
					5,
					GeoUnit.Kilometers
				);

				var driverId = driverRad
					.Select(x => int.Parse(x.Member.ToString()))
					.ToList();

				var driverOnline = await context.Drives
					.Where(x =>
						driverId.Contains(x.Id) &&
						x.DriverAvailabilityStatu ==
							DriverAvailabilityStatus.Online
					)
					.ToListAsync(stoppingToken);

				var currentDriverIds = driverOnline
				.Select(x => x.Id)
				.ToHashSet();
				if (!currentDriverIds.SetEquals(previousDriverIds))
				{
					Console.WriteLine("🔄 Driver list changed");

					await _hub.Clients
						.Group($"user-{pendingTrip.UserId}")
						.SendAsync(
							"NearbyDrivers",
							driverOnline.Select(driver =>
							{
								var location = driverRad
									.First(x =>
										x.Member.ToString() ==
										driver.Id.ToString());

								return new
								{
									DriverId = driver.Id,
									DistanceKm = location.Distance ,
									TripId = currentTrip.TripId

								};
							}).ToList(),
							stoppingToken
						);
				}
				previousDriverIds = currentDriverIds;

				Console.WriteLine(
					$"🟢 Nearby + Online Drivers: {driverOnline.Count}"
				);

				foreach (var driver in driverOnline)
				{
					var location = driverRad
						.First(x => x.Member.ToString() == driver.Id.ToString());

					Console.WriteLine(
						$"Driver ID: {driver.Id} | " +
						$"Distance: {location.Distance} km"
					);
				}

				if (driverOnline.Any())
				{
					Console.WriteLine(
						$"🚕 Driver found for Trip {tripId}"
					);


				}
				if (!driverOnline.Any())
				{
					Console.WriteLine(
					$"⏳ No driver found for Trip {tripId}. " +
					$"Retrying in 5 seconds..."
				);



				}


				await Task.Delay(
					TimeSpan.FromSeconds(5),
					stoppingToken
				);
			}




		}
	}
}
