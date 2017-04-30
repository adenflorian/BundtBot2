using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BundtCommon;
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
                        await Wait.Until(() => _audioQueue.Count > 0).StartAsync();

                        var audioRequest = Dequeue();

                        await audioRequest.VoiceChannel.JoinAsync();
                        _currentlyPlayingRequest = audioRequest;

                        using (var pcmAudioFileStream = audioRequest.WavAudioFile.OpenRead())
                        using (_djStream = new DjStream(pcmAudioFileStream))
                        {
                            var task = audioRequest.VoiceChannel.SendAudioAsync(_djStream);

                            await Wait.Until(() => _cancelCurrentSong || task.IsCompleted).StartAsync();

                            _currentlyPlayingRequest = null;
                            _cancelCurrentSong = false;
                            _djStream.Position = _djStream.Length;

                            await Wait.Until(() => task.IsCompleted).StartAsync();
                            
                            if (_audioQueue.Count == 0) await audioRequest.VoiceChannel.Server.LeaveVoice();
                        }

                        _djStream = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                        _logger.LogError("Restarting DJ loop");
                    }
                }
            });
        }

        AudioRequest Dequeue()
        {
            AudioRequest audioRequest;
            Debug.Assert(_audioQueue.TryDequeue(out audioRequest) == true);
            return audioRequest;
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
            ResumeAudio();
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