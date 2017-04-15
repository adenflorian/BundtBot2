using System;
using System.IO;
using BundtBot;

namespace DiscordApiWrapper.Audio
{
    public class WavFileReader
    {
        static readonly MyLogger _logger = new MyLogger(nameof(WavFileReader));

        /// <summary>
        /// Will thrown an Exception if the data bytes are not found in the first 1000 bytes,
        /// meaning it probably isn't a valid wav file.
        /// </summary>
        public short[] ReadFile(FileInfo fileInfo)
        {
            var fileBytes = File.ReadAllBytes(fileInfo.FullName);

            int indexOfSamplesStart = FindSamplesStartingIndex(fileBytes);
            _logger.LogDebug($"Found starting index of sample data: {indexOfSamplesStart}");

            var sampleBytes = new byte[fileBytes.Length - indexOfSamplesStart];

            Buffer.BlockCopy(fileBytes, indexOfSamplesStart, sampleBytes, 0, sampleBytes.Length);

            // TODO: return byte[] instead

            // TODO: Support mono

            return GetShortArray(sampleBytes);
        }

        short[] GetShortArray(byte[] sampleBytes)
        {
            var shortArray = new short[sampleBytes.Length / 2];

            for (int i = 0; i < shortArray.Length; i++)
            {
                shortArray[i] = BitConverter.ToInt16(new byte[] { sampleBytes[(i * 2)], sampleBytes[(i * 2) + 1] }, 0);
            }

            return shortArray;
        }

        int FindSamplesStartingIndex(byte[] fileBytes)
        {
            using (var fileBytesStream = new MemoryStream(fileBytes))
            using (var fileBytesReader = new BinaryReader(fileBytesStream))
            {
                int counter = 0;

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    var result = fileBytesReader.ReadByte();
                    _logger.LogTrace(i + " : " + BitConverter.ToString(new byte[] { result }));

                    if (counter == 0 && result == 'd')
                    {
                        counter++;
                        continue;
                    }

                    if (counter == 1)
                    {
                        if (result == 'a')
                        {
                            counter++;
                            continue;
                        }
                        else
                        {
                            counter = 0;
                        }
                    }

                    if (counter == 2)
                    {
                        if (result == 't')
                        {
                            counter++;
                            continue;
                        }
                        else
                        {
                            counter = 0;
                        }
                    }

                    if (counter == 3)
                    {
                        if (result == 'a')
                        {
                            return i + 5;
                        }
                        else
                        {
                            counter = 0;
                        }
                    }

                    if (i > 1000)
                    {
                        throw new Exception();
                    }
                }
            }

            throw new Exception();
        }
    }
}