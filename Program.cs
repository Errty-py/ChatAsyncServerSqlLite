using SpaceChatServer.Abstractions.Interfaces;
using SpaceChatServer.Core.Configurations;
using SpaceChatServer.Core.Networking;
using SpaceChatServer.Core.Sessions;
using SpaceChatServer.Core;
using SpaceChatServer.Data;
using SpaceChatServer.Data.Repositories;
using SpaceChatServer.Handlers;
using SpaceChatServer.Routing;
using SpaceChatServer.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using System.Net;

using Serilog;


HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Services.Configure<ServerSettings>(builder.Configuration.GetSection("Server"));

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/Log-.txt",
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

builder.Services.AddSingleton<IPacketBroadcaster, PacketBroadcaster>();
builder.Services.AddSingleton<NetworkHelper>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ClientService>();

builder.Services.AddScoped<ClientHandler>();
builder.Services.AddScoped<AuthHandler>();
builder.Services.AddScoped<MessageHandler>();
builder.Services.AddScoped<PacketRouter>();

try
{
    IHost host = builder.Build();

    using (IServiceScope scope = host.Services.CreateAsyncScope())
    {
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    Server server = host.Services.GetRequiredService<Server>();

    try
    {
        await server.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }
    finally
    {
        await server.StopAsync();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex,
              "Server terminated unexpectedly");

    throw;
}
finally
{
    Log.CloseAndFlush();
}