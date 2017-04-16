using System;

namespace DiscordApiWrapper.Audio
{
    class AudioGeneration
    {
        const int _msPerSecond = 1000;
        
        public short[] GenerateSquareWavePcm(int channels, int samplingRate, int lengthInMs)
        {
            var pcm = new short[((samplingRate * channels) / _msPerSecond) * lengthInMs];

            for (int time = 0; time < pcm.Length / channels; time++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    if (time % 200 > 100)
                    {
                        pcm[(time * channels) + channel] = 4000;
                    }
                    else
                    {
                        pcm[(time * channels) + channel] = -4000;
                    }
                }
            }

            return pcm;
        }

        public short[] GenerateSinWavePcm(int channels, int samplingRate, int lengthInMs)
        {
            var pcm = new short[((samplingRate * channels) / _msPerSecond) * lengthInMs];

            for (int time = 0; time < pcm.Length / channels; time++)
            {
                for (int channel = 0; channel < channels; channel++)
                {
                    pcm[(time * channels) + channel] = (short)(Math.Sin(time / 50) * 4000);
                }
            }

            return pcm;
        }
    }
}