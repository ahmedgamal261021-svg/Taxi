using Taxiiii.Models;

namespace Taxiiii.DtoS
{
	public class NotificationDto
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public bool IsRead { get; set; }
		public string Type { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
