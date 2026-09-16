using Taxiiii.Data;
using Taxiiii.ApiResponse;
using Taxiiii.Models;
using Microsoft.EntityFrameworkCore;
using Taxiiii.DtoS;

namespace Taxiiii.Services
{
	public class NotificationService
	
	{
		protected readonly AppDbContext _appDbContext;


		public NotificationService(AppDbContext _dbContext)
		{
		 _appDbContext = _dbContext;
		
		}


		public async Task<ApiResponse<List<NotificationDto>>> GetUserNotifications(int userId)
		{
			var res = new ApiResponse<List<NotificationDto>>();

			var user = await _appDbContext.RigesterUsers.FindAsync(userId);

			if (user == null)
			{
				res.Success = false;
				res.Message = "User not Found";
				return res;
			}

			var notifications = await _appDbContext.Notifications
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.CreatedAt).Select(x => new NotificationDto
				{
					Id = x.Id,
					Title = x.Title,
					Message = x.Message,
					IsRead = x.IsRead,
                    Type = x.Type.ToString(), 
		CreatedAt = x.CreatedAt


				})
				.ToListAsync();

			if (!notifications.Any())
			{
				res.Success = false;
				res.Message = "No Notifications Found";
				res.Data = new List<NotificationDto>();
				return res;
			}

			res.Success = true;
			res.Message = "Notifications retrieved successfully";
			res.Data = notifications;

			return res;
		}
	}

}
