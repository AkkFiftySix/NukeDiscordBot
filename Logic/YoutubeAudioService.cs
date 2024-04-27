using Nuke.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Nuke.Logic;

public class YoutubeAudioService(AppSettings config)
{
    public readonly string PathToFolder = !string.IsNullOrEmpty(config.AudioPath)
        ? config.AudioPath
        : $@"{Environment.CurrentDirectory}\ConvertedMp3\";

    public async Task<YTAudioResult> GetAudio(string url)
    {
        Console.WriteLine("YoutubeAudioService started.");
        var videoId = GetVideoId(url);
        var id = GetHash(videoId);
        var (hasTimeFrom, timeFromSeconds) = GetTimeFrom(url);
        var result = new YTAudioResult(id, hasTimeFrom, timeFromSeconds);

        if (Directory.Exists(PathToFolder) && Directory.GetFiles(PathToFolder).Any(x => x.Contains(id)))
        {
            Console.WriteLine("Already downloaded!");
            return result;
        }

        var p = await Task.Run(() => DownloadWebmAudio(url, id));
        while (!p.HasExited)
        {
            Console.WriteLine($"Downloading...");
            await Task.Delay(1000);
        }

        if (Directory.Exists(PathToFolder) && Directory.GetFiles(PathToFolder).Any(x => x.Contains(id)))
        {
            Console.WriteLine("Downloaded!");
            return result;
        }
        else
            throw new Exception("Download failed!");
    }

    private static string GetVideoId(string urlStr)
    {
        var uri = new Uri(urlStr);
        var videoId = HttpUtility.ParseQueryString(uri.Query).Get("v");
        videoId ??= uri.AbsolutePath.Substring(1);
        return videoId;
    }

    private static (bool, int) GetTimeFrom(string urlStr)
    {
        var uri = new Uri(urlStr);
        var timeFrom = HttpUtility.ParseQueryString(uri.Query).Get("t");
        return timeFrom is not null ? (true, int.Parse(timeFrom)) : (false, 0);
    }

    private static string GetHash(string str)
    {
        var inputBytes = Encoding.ASCII.GetBytes(str);
        var hashBytes = MD5.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    private Process DownloadWebmAudio(string url, string audioId)
    {
        return InitProcess("yt-dlp.exe",
            $" -f bestaudio  --extract-audio --audio-format mp3 --audio-quality 0 {url} -o {PathToFolder}\\{audioId}.%(ext)s");
    }

    private static Process InitProcess(string fileName, string args)
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = args;

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }
}