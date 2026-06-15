using System;
using System.Collections.Generic;
using Taxiiii.ApiResponse;
using Taxiiii.DtoS;
using Taxiiii.Models;

namespace Taxiiii.Interfaces
{
	public interface IDriverService
	{
		Task  <ApiResponse<string>>  ApplyDriver(ApplyDriverDto applyDriverDto, int userId);
		Task<ApiResponse<string>> AssignedCarToDriver(AssignedCar AssCar, int userId); 
		Task<ApiResponse<DriverInfoDto>> InformationDrive (int userId) ;
		Task<ApiResponse<string>> LocationDriveAsync(UpdateLocationDto LocDto, int userId);

		Task<ApiResponse<string>> SetStautueDriverByDriver(string Staute, int userId); 
		Task<ApiResponse<string>> AcceptTrip(int tripId, int userId);
		Task<ApiResponse<string>> StartTrip(int tripId, int userId);
		Task<ApiResponse<string>> completeTrip(int tripId, int userId);
		Task<ApiResponse<string>> CancelTripByDriver(int tripId, string reason,int userId);	

		Task<ApiResponse<string>> currentTrip(int userId);
		Task<ApiResponse<List<Models.Trip>>> GetTripTheDriveByHistory(int userId);


		Task<ApiResponse<List<DriverCarDto>>> GetAllCarsByDriver(int userId);









	}
}
