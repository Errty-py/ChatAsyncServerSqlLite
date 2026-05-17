using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Core.Networking;
using TcpChatServer.Core.Sessions;
using TcpChatServer.Core;
using TcpChatServer.Data;
using TcpChatServer.Data.Repositories;
using TcpChatServer.Handlers;
using TcpChatServer.Routing;
using TcpChatServer.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;

using Serilog;
using Microsoft.Extensions.Configuration;


HostApplicationBuilder builder = Host.CreateApplicationBuilder();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddDbContext<AppDbContext>(options => 
{
    options.UseSqlite(
        builder.Configuration
               .GetConnectionString("Default"));
});

builder.Services.AddSingleton<Server>(provider =>
    {
        return new Server(
            new IPEndPoint(IPAddress.Any, 7000),
            provider.GetRequiredService<SessionManager>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILogger<Server>>()
        );
    });

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddSingleton<IMessageBroadcaster, TcpMessageBroadcaster>();
builder.Services.AddSingleton<NetworkHelper>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddScoped<AuthHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<PacketRouter>();

IHost host = builder.Build();

var server = host.Services.GetRequiredService<Server>();

await server.StartAsync();

Console.ReadLine();

await server.StopAsync();

Log.CloseAndFlush();