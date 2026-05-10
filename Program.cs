using ChatAsyncServerSqlLite.Abstractions.Interfaces;
using ChatAsyncServerSqlLite.Core.Networking;
using ChatAsyncServerSqlLite.Core;
using ChatAsyncServerSqlLite.Data;
using ChatAsyncServerSqlLite.Data.Repositories;
using ChatAsyncServerSqlLite.Handlers;
using ChatAsyncServerSqlLite.Routing;
using ChatAsyncServerSqlLite.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseSqlite(
            "Data Source=chat.db"
        );
    });

builder.Services.AddLogging();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddScoped<IClientRepository, ClientRepository>();

builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddScoped<AuthHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<PacketRouter>();

builder.Services.AddScoped<MessageBroadcaster>();

IHost host = builder.Build();

IServiceScopeFactory scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();

SessionManager sessionManager = host.Services.GetRequiredService<SessionManager>();

IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 7000);

Server server = new Server(
    iPEndPoint,
    sessionManager,
    scopeFactory,
    
);

_ = server.StartAsync();

Console.ReadLine();

await server.StopAsync();