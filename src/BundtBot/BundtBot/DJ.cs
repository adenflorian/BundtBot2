using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtCord.Discord;

namespace BundtBot
{
    public class DJ
    {
        class AudioRequest
        {
            public VoiceChannel VoiceChannel;
            public byte[] Audio;
        }

        ConcurrentQueue<AudioRequest> _audioQueue = new ConcurrentQueue<AudioRequest>();
        AudioRequest _currentlyPlayingRequest;

        public void EnqueueAudio(byte[] pcmAudio, VoiceChannel voiceChannel)
        {
            _audioQueue.Enqueue(new AudioRequest { VoiceChannel = voiceChannel, Audio = pcmAudio });
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    while (_audioQueue.Count == 0) await Task.Delay(100);

                    AudioRequest audioRequest;
                    if (_audioQueue.TryDequeue(out audioRequest) == false) continue;
                    _currentlyPlayingRequest = audioRequest;

                    await audioRequest.VoiceChannel.JoinAsync();
                    await audioRequest.VoiceChannel.SendAudioAsync(audioRequest.Audio);
                    // TODO Join next channel if something else in queue instead of disconnecting everytime
                    await audioRequest.VoiceChannel.Server.LeaveVoice();
                }
            });
        }

        public async Task PauseAudioAsync()
        {
            if (_currentlyPlayingRequest == null) throw new InvalidOperationException("Nothing is playing, nothing to pause");

            await _currentlyPlayingRequest.VoiceChannel.Server.VoiceClient.PauseAsync();
        }

        public void ResumeAudio()
        {
            if (_currentlyPlayingRequest == null) throw new InvalidOperationException("Nothing is playing, nothing to resume");

            _currentlyPlayingRequest.VoiceChannel.Server.VoiceClient.Resume();
        }
    }
}