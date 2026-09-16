using System.Threading.Channels;
using Taxiiii.Interfaces;

namespace Taxiiii.Services
{
	public class PendingTripQueue : IPendingTripQueue
	{
		private readonly Channel<int> _queue = Channel.CreateUnbounded<int>();
		
		public ValueTask QueueAsync(int tripId)
		{
			return _queue.Writer.WriteAsync(tripId);

		}
		public ValueTask<int> DequeueAsync(
	 CancellationToken cancellationToken)
		{
			return _queue.Reader.ReadAsync(cancellationToken);
		}

	}
}
