using System.IO;

namespace BundtBot.Youtube
{
    public class YoutubeFile
    {
        public readonly FileInfo DownloadedFile;
        public readonly YoutubeInfo Info;

        public YoutubeFile(FileInfo downloadedAudioFile, YoutubeInfo infoJsonObject)
        {
            DownloadedFile = downloadedAudioFile;
            Info = infoJsonObject;
        }
    }
}