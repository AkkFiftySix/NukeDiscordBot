using Discord.Audio;
using System.Diagnostics;

namespace Nuke.Models;
public class ChannelContext(IAudioClient audioClient, Process process = null, bool needLeave = true)
{
    public IAudioClient AudioClient { get; private set; } = audioClient;
    public Process Process { get; set; } = process;
    public bool NeedLeave { get; set; } = needLeave;
}
