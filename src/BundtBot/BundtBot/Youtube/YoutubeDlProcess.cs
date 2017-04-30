using System.Diagnostics;
using System.Threading.Tasks;
using BundtCommon;

namespace BundtBot.Youtube
{
    /// <summary>
    /// Wrapper for youtube-dl.exe
    /// https://rg3.github.io/youtube-dl/
    /// </summary>
    static class YoutubeDlProcess
    {
        static readonly MyLogger _logger = new MyLogger(nameof(YoutubeDlProcess));

        public static async Task Run(YoutubeDlArgs youtubeDlArgs)
        {
            using (var youtubeDlProcess = new Process())
            {
                SetupYoutubeDlProcess(youtubeDlArgs, youtubeDlProcess);
                _logger.LogDebug($"Starting process: {youtubeDlProcess.StartInfo.FileName} {youtubeDlProcess.StartInfo.Arguments}");
                youtubeDlProcess.Start();
                await Wait.Until(() => youtubeDlProcess.HasExited).StartAsync();
                await LogStdOutAndStdErr(youtubeDlProcess);
            }
        }

        static void SetupYoutubeDlProcess(YoutubeDlArgs args, Process youtubeDlProcess)
        {
            youtubeDlProcess.StartInfo.FileName = "./youtube-dl.exe";
            youtubeDlProcess.StartInfo.Arguments = args.ToString();
            youtubeDlProcess.StartInfo.CreateNoWindow = true;
            youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
            youtubeDlProcess.StartInfo.RedirectStandardError = true;
        }

        static async Task LogStdOutAndStdErr(Process youtubeDlProcess)
        {
            while (youtubeDlProcess.StandardOutput.EndOfStream == false)
            {
                _logger.LogDebug("youtube-dl stdout: " + await youtubeDlProcess.StandardOutput.ReadLineAsync());
            }
            while (youtubeDlProcess.StandardError.EndOfStream == false)
            {
                _logger.LogError("youtube-dl stderr: " + await youtubeDlProcess.StandardError.ReadLineAsync());
            }
        }
    }
}