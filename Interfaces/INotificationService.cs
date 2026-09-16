using Taxiiii.ApiResponse;
using Taxiiii.DtoS;
using Taxiiii.Models;

namespace Taxiiii.Interfaces
{
	public interface INotificationService
	{
		Task<ApiResponse<List<NotificationDto>>> GetUserNotifications(); 
	}
}
