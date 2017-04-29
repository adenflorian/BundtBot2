using System;
using System.IO;
using System.Threading.Tasks;
using BundtBot.Extensions;
using BundtCord.Discord;

namespace BundtBot
{
    class YoutubeDlOld
    {
        static readonly MyLogger _logger = new MyLogger(nameof(YoutubeDlOld));

        decimal _lastPercentage;

        public async Task<FileInfo> DownloadAndConvertAsync(TextChannelMessage message, string youtubeDlUrl, DirectoryInfo outputFolder)
        {
            var urlToDownload = youtubeDlUrl;
            var newFilename = Guid.NewGuid().ToString();

            var downloader = new YoutubeDlProcess(urlToDownload, newFilename, outputFolder);
            downloader.ProgressDownload += (sender, ev) =>
            {
                _logger.LogInfo(ev.Percentage.ToString("0.0"), ConsoleColor.Green);
                if (ev.Percentage > _lastPercentage + 25)
                {
                    //await _progressMessage.Edit("downloading: " + ev.Percentage.ToString("0") + "%");
                    _lastPercentage = ev.Percentage;
                }
            };
            downloader.FinishedDownload += (sender, ev) =>
            {
                Console.WriteLine("Finished Download!");
                //await _progressMessage.Edit("downloading: :100: ");
            };
            downloader.ErrorDownload += downloader_ErrorDownload;
            downloader.StartedDownload += downloader_StartedDownload;

            //_progressMessage = await message.ReplyAsync("downloading");
            FileInfo outputPath;
            try
            {
                outputPath = downloader.Download();
            }
            catch (Exception)
            {
                _logger.LogInfo("downloader.Download(); threw an exception :( possibly to big filesize", ConsoleColor.Yellow);
                await message.ReplyAsync("ummm...bad news...something broke...the video was probably too big to download, so try somethin else, k?");
                throw;
            }
            Console.WriteLine("downloader.Download() Finished! " + outputPath);

            if (outputPath == null)
            {
                throw new YoutubeException("that thing you asked for, i don't think i can get it for you, but i might know someone who can... :frog:");
            }

            if (outputPath.Exists == false)
            {
                throw new YoutubeException("that thing you asked for, i don't think i can get it for you, but i might know someone who can... :frog:");
            }

            return outputPath;
        }

        static void downloader_ErrorDownload(object sender, ProgressEventArgs e)
        {
            Console.WriteLine("error");
        }

        static void downloader_StartedDownload(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("yotube-dl process started");
        }
    }
}
