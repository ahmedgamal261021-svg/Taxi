using Taxiiii.ApiResponse;
using Taxiiii.Models;
namespace Taxiiii.Interfaces
{
	public interface IAdminService
	{
		Task<ApiResponse<string>> RigesterAsAdmin(int usrId); 

		Task<ApiResponse<List<RigesterUser>>> GetAllUsers();
        Task<ApiResponse<List<Drive>>> GetAllDriver();  
        Task<ApiResponse<List<Drive>>> GetAllDriverIsPandind();  
        Task<ApiResponse<List<Trip>>> GetAlltrip();  
        Task<ApiResponse<List<Drive>>> ApprovedDriver(int userId, int driveId);  
        Task<ApiResponse<List<Drive>>> RejectedDriver(int userId, int driveId);
		Task<ApiResponse<List<int>>> DashboardToAdmin();










	}
}
