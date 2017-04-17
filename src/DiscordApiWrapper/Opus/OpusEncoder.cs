using System;
using BundtBot;

namespace DiscordApiWrapper.Opus
{
    class OpusEncoder : IDisposable
    {
        static readonly MyLogger _logger = new MyLogger(nameof(OpusEncoder));
        
        readonly int _inputChannels;
        readonly int _maxDataBytes;
        
        IntPtr _pointerToEncoder;
        bool _disposed;

        public static OpusEncoder Create(int inputSamplingRate, int inputChannels, Application application)
        {
            if (inputSamplingRate != 48000) throw new ArgumentOutOfRangeException("inputSamplingRate - only supports 48000");
            if (inputChannels != 2) throw new ArgumentOutOfRangeException("inputChannels - only supports 2");

            IntPtr error;
            IntPtr encoder = OpusWrapper.opus_encoder_create(inputSamplingRate, inputChannels, (int)application, out error);

            if ((Errors)error != Errors.OK) throw new Exception("Exception occured while creating encoder");

            return new OpusEncoder(encoder, inputSamplingRate, inputChannels, application);
        }

        OpusEncoder(IntPtr encoder, int inputSamplingRate, int inputChannels, Application application)
        {
            _pointerToEncoder = encoder;
            _inputChannels = inputChannels;
            _maxDataBytes = 4000;
        }

        public unsafe byte[] Encode(byte[] inputPcmSamples, int samplesLength, out int encodedLength)
        {
            if (_disposed) throw new ObjectDisposedException("OpusEncoder");

            int frames = FrameCount(inputPcmSamples);
            IntPtr encodedPtr;
            byte[] encoded = new byte[_maxDataBytes];
            int length = 0;

            fixed (byte* benc = encoded)
            {
                encodedPtr = new IntPtr((void*)benc);
                length = OpusWrapper.opus_encode(_pointerToEncoder, inputPcmSamples, frames, encodedPtr, samplesLength);
            }

            encodedLength = length;

            if (length < 0) throw new Exception("Encoding failed - " + ((Errors)length).ToString());

            return encoded;
        }

        int FrameCount(byte[] pcmSamples)
        {
            int bitDepth = 16;
            int bytesPerSample = (bitDepth / 8) * _inputChannels;
            return pcmSamples.Length / bytesPerSample;
        }

        ~OpusEncoder()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;

            GC.SuppressFinalize(this);

            if (_pointerToEncoder != IntPtr.Zero)
            {
                OpusWrapper.opus_encoder_destroy(_pointerToEncoder);
                _pointerToEncoder = IntPtr.Zero;
            }

            _disposed = true;
        }
    }
}
