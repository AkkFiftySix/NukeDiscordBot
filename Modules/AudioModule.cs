using Discord;
using Discord.Commands;
using Nuke.Logic;

namespace Nuke.Modules;

public class AudioModule(AudioService service, YoutubeAudioService youtubeAudioservice) : ModuleBase<ICommandContext>
{
    private readonly AudioService _audioService = service;
    private readonly YoutubeAudioService _ytAudioservice = youtubeAudioservice;

    [Command("p", RunMode = RunMode.Async)]
    public async Task Play(string msg)
    {
        var ytAudioResult = await _ytAudioservice.GetAudio(msg);
        await _audioService.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        var needLeave = await _audioService.SendAudioAsync(Context.Guild, Context.Channel, ytAudioResult);
        if (needLeave) await _audioService.LeaveAudio(Context.Guild);
    }

    [Command("s", RunMode = RunMode.Async)]
    public async Task Stop()
    {
        await _audioService.LeaveAudio(Context.Guild);
    }
}