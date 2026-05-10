using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Core.Networking;
using ChatAsyncServerSqlLite.Core;
using ChatAsyncServerSqlLite.Data;
using ChatAsyncServerSqlLite.Data.Repositories;
using ChatAsyncServerSqlLite.Handlers;
using ChatAsyncServerSqlLite.Routing;
using ChatAsyncServerSqlLite.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;

using Serilog;


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

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseSqlite(
            "Data Source=chat.db"
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

IServiceScopeFactory scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

SessionManager sessionManager = host.Services.GetRequiredService<SessionManager>();
ILogger<Server> logger = host.Services.GetRequiredService<ILogger<Server>>();

IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 7000);

Server server = new Server(
    iPEndPoint,
    sessionManager,
    scopeFactory,
    logger
);

_ = server.StartAsync();

Console.ReadLine();

await server.StopAsync();