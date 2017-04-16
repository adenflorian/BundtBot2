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
        public byte[] ReadFileBytes(FileInfo fileInfo)
        {
            var fileBytes = File.ReadAllBytes(fileInfo.FullName);

            int indexOfSamplesStart = FindSamplesStartingIndex(fileBytes);
            _logger.LogDebug($"Found starting index of sample data: {indexOfSamplesStart}");

            var sampleBytes = new byte[fileBytes.Length - indexOfSamplesStart];

            Buffer.BlockCopy(fileBytes, indexOfSamplesStart, sampleBytes, 0, sampleBytes.Length);

            return sampleBytes;
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
                        throw new Exception("Could not find 'data' in first 1000 bytes, so it's probably not a wave file");
                    }
                }
            }

            throw new Exception("File too short...?");
        }
    }
}