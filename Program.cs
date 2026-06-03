using TcpChatServer.Abstractions.Interfaces;
using TcpChatServer.Core.Configurations;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;

using Serilog;
using Microsoft.Extensions.Options;


HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection("Server"));

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt",
                  rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    DatabaseSettings databaseSettings = provider.GetRequiredService<IOptions<DatabaseSettings>>()
                                                .Value;

    options.UseNpgsql(databaseSettings.ConnectionString);
});

builder.Services.AddSingleton<Server>(provider =>
{
    ServerSettings serverSettings = provider.GetRequiredService<IOptions<ServerSettings>>()
                                            .Value;

    IPEndPoint endPoint = new(IPAddress.Parse(serverSettings.Ip),
                              serverSettings.Port);

    return new Server(endPoint,
                      provider.GetRequiredService<SessionManager>(),
                      provider.GetRequiredService<IServiceScopeFactory>(),
                      provider.GetRequiredService<ILogger<Server>>());
});

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddSingleton<ITcpBroadcaster, TcpBroadcaster>();
builder.Services.AddSingleton<NetworkHelper>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddScoped<ClientHandler>();
builder.Services.AddScoped<AuthHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<PacketRouter>();

IHost host = builder.Build();

Server server = host.Services.GetRequiredService<Server>();

_ = Task.Run(async () =>
{
    await server.StartAsync();    
});

Console.ReadLine();

await server.StopAsync();

Log.CloseAndFlush();