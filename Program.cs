using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using StackExchange.Redis;
using System.Text;
using Taxiiii.Data;
using Taxiiii.EmailService;
using Taxiiii.Interfaces;

using Taxiiii.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
	var configuration = builder.Configuration["Redis:ConnectionString"];
	return ConnectionMultiplexer.Connect(configuration);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<TripService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,

		ValidIssuer = builder.Configuration["JWT:Issuer"],
		ValidAudience = builder.Configuration["JWT:Audience"],

		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
	};
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			var accessToken = context.Request.Query["access_token"];

			var path = context.HttpContext.Request.Path;

			if (!string.IsNullOrEmpty(accessToken)
				&& path.StartsWithSegments("/NotificationHub"))
			{
				context.Token = accessToken;
			}

			return Task.CompletedTask;
		}
	};
});
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("MyConnection")));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Enter: Bearer {your token}"
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});
//builder.WebHost.UseUrls("http://0.0.0.0:5139");
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPendingTripQueue, PendingTripQueue>();
builder.Services.AddHostedService<PendingTripService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.MapHub<RideHub>("/rideHub");
app.MapHub<NotificationHub>("/NotificationHub");

app.Run();
