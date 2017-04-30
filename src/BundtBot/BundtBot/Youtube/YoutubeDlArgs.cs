using System;

namespace BundtBot.Youtube
{
    class YoutubeDlArgs
    {
        public readonly string UrlOrYtSearchString;
        string _urlArg => UrlOrYtSearchString + ' ';

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
        /// Do not download any videos larger than this (in Megabytes)
        /// </summary>
        public uint MaxFileSizeMB;
        string _maxFileSizeArg => $"--max-filesize {MaxFileSizeMB}m ";

        /// <summary>
        /// See youtube-dl docs for info on how to use this. (-o, --output)
        /// </summary>
        public string OutputTemplate;
        string _outputArg => OutputTemplate != null ? "--output " + OutputTemplate + ' ' : "";

        /// <summary>
        /// Write video metadata to a [filename].info.json file. (--write-info-json)
        /// Example: Downloaded filename is abc.wav, info file would be abc.info.json
        /// </summary>
        public bool WriteInfoJson;
        string _writeInfoJson => WriteInfoJson ? "--write-info-json " : "";

        /// <summary>
        /// Do not download the video. (--skip-download)
        /// </summary>
        public bool SkipDownload;

        public YoutubeDlArgs(YoutubeDlUrl youtubeDlUrl)
        {
            UrlOrYtSearchString = youtubeDlUrl.UrlOrSearchString;
        }

        string _skipDownload => SkipDownload ? "--skip-download " : "";

        public override string ToString()
        {
            var args = "";

            args += _urlArg;
            args += _extractAudioArg;
            args += _audioFormatArg;
            args += _outputArg;
            args += _maxFileSizeArg;
            args += _writeInfoJson;
            //args += _dumpJson;
            args += _skipDownload;

            return args;
        }
    }
}