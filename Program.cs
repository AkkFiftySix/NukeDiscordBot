using Discord;
using Microsoft.Extensions.Hosting;
using Nuke.Extensions;

namespace Nuke;

public sealed class Program
{    static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, sc) => sc.AddNukeServices())
            .Build()
            .RunAsync();

        await Task.Delay(Timeout.Infinite);
    }
}
