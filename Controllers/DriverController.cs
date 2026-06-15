using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taxiiii.ApiResponse;
using Taxiiii.DtoS;
using Taxiiii.Interfaces;

namespace Taxiiii.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DriverController : ControllerBase
	{
		protected readonly IDriverService _driverService;
		public DriverController(IDriverService driverService)
		{
			_driverService = driverService;
		}
		[Authorize]
		[HttpPost("apply")]
		public async Task<IActionResult> ApplyDrive(DtoS.ApplyDriverDto applyDriverDto)
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
			var result = await _driverService.ApplyDriver(applyDriverDto, UserId);
			return Ok(result);

		}
		[Authorize]
		[HttpPost]
		[Route("assign-car")]
		public async Task<IActionResult> AssignCarToDriver(AssignedCar Asscar)
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
			var result = await _driverService.AssignedCarToDriver(Asscar, UserId);

			return Ok(result);
		}
		[Authorize]
		[HttpGet]
		[Route("information-drive")]
		public async Task<IActionResult> InformationDrive()
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
			var result = await _driverService.InformationDrive(UserId);
			return Ok(result);
		}
		[Authorize]
		[HttpPost]
		[Route("location-drive")]
		public async Task<IActionResult> LocationDriveAsync(UpdateLocationDto LocDto)
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
			var result = await _driverService.LocationDriveAsync(LocDto, UserId);
			return Ok(result);
		}
		[Authorize]
		[HttpPut]
		[Route("stautue-drive")]
		public async Task<IActionResult> StautueDriver(string Staute)
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
			var result = await _driverService.SetStautueDriverByDriver(Staute, UserId);
			return Ok(result);
		}
		[Authorize]
		[HttpPut]
		[Route("accept-trip/{tripId}")]
		public async Task<IActionResult> AcceptTrip(int tripId)
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
			var result = await _driverService.AcceptTrip(tripId, UserId);
			return Ok(result);
		}
		[Authorize]
		[HttpPut]
		[Route("CompleteTrip")]
		public async Task<IActionResult> completeTrip(int tripId)
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
			var result = await _driverService.completeTrip(tripId, UserId);
			return Ok(result);


		}
		[Authorize]
		[HttpPut]
		[Route("StartTrip")]
		public async Task<IActionResult> StartTrip(int tripId)
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
			var result = await _driverService.StartTrip(tripId, UserId);
			return Ok(result);

		}

		[Authorize]
		[HttpPut]
		[Route("CancelTripByDriver")]
		public async Task<IActionResult> CancelTripByDriver(int tripId, string reason)
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
			var result = await _driverService.CancelTripByDriver(tripId, reason, UserId);
			return Ok(result);

		}
		[Authorize]
		[HttpGet]
		[Route("currentTrip")]
		public async Task<IActionResult> currentTrip()
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
			var result = await _driverService.currentTrip(UserId);
			return Ok(result);
		}
		[Authorize]
		[HttpGet]
		[Route("trip-history")]
		public async Task<IActionResult> GetTripTheDriveByHistory()
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
			Console.WriteLine("UserIdfdddddddddddddd" + UserId);
			var result = await _driverService.GetTripTheDriveByHistory(UserId);
			return Ok(result);

		}
		[Authorize]
		[HttpGet]
		[Route("GetAllCarsByDriver")]
		public async Task<IActionResult> GetAllCarsByDriver()
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
			var result = await _driverService.GetAllCarsByDriver(UserId);
			return Ok(result);



		}
	}
	}
