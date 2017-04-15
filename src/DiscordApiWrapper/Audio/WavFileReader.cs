using System;
using System.IO;
using BundtBot;

namespace DiscordApiWrapper.Audio
{
    public class WavFileReader
    {
        static readonly MyLogger _logger = new MyLogger(nameof(WavFileReader));

        public short[] ReadFile(FileInfo fileInfo)
        {
            // Read all bytes in from a file on the disk.
            var file = File.ReadAllBytes(fileInfo.FullName);
            int indexOfSamplesStart = 0;

            // Create a memory stream from those bytes.
            using (var memory = new MemoryStream(file))
            {
                // Use the memory stream in a binary reader.
                using (var reader = new BinaryReader(memory))
                {
                    int flag = 0;
                    // Read in each byte from memory.
                    for (int i = 0; i < file.Length; i++)
                    {

                        byte result = reader.ReadByte();
                        _logger.LogDebug(i + " : " + BitConverter.ToString(new byte[] {result}));
                        if (flag == 0 && result == 0x64)
                        {
                            flag++;
                            continue;
                        }

                        if (flag == 1)
                        {
                            if (result == 0x61)
                            {
                                flag++;
                                continue;
                            }
                            else
                            {
                                flag = 0;
                            }
                        }

                        if (flag == 2)
                        {
                            if (result == 0x74)
                            {
                                flag++;
                                continue;
                            }
                            else
                            {
                                flag = 0;
                            }
                        }

                        if (flag == 3)
                        {
                            if (result == 0x61)
                            {
                                indexOfSamplesStart = i + 5;
                                break;
                            }
                            else
                            {
                                flag = 0;
                            }
                        }

                        if (i > 100)
                        {
                            throw new Exception();
                        }
                    }
                }
            }

            _logger.LogDebug($"Found starting index of sample data: {indexOfSamplesStart}");


            var sampleBytes = new byte[file.Length - indexOfSamplesStart];

            Buffer.BlockCopy(file, indexOfSamplesStart, sampleBytes, 0, sampleBytes.Length);

            var shortArray = new short[sampleBytes.Length / 2];

            // read two bytes at a time into a short array
            for (int i = 0; i < shortArray.Length; i++)
            {
                shortArray[i] = BitConverter.ToInt16(new byte[] { sampleBytes[(i * 2)], sampleBytes[(i * 2) + 1] }, 0);
                // var shorterArray = new short[16];
                // Buffer.BlockCopy(shortArray, 0, shorterArray, 0, 16);

                // _logger.LogDebug($"After short array:");
                // for (int j = 0; j < shorterArray.Length; j++)
                // {
                //     _logger.LogDebug(shorterArray[j]);
                // }

            }

            return shortArray;
        }
    }
}