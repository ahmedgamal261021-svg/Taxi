using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taxiiii.ApiResponse;
using Taxiiii.DtoS;

namespace Taxiiii.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TripController : ControllerBase
	{
		private readonly Services.TripService _tripService;

		public TripController(Services.TripService tripService)
		{
			_tripService = tripService;
		}
		[Authorize]
		[HttpPost("createTrip")]
		public async Task<IActionResult> CreateTrip(CreateTripDto Ctdto)
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
			var result = await _tripService.CreaeteTrip(Ctdto, UserId);

			return Ok(result);
		}

		[Authorize]
		[HttpGet("myTrips")]
		public async Task<IActionResult> GetMyTrips()
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
			var result = await _tripService.GetUserTripsAsync(UserId);
			return Ok(result);


		}
		[Authorize]
		[HttpPut("updateTrip/{TripId}")]
		public async Task<IActionResult> UpdateTrip(int TripId, CreateTripDto Ctdto)
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
			var result = await _tripService.UpdateTrip(Ctdto, UserId, TripId);
			return Ok(result);
		}

		[Authorize]
		[HttpPost("cancelTrip/{TripId}")]
		public async Task<IActionResult> CancelTrip(int TripId, string reason)
		{
			var UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			Console.WriteLine($"UserId : {UserId}");
			if (UserId == null)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _tripService.CancelTrip(TripId, UserId, reason);
			return Ok(result);
		}
		[Authorize]
		[HttpGet]
		[Route("available-trips")]
		public async Task<IActionResult> GetAvailableTrips()
		{
			var result = await _tripService.available_trips();
			return Ok(result);
		}
		[Authorize]
		[HttpGet]
		[Route("GetTripsById")]
		public async Task<IActionResult> GetAnyTripsById(int TripId)
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

			var result = await _tripService.GetTripsById(TripId, UserId);

			return Ok(result);
		}
		[Authorize]
		[HttpGet]
		[Route("history")]
		public async Task<IActionResult> GetTripsByHistory()
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
			var result = await _tripService.GetTripsByHistory();
			return Ok(result);
		}
	}
}
