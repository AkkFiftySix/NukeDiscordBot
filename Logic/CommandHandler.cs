using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Nuke.Models;
using System.Reflection;

namespace Nuke.Logic;

public class CommandHandler(
    DiscordSocketClient client,
    ILogger<CommandHandler> logger,
    CommandService service,
    IServiceProvider provider,
    AppSettings config) : DiscordClientService(client, logger)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += OnMessageRecevied;
        service.CommandExecuted += OnCommandExecuted;
        await service.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
    }

    private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        await commandContext.Channel.SendMessageAsync(result.ErrorReason);
    }

    private async Task OnMessageRecevied(SocketMessage socketMsg)
    {
        if (socketMsg is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;

        var argPos = 0;
        if (!message.HasStringPrefix(config.CommandsPrefix, ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

        var context = new SocketCommandContext(Client, message);
        await service.ExecuteAsync(context, argPos, provider);
    }
}