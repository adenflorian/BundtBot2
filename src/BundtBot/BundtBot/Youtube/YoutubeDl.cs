using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtCommon;

namespace BundtBot.Youtube
{
    class YoutubeDl
    {
        public readonly DirectoryInfo YoutubeAudioFolder;

        public YoutubeDl(DirectoryInfo youtubeAudioFolder)
        {
            YoutubeAudioFolder = youtubeAudioFolder;
        }
        
        public async Task<FileInfo> DownloadAsync(YoutubeDlArgs args)
        {
            var guid = Guid.NewGuid();

            args.OutputTemplate = $@"{YoutubeAudioFolder}/{guid}.%(ext)s";

            using (var youtubeDlProcess = new Process())
            {
                youtubeDlProcess.StartInfo.FileName = "./youtube-dl.exe";
                youtubeDlProcess.StartInfo.Arguments = args.ToString();
                youtubeDlProcess.StartInfo.CreateNoWindow = true;

                youtubeDlProcess.Start();

                await Wait.Until(() => youtubeDlProcess.HasExited).StartAsync();
            }

            var downloadedAudioFile = new FileInfo(YoutubeAudioFolder.FullName + '/' + guid + '.' + args.AudioFormat);

            if (downloadedAudioFile.Exists == false)
            {
                throw new YoutubeException("that thing you asked for, i don't think i can get it for you, but i might know someone who can... :frog:");
            }

            return downloadedAudioFile;
        }
    }
}