using System;

namespace BundtBot.Youtube
{
    public enum YoutubeDlAudioFormat
    {
        wav
    }
    
    public class YoutubeDlArgs
    {
        public readonly string UrlOrSearchString;
        string _urlArg => UrlOrSearchString + ' ';

        /// <summary>
        /// If true, resulting file will be an audio file. (-x, --extract-audio)
        /// </summary>
        public bool ExtractAudio = true;
        string _extractAudioArg => ExtractAudio ? "--extract-audio " : "";

        /// <summary>
        /// Only used when ExtractAudio is true. (--audio-format)
        /// </summary>
        public YoutubeDlAudioFormat AudioFormat = YoutubeDlAudioFormat.wav;
        string _audioFormatArg => "--audio-format " + AudioFormat.ToString() + ' ';

        /// <summary>
        /// See youtube-dl docs for info on how to use this. (-o, --output)
        /// </summary>
        public string OutputTemplate;
        string _outputArg => OutputTemplate != null ? "--output " + OutputTemplate + ' ' : "";

        /// <summary>
        /// Do not download any videos larger than this (in Megabytes)
        /// </summary>
        public uint MaxFileSizeMB;
        string _maxFileSizeArg => $"--max-filesize {MaxFileSizeMB}m";

        /// <summary>
        /// See this link for supported sites: https://rg3.github.io/youtube-dl/supportedsites.html
        /// </summary>
        public static YoutubeDlArgs FromUrl(Uri mediaUrl)
        {
            if (mediaUrl.IsAbsoluteUri == false) throw new ArgumentException("Must be an absolute uri", nameof(mediaUrl));

            return new YoutubeDlArgs(mediaUrl.ToString());
        }

        /// <summary>
        /// Will search youtube and download first video found.
        /// </summary>
        public static YoutubeDlArgs FromSearchString(string searchString)
        {
            return new YoutubeDlArgs($"\"ytsearch1:{searchString}\"");
        }

        YoutubeDlArgs(string youtubeDlUrl)
        {
            UrlOrSearchString = youtubeDlUrl;
        }

        public override string ToString()
        {
            var args = "";

            args += _urlArg;
            args += _extractAudioArg;
            args += _audioFormatArg;
            args += _outputArg;
            args += _maxFileSizeArg;

            return args;
        }
    }
}