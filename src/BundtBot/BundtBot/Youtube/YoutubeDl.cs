using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtCommon;
using Newtonsoft.Json;

namespace BundtBot.Youtube
{
    class YoutubeDl
    {
        public readonly DirectoryInfo YoutubeAudioFolder;

        static readonly MyLogger _logger = new MyLogger(nameof(YoutubeDl));

        public YoutubeDl(DirectoryInfo youtubeAudioFolder)
        {
            YoutubeAudioFolder = youtubeAudioFolder;
        }
        
        public async Task<YoutubeFile> DownloadAsync(YoutubeDlArgs args)
        {
            var guid = Guid.NewGuid();
            args.OutputTemplate = $@"{YoutubeAudioFolder}/{guid}.%(ext)s";
            args.WriteInfoJson = true;

            using (var youtubeDlProcess = new Process())
            {
                SetupYoutubeDlProcess(args, youtubeDlProcess);
                _logger.LogDebug($"Starting process: {youtubeDlProcess.StartInfo.FileName} {youtubeDlProcess.StartInfo.Arguments}");
                youtubeDlProcess.Start();
                await Wait.Until(() => youtubeDlProcess.HasExited).StartAsync();
            }

            var downloadedFile = GetDownloadedFile(args, guid);
            var infoJsonObject = LoadInfoJson(downloadedFile);

            // TODO Rename audio file to use video id

            return new YoutubeFile(downloadedFile, infoJsonObject);
        }

        static void SetupYoutubeDlProcess(YoutubeDlArgs args, Process youtubeDlProcess)
        {
            youtubeDlProcess.StartInfo.FileName = "./youtube-dl.exe";
            youtubeDlProcess.StartInfo.Arguments = args.ToString();
            youtubeDlProcess.StartInfo.CreateNoWindow = true;
        }

        FileInfo GetDownloadedFile(YoutubeDlArgs args, Guid guid)
        {
            var downloadedAudioFile = new FileInfo(YoutubeAudioFolder.FullName + '/' + guid + '.' + args.AudioFormat);
            if (downloadedAudioFile.Exists == false)
            {
                throw new YoutubeException("Sorry :( I couldn't find the downloadedAudioFile...");
            }

            return downloadedAudioFile;
        }

        static YoutubeInfo LoadInfoJson(FileInfo downloadedFile)
        {
            var infoJsonFile = new FileInfo(Path.ChangeExtension(downloadedFile.FullName, ".info.json"));
            if (infoJsonFile.Exists == false)
            {
                throw new YoutubeException("Sorry :( I couldn't find the infoJsonFile...");
            }
            var infoJsonObject = JsonConvert.DeserializeObject<YoutubeInfo>(File.ReadAllText(infoJsonFile.FullName));
            return infoJsonObject;
        }
    }
}