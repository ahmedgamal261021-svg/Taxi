using Taxiiii.ApiResponse;
using Taxiiii.DtoS;

namespace Taxiiii.Interfaces
{
	public interface IUserService
	{
		Task<ApiResponse<string>> RegisterAsync(RegisterDto dto);

		Task<ApiResponse<string>> LoginAsync(LoginDto dto);

		Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
		Task<ApiResponse<string>> UserCanncelTrip(string Reason , int userId, int tripId);

		Task<ApiResponse<string>> GetNearbyDrivers(int userId, double radiusKm);
		Task<ApiResponse<string>> MakeRateToDrive(int tripId, int userId, int Rate);
		
	}

}
