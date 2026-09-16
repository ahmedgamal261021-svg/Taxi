using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class NotificationHub : Hub
{
	public override async Task OnConnectedAsync()
	{
		var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		Console.WriteLine($"Connected UserId = {userId}");

		if (!string.IsNullOrEmpty(userId))
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, userId);
			Console.WriteLine($"Added To Grouppppppppppppppppppppppppppppp = {userId}");
		}

		await base.OnConnectedAsync();
	}

	public async Task SendNotificationToUser(string userId, string message)
	{
		await Clients.Group(userId)
			.SendAsync("ReceiveNotification", message);
	}
}