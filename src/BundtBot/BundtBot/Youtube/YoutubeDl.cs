using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtCommon;
using Newtonsoft.Json;

namespace BundtBot.Youtube
{
    /// <summary>
    /// Wrapper for youtube-dl.exe
    /// https://rg3.github.io/youtube-dl/
    /// </summary>
    class YoutubeDl
    {
        public readonly DirectoryInfo OutputFolder;
        public readonly DirectoryInfo TempFolder;

        static readonly MyLogger _logger = new MyLogger(nameof(YoutubeDl));

        public YoutubeDl(DirectoryInfo outputFolder, DirectoryInfo tempFolder)
        {
            OutputFolder = outputFolder;
            TempFolder = tempFolder;
        }

        /// <summary>
        /// Will override the OutputTemplate and WriteInfoJson args
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<YoutubeFile> DownloadAsync(YoutubeDlArgs args)
        {
            var guid = Guid.NewGuid();

            // TODO Shouldn't be chaning the args in this method
            args.OutputTemplate = $@"{TempFolder}/{guid}.%(ext)s";
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

            var finalFile = new FileInfo(OutputFolder.FullName + '/' + infoJsonObject.Id + downloadedFile.Extension);

            if (finalFile.Exists)
            {
                downloadedFile.Delete();
            }
            else
            {
                downloadedFile.MoveTo(finalFile.FullName);
            }

            return new YoutubeFile(finalFile, infoJsonObject);
        }

        static void SetupYoutubeDlProcess(YoutubeDlArgs args, Process youtubeDlProcess)
        {
            youtubeDlProcess.StartInfo.FileName = "./youtube-dl.exe";
            youtubeDlProcess.StartInfo.Arguments = args.ToString();
            youtubeDlProcess.StartInfo.CreateNoWindow = true;
        }

        FileInfo GetDownloadedFile(YoutubeDlArgs args, Guid guid)
        {
            var downloadedAudioFile = new FileInfo(TempFolder.FullName + '/' + guid + '.' + args.AudioFormat);
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
            infoJsonFile.Delete();
            return infoJsonObject;
        }
    }
}