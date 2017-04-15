using System;
using Concentus.Enums;
using Concentus.Structs;

namespace ConcentusDemo
{
    public class ConcentusCodec
    {
        readonly int _channels = 2;
        private int _bitrate = 64;
        private int _complexity = 5;
        private double _frameSize = 20;
        private int _packetLoss = 0;
        private bool _vbr = false;
        private bool _cvbr = false;
        private OpusApplication _application = OpusApplication.OPUS_APPLICATION_AUDIO;

        private BasicBufferShort _incomingSamples = new BasicBufferShort(48000);

        private OpusEncoder _encoder;

        private byte[] scratchBuffer = new byte[10000];

        public ConcentusCodec(int channels)
        {
            _channels = channels;
            _encoder = OpusEncoder.Create(48000, _channels, OpusApplication.OPUS_APPLICATION_AUDIO);

            SetBitrate(_bitrate);
            SetComplexity(_complexity);
            SetVBRMode(_vbr, _cvbr);
        }

        public void SetBitrate(int bitrate)
        {
            _bitrate = bitrate;
            _encoder.Bitrate = (_bitrate * 1024);
        }

        public void SetComplexity(int complexity)
        {
            _complexity = complexity;
            _encoder.Complexity = (_complexity);
        }

        public void SetFrameSize(double frameSize)
        {
            _frameSize = frameSize;
        }

        public void SetPacketLoss(int loss)
        {
            _packetLoss = loss;
            if (loss > 0)
            {
                _encoder.PacketLossPercent = _packetLoss;
                _encoder.UseInbandFEC = true;
            }
            else
            {
                _encoder.PacketLossPercent = 0;
                _encoder.UseInbandFEC = false;
            }
        }

        public void SetApplication(OpusApplication application)
        {
            _application = application;
            _encoder.Application = _application;
        }

        public void SetVBRMode(bool vbr, bool constrained)
        {
            _vbr = vbr;
            _cvbr = constrained;
            _encoder.UseVBR = vbr;
            _encoder.UseConstrainedVBR = constrained;
        }

        private int GetFrameSize()
        {
            return (int)(48000 * _channels * _frameSize / 1000);
        }

        public byte[] Compress(short[] input)
        {
            int frameSize = GetFrameSize();

            _incomingSamples.Write(input);

            int outCursor = 0;

            if (_incomingSamples.Available() >= frameSize)
            {
                short[] nextFrameData = _incomingSamples.Read(frameSize);
                int thisPacketSize = _encoder.Encode(nextFrameData, 0, frameSize / _channels, scratchBuffer, outCursor, scratchBuffer.Length);
                outCursor += thisPacketSize;
            }

            byte[] finalOutput = new byte[outCursor];
            Array.Copy(scratchBuffer, 0, finalOutput, 0, outCursor);
            return finalOutput;
        }
    }
}