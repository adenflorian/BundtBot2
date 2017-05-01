using System;
using System.IO;
using System.Threading.Tasks;
using BundtBot.Extensions;
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

        public async Task<YoutubeInfo> DownloadInfoAsync(YoutubeDlUrl youtubeDlUrl)
        {
            var guid = Guid.NewGuid();

            var youtubeDlArgs = new YoutubeDlArgs(youtubeDlUrl)
            {
                OutputTemplate = $@"{TempFolder}/{guid}.%(ext)s",
                SkipDownload = true,
                WriteInfoJson = true
            };

            await YoutubeDlProcess.Run(youtubeDlArgs);

            return LoadInfoUsingGuid(guid);
        }

        YoutubeInfo LoadInfoUsingGuid(Guid guid)
        {
            return LoadInfoFromInfoJsonFile(new FileInfo(TempFolder.FullName + '/' + guid + ".info.json"));
        }

        static YoutubeInfo LoadInfoFromInfoJsonFile(FileInfo infoJsonFile)
        {
            if (infoJsonFile.Exists == false)
            {
                throw new YoutubeException("Sorry :( I couldn't find the infoJsonFile...");
            }
            var infoJsonObject = File.ReadAllText(infoJsonFile.FullName).Deserialize<YoutubeInfo>();
            infoJsonFile.Delete();
            return infoJsonObject;
        }

        public async Task<YoutubeFile> DownloadAudioAsync(YoutubeDlUrl youtubeDlUrl, YoutubeDlAudioFormat audioFormat, uint maxFileSize)
        {
            var guid = Guid.NewGuid();

            var youtubeDlArgs = new YoutubeDlArgs(youtubeDlUrl)
            {
                AudioFormat = audioFormat,
                MaxFileSizeMB = maxFileSize,
                ExtractAudio = true,
                OutputTemplate = $@"{TempFolder}/{guid}.%(ext)s",
                WriteInfoJson = true
            };

            await YoutubeDlProcess.Run(youtubeDlArgs);

            var downloadedFile = GetDownloadedFile(youtubeDlArgs, guid);
            var infoJsonObject = LoadInfoFromDownloadedFile(downloadedFile);

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

        FileInfo GetDownloadedFile(YoutubeDlArgs args, Guid guid)
        {
            var downloadedAudioFile = new FileInfo(TempFolder.FullName + '/' + guid + '.' + args.AudioFormat);
            if (downloadedAudioFile.Exists == false)
            {
                throw new YoutubeException("Sorry :( I couldn't find the downloadedAudioFile...");
            }

            return downloadedAudioFile;
        }

        static YoutubeInfo LoadInfoFromDownloadedFile(FileInfo downloadedFile)
        {
            return LoadInfoFromInfoJsonFile(new FileInfo(Path.ChangeExtension(downloadedFile.FullName, ".info.json")));
        }
    }
}