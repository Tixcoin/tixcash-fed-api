using dataAPI;
using Google.Api;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using TronNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddPolicy("DataAPI", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var configBuilder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

var Configuration = configBuilder.Build();
var origins = Configuration["origins"].Split(',');

builder.Services.AddCors(options => options.AddPolicy("ApiCorsPolicy", build =>
{
    build.WithOrigins(origins)
         .AllowAnyMethod()
         .AllowAnyHeader();
}));

builder.Services.AddTronNet(x =>
{
    x.Network = TronNetwork.MainNet;
    x.Channel = new GrpcChannelOption { Host = Configuration["ChannelHost"], Port = Int32.Parse(Configuration["ChannelPort"]) };
    x.SolidityChannel = new GrpcChannelOption { Host = Configuration["SolidityChannelHost"], Port = Int32.Parse(Configuration["SolidityChannelPort"]) };
    x.ApiKey = "";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<SwaggerBasicAuthMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("DataAPI");
app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseCors(x => x
//       .WithOrigins(origins)
//       .AllowAnyMethod()
//       .AllowAnyHeader()
//       .SetIsOriginAllowed(origin => true) // allow any origin
//       .AllowCredentials()); // allow credentials

app.MapControllers();

app.Run();
