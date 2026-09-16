using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taxiiii.ApiResponse;
using Taxiiii.Services;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Taxiiii.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		protected readonly NotificationService _notificationService;

		public NotificationController(NotificationService notificationService) 
		{

			_notificationService = notificationService;

		}


		// GET: api/<NotificationController>
		//[HttpGet]
		//public async Task<IActionResult>  GetUserNotifications()
		//{
			
		//	var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
		//	if (userId == 0)
		//	{
		//		return Unauthorized(new ApiResponse<string>
		//		{
		//			Success = false,
		//			Message = "Unauthorized"
		//		});
		//	}
		//	var result = await _notificationService.GetUserNotifications(userId);


		//	return Ok(result);
		//}


		//// GET api/<NotificationController>/5
		//[HttpGet("{id}")]
		//public string Get(int id)
		//{
		//	return "value";
		//}

		//// POST api/<NotificationController>
		//[HttpPost]
		//public void Post([FromBody] string value)
		//{
		//}

		//// PUT api/<NotificationController>/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//}

		//// DELETE api/<NotificationController>/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//}
	}
}
