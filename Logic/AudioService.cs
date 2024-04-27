using Discord;
using Discord.Audio;
using Nuke.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Nuke.Logic;

public class AudioService(YoutubeAudioService ytAudioService, AppSettings config)
{
    private readonly YoutubeAudioService _ytAudioService = ytAudioService;
    private readonly AppSettings _config = config;

    private readonly ConcurrentDictionary<ulong, ChannelContext> ConnectedChannels = new();
    private readonly object _locker = new();

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out _))
        {
            return;
        }
        if (target.Guild.Id != guild.Id)
        {
            return;
        }

        var audioClient = await target.ConnectAsync();
        ConnectedChannels.TryAdd(guild.Id, new ChannelContext(audioClient));
    }

    public async Task<bool> SendAudioAsync(IGuild guild, IMessageChannel channel, YTAudioResult ytAudioResult)
    {
        var ctx = ConnectedChannels[guild.Id];
        var path = @$"{_ytAudioService.PathToFolder}\{ytAudioResult.AudioId}.mp3";

        if (!File.Exists(path))
        {
            await channel.SendMessageAsync("File does not exist.");
            return true;
        }
        
        if (ctx.Process is not null && !ctx.Process.HasExited)
        {
            ctx.Process.Kill();
            ctx.NeedLeave = false;
        }

        lock (_locker)
        {
            ctx.NeedLeave = true;
            using var ffmpeg = CreateProcess(path, ytAudioResult);
            using var stream = ctx.AudioClient.CreatePCMStream(AudioApplication.Music);
            ctx.Process = ffmpeg;
            ffmpeg.StandardOutput.BaseStream.CopyTo(stream);
            stream.Flush();
            if (!_config.CacheAudio) File.Delete(path);
            return ctx.NeedLeave;
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (ConnectedChannels.TryRemove(guild.Id, out var ctx))
        {
            try { ctx.Process.Kill(); } catch { }
            await Task.Delay(1000);
            await ctx.AudioClient.StopAsync();
        }
    }

    private static Process CreateProcess(string path, YTAudioResult ytAudioResult)
    {
        var args = ytAudioResult.HasTimeFrom
            ? $"-hide_banner -loglevel panic -ss {ytAudioResult.TimeFromSeconds} -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1"
            : $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1";

        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}