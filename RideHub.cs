using Microsoft.AspNetCore.SignalR;

public class RideHub : Hub
{
	public async Task SendLocation(
	   double latitude,
	   double longitude)
	{
		await Clients.All.SendAsync(
			"ReceiveLocation",
			latitude,
			longitude);
	}

	public override async Task OnConnectedAsync()
	{
		Console.WriteLine(
			$"Connected: {Context.ConnectionId}");

		await base.OnConnectedAsync();
	}
}