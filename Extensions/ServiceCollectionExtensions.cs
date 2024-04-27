using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuke.Logic;
using Nuke.Models;

namespace Nuke.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNukeServices(this IServiceCollection serviceCollection)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        serviceCollection
            .AddSingleton(configuration.GetSection("AppSettings").Get<AppSettings>())
            .AddSingleton<AudioService>()
            .AddSingleton<YoutubeAudioService>()
            .AddSingleton<AudioService>()
            .AddHostedService<CommandHandler>();

        serviceCollection.AddDiscordHost((cfg, _) =>
        {
            cfg.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200,
                GatewayIntents = GatewayIntents.All
            };
            cfg.Token = configuration["AppSettings:BotToken"];
        });

        serviceCollection.AddCommandService((cfg, _) =>
        {
            cfg.DefaultRunMode = RunMode.Async;
            cfg.CaseSensitiveCommands = false;
        });

        serviceCollection.AddInteractionService((config, _) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.UseCompiledLambda = true;
        });


        return serviceCollection;
    }
}
