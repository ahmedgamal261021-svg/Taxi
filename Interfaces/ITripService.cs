using Taxiiii.DtoS;
using Taxiiii.ApiResponse;
using Taxiiii.Models;

namespace Taxiiii.Interfaces
{
	public interface ITripService
	{
		Task<ApiResponse<int>> CreaeteTrip(CreateTripDto createTripDto, int userId);
		Task<ApiResponse<string>> UpdateTrip(CreateTripDto createTripDto, int TripId, int userId);
		Task<ApiResponse<string>> CancelTrip(int TripId , int userId, string reason);

		Task<ApiResponse<List<TripResponseDto>>> GetUserTripsAsync(int userId);
		Task<ApiResponse<List<TripResponseDto>>> available_trips(); 
		Task<ApiResponse<Trip>> GetTripsById(int TripId, int UserId); 
		Task<ApiResponse<List<Trip>>> GetTripsByHistory(); 



	}
}
