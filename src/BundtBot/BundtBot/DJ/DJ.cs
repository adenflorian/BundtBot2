using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtCord.Discord;

namespace BundtBot
{
    class DJ
    {
        static readonly MyLogger _logger = new MyLogger(nameof(DJ));

        ConcurrentQueue<AudioRequest> _audioQueue = new ConcurrentQueue<AudioRequest>();
        AudioRequest _currentlyPlayingRequest;
        bool _cancelCurrentSong;

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
                        var task =  audioRequest.VoiceChannel.SendAudioAsync(audioRequest.Audio);

                        while (true)
                        {
                            await Task.Delay(100);
                            if (_cancelCurrentSong) { _cancelCurrentSong = false; break; }
                            if (task.IsCompleted) break;
                        }

                        _currentlyPlayingRequest = null;
                        // TODO Join next channel if something else in queue instead of disconnecting everytime
                        await audioRequest.VoiceChannel.Server.LeaveVoice();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex);
                    }
                }
            });
        }

        public void EnqueueAudio(byte[] pcmAudio, VoiceChannel voiceChannel)
        {
            _audioQueue.Enqueue(new AudioRequest { VoiceChannel = voiceChannel, Audio = pcmAudio });
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
    }
}