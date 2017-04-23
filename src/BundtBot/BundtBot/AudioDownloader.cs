using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace BundtBot
{
    public class AudioDownloader
    {
        static readonly MyLogger _logger = new MyLogger(nameof(AudioDownloader));

        public Action<object, ProgressEventArgs> ProgressDownload;
        public Action<object, DownloadEventArgs> FinishedDownload;
        public Action<object, DownloadEventArgs> StartedDownload;
        public Action<object, ProgressEventArgs> ErrorDownload;

        public object ProcessObject { get; set; }
        public bool Started { get; set; }
        public bool Finished { get; set; }
        public decimal Percentage { get; set; }
        public Process Process { get; set; }
        public string OutputName { get; set; }
        public string Url { get; set; }

        public string ConsoleLog { get; set; }

        public FileInfo FinishedOutputFilePath { get; private set; }


        public AudioDownloader(string url, string outputName, DirectoryInfo outputfolder)
        {
            Started = false;
            Finished = false;
            Percentage = 0;

            Url = url;

            // make sure filename ends with an mp3 extension
            OutputName = outputName;
            if (!OutputName.ToLower().EndsWith(".mp3"))
            {
                OutputName += ".mp3";
            }

            // if the destination file exists, exit
            //var destinationPath = System.IO.Path.Combine(outputfolder, OutputName);
            //if (System.IO.File.Exists(destinationPath))
            //{
            //    throw new Exception(destinationPath + " exists");
            //}
            var arguments = $@"--max-filesize 100m --extract-audio {url} --audio-format wav -o {outputfolder.FullName}/%(id)s.%(ext)s";  //--ignore-errors

            // setup the process that will fire youtube-dl
            Process = new Process
            {
                StartInfo = {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "./youtube-dl.exe",
                    Arguments = arguments,
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            Process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            Process.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);
        }

        protected virtual void OnProgress(ProgressEventArgs e)
        {
            ProgressDownload?.Invoke(this, e);
        }

        protected virtual void OnDownloadFinished(DownloadEventArgs e)
        {
            if (Finished) return;
            Finished = true;
            FinishedDownload?.Invoke(this, e);
        }

        protected virtual void OnDownloadStarted(DownloadEventArgs e)
        {
            StartedDownload?.Invoke(this, e);
        }

        protected virtual void OnDownloadError(ProgressEventArgs e)
        {
            ErrorDownload?.Invoke(this, e);
        }

        /// <exception cref="YoutubeException">
        /// Thrown if we were unable to capture an output file from the stdout of youtube-dl.
        /// This could happen for many reasons.
        /// (Examples: video removed from youtube, taken down because of copyright, too big)
        /// </exception>
        public FileInfo Download()
        {
            _logger.LogInfo($"Downloading {Url}");
            Process.Exited += Process_Exited;

            _logger.LogInfo("\n" + Process.StartInfo.FileName + " " + Process.StartInfo.Arguments + "\n");

            Process.Start();
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
            Console.Write("Waiting for Process to exit...");
            // Wait for the child app to stop
            Process.WaitForExit();
            _logger.LogInfo("Exited!");

            if (FinishedOutputFilePath != null && FinishedOutputFilePath.Exists == false)
            {
                throw new YoutubeException("There was not output file captured from the stdout of youtube-dl");
            }

            return FinishedOutputFilePath;
        }

        void Process_Exited(object sender, EventArgs e)
        {
            _logger.LogInfo("youtube-dl Exited");
            OnDownloadFinished(new DownloadEventArgs() { ProcessObject = this.ProcessObject });
        }

        public void ErrorDataReceived(object sendingprocess, DataReceivedEventArgs error)
        {
            if (error.Data != null) _logger.LogInfo(error.Data);
            if (!string.IsNullOrEmpty(error.Data))
            {
                OnDownloadError(new ProgressEventArgs() { Error = error.Data, ProcessObject = this.ProcessObject });
            }
        }
        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {

            // extract the percentage from process output
            if (string.IsNullOrEmpty(outLine.Data))
            {
                return;
            }
            _logger.LogInfo(outLine.Data);

            const string ffmpegDestinationString = "[ffmpeg] Destination: ";
            const string ffmpegCorrectingContainerString = "[ffmpeg] Correcting container in \"";

            if (outLine.Data.StartsWith(ffmpegDestinationString))
            {
                FinishedOutputFilePath = new FileInfo(outLine.Data.Substring(ffmpegDestinationString.Length));
            }
            else if (outLine.Data.StartsWith(ffmpegCorrectingContainerString))
            {
                FinishedOutputFilePath = new FileInfo(outLine.Data.Substring(ffmpegCorrectingContainerString.Length).TrimEnd('"'));
            }

            // extract the percentage from process output
            if (Finished)
            {
                return;
            }

            ConsoleLog += outLine.Data;

            if (outLine.Data.Contains("ERROR"))
            {
                OnDownloadError(new ProgressEventArgs() { Error = outLine.Data, ProcessObject = this.ProcessObject });
                return;
            }

            if (!outLine.Data.Contains("[download]"))
            {
                return;
            }
            var pattern = new Regex(@"\b\d+([\.,]\d+)?", RegexOptions.None);
            if (!pattern.IsMatch(outLine.Data))
            {
                return;
            }

            // fire the process event
            var perc = Convert.ToDecimal(Regex.Match(outLine.Data, @"\b\d+([\.,]\d+)?").Value);
            if (perc > 100 || perc < 0)
            {
                _logger.LogInfo($"weird perc {perc}");
                return;
            }
            Percentage = perc;
            OnProgress(new ProgressEventArgs() { ProcessObject = this.ProcessObject, Percentage = perc });

            // is it finished?
            if (perc < 100)
            {
                return;
            }

            if (perc == 100 && !Finished)
            {
                OnDownloadFinished(new DownloadEventArgs() { ProcessObject = this.ProcessObject });
            }
        }
    }

}
