using Taxiiii.ApiResponse;
using Taxiiii.Models;
namespace Taxiiii.Interfaces
{
	public interface IPendingTripQueue
	{
		ValueTask QueueAsync(int tripId);

		ValueTask<int> DequeueAsync(
			CancellationToken cancellationToken);
	}
}
