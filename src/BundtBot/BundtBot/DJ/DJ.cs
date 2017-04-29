using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BundtCord.Discord;
using DiscordApiWrapper.Audio;

namespace BundtBot
{
    class DJ
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DJ));

        ConcurrentQueue<AudioRequest> _audioQueue = new ConcurrentQueue<AudioRequest>();
        AudioRequest _currentlyPlayingRequest;
        bool _cancelCurrentSong;
        DjStream _djStream;

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        while (_audioQueue.Count == 0) await Task.Delay(100);

                        AudioRequest audioRequest;
                        if (_audioQueue.TryDequeue(out audioRequest) == false) continue;

                        await audioRequest.VoiceChannel.JoinAsync();
                        _currentlyPlayingRequest = audioRequest;

                        using (var audioStream = audioRequest.WavAudioFile.OpenRead())
                        {
                            _djStream = new DjStream(audioStream);

                            var task = audioRequest.VoiceChannel.SendAudioAsync(_djStream);

                            while (true)
                            {
                                await Task.Delay(100);
                                if (_cancelCurrentSong) { _cancelCurrentSong = false; break; }
                                if (task.IsCompleted) break;
                            }

                            _djStream.Dispose();
                            _djStream = null;
                            _currentlyPlayingRequest = null;
                            
                            // TODO Join next channel if something else in queue instead of disconnecting everytime
                            await audioRequest.VoiceChannel.Server.LeaveVoice();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                    }
                }
            });
        }

        public void EnqueueAudio(FileInfo wavAudioFile, VoiceChannel voiceChannel)
        {
            _audioQueue.Enqueue(new AudioRequest { VoiceChannel = voiceChannel, WavAudioFile = wavAudioFile });
        }

        public async Task PauseAudioAsync()
        {
            if (_currentlyPlayingRequest == null) throw new DJException("Nothing is playing, nothing to pause");

            await _currentlyPlayingRequest.VoiceChannel.Server.VoiceClient.PauseAsync();
        }

        public void ResumeAudio()
        {
            if (_currentlyPlayingRequest == null) throw new DJException("Nothing is playing, nothing to resume");

            _currentlyPlayingRequest.VoiceChannel.Server.VoiceClient.Resume();
        }

        public void StopAudioAsync()
        {
            if (_currentlyPlayingRequest == null) throw new DJException("Nothing is playing, nothing to stop");
            _audioQueue = new ConcurrentQueue<AudioRequest>();
            _cancelCurrentSong = true;
        }

        public void Next()
        {
            if (_currentlyPlayingRequest == null) throw new DJException("Nothing is playing, nothing to next");
            _cancelCurrentSong = true;
        }

        public void FastForward()
        {
            if (_djStream == null) throw new DJException("I don't think anything is playing, I could be wrong tho ¯\\_(ツ)_/¯");
            _djStream.EnableFastforward();
        }

        public void StopFastForward()
        {
            if (_djStream == null) throw new DJException("I don't think anything is playing, I could be wrong tho ¯\\_(ツ)_/¯");
            _djStream.DisableFastforward();
        }
    }
}