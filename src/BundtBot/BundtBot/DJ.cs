using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundtCord.Discord;

namespace BundtBot
{
    public class DJ
    {
        struct AudioRequest
        {
            public VoiceChannel VoiceChannel;
            public byte[] Audio;
        }

        ConcurrentQueue<AudioRequest> _audioQueue = new ConcurrentQueue<AudioRequest>();

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

                    await audioRequest.VoiceChannel.JoinAsync();
                    await audioRequest.VoiceChannel.SendAudioAsync(audioRequest.Audio);
                    // TODO Join next channel if something else in queue instead of disconnecting everytime
                    await audioRequest.VoiceChannel.Server.LeaveVoice();
                }
            });
        }
    }
}