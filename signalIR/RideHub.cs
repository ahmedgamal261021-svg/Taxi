	using Microsoft.AspNetCore.SignalR;

	public class RideHub : Hub
	{
					public async Task JoinTrip (int tripId)
				{
					await Groups.AddToGroupAsync(
					   Context.ConnectionId,
					   $"trip-{tripId}"
				   );
				Console.WriteLine(
					 $"Connection {Context.ConnectionId} joined trip-{tripId}");
			}
		

	public async Task JoinUser(int userId)
	{
		await Groups.AddToGroupAsync(
			Context.ConnectionId,
			$"user-{userId}"
		);
	}
	public async Task JoinOnlineDrivers()
	{
		Console.WriteLine("🔥 JoinOnlineDrivers CALLED");

		await Groups.AddToGroupAsync(
			Context.ConnectionId,
			"online-drivers"
		);

		Console.WriteLine(
			$"Connection {Context.ConnectionId} joined online-drivers"
		);
	}
	public override async Task OnConnectedAsync()
	{
		Console.WriteLine(
			$"Connected: {Context.ConnectionId}");

		await base.OnConnectedAsync();
	}
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		Console.WriteLine(
			$"🔴 Disconnected: {Context.ConnectionId}");

		await base.OnDisconnectedAsync(exception);
	}


}