using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taxiiii.ApiResponse;
using Taxiiii.Services;

namespace Taxiiii.Controllers
{
	// Fixed the property name from "Role" to "Roles"
	[Route("api/[controller]")]
	[ApiController]
	public class AdminController : ControllerBase
	{
		protected readonly AdminService _adminService;
		public AdminController(AdminService adminService)
		{
			_adminService = adminService;


		}
		[Authorize]
		[Route("MakeAdmin")]
		[HttpPost]
		public async Task<IActionResult> MakeAdmin()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
				Console.WriteLine(userId + "sssssddddddddddddddddddddd");
			}
			Console.WriteLine(userId+"ssssssssssssssssss");

			var result = await _adminService.RigesterAsAdmin(userId);
			if (result.Success)
			{
				return Ok(result.Message);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}

		[Authorize]
		[Route("GetAllUsers")]
		[HttpGet]
		public async Task<IActionResult> GetAllUsers()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.GetUsers();

			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}
		[Route("GetAllDrivers")]
		[HttpGet]
		public async Task<IActionResult> GetAllDrivers()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.GetAllDrivers();
			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}

		[Route("ApprovedDriver")]
		[HttpPut]
		public async Task<IActionResult> ApprovedDriver (int driveId)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.ApprovedDriver(userId, driveId);
			if (result.Success)
			{
				return Ok(result.Message);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}
		[Route("RejectedDriver")]
		[HttpPut]
		public async Task<IActionResult> RejectedDriver(int driveId)
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.RejectedDriver(userId, driveId);
			if (result.Success)
			{
				return Ok(result.Message);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}
		[Route("GetAllDriverIsPandind")]
		[HttpGet]
		public async Task<IActionResult> GetAllDriverIsPandind()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.GetAllDriverIsPandind(userId);
			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}
		[Route("GetAlltrip")]
		[HttpGet]
		public async Task<IActionResult> GetAlltrip()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
			if (userId == 0)
			{
				return Unauthorized(new ApiResponse<string>
				{
					Success = false,
					Message = "Unauthorized"
				});
			}
			var result = await _adminService.GetAlltrip(userId);
			if (result.Success)
			{
				return Ok(result);
			}
			else
			{
				return BadRequest(result.Message);
			}
		}

	}
}
