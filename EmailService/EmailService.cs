
using MailKit.Net.Smtp;
using MimeKit;
namespace Taxiiii.EmailService
{
	public interface IEmailService
	{
		Task SendAsync(string toEmail, string subject, string body);
	}
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;
		public EmailService(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public async Task SendAsync(string ToEmail , string subject, string body)
		{
			var email = new MimeMessage(); 
			email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:FromEmail"]));
			email.To.Add(MailboxAddress.Parse(ToEmail));
			email.Subject = subject;
			email.Body = new TextPart("plain")
			{
				Text = body
			}; 
			using var smtp = new SmtpClient();

			await smtp.ConnectAsync(_configuration["EmailSettings:SmtpServer"],
				int.Parse(_configuration["EmailSettings:Port"]),
				MailKit.Security.SecureSocketOptions.StartTls

				  );
			await smtp.AuthenticateAsync(
		_configuration["EmailSettings:FromEmail"],
		  _configuration["EmailSettings:Password"]
	  
		  
		  );
			await smtp.SendAsync(email);

			await smtp.DisconnectAsync(true);
		}

	}
}
