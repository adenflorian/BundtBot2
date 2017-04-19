using System;
using System.Runtime.InteropServices;

namespace DiscordApiWrapper.Sodium
{
    internal unsafe static class SecretBox
    {
        private static class SafeNativeMethods
        {
            const string sodiumPath = "libsodium.dll";

            [DllImport(sodiumPath, EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);
            [DllImport(sodiumPath, EntryPoint = "crypto_secretbox_open_easy", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SecretBoxOpenEasy(byte[] output, byte* input, long inputLength, byte[] nonce, byte[] secret);
        }

        public static int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            if (input == null) throw new ArgumentNullException();
            if (inputLength < 1) throw new ArgumentException();
            if (output == null) throw new ArgumentNullException();
            if (outputOffset < 0) throw new ArgumentException();
            if (nonce == null) throw new ArgumentNullException();
            if (secret == null) throw new ArgumentNullException();

            fixed (byte* outPtr = output)
                return SafeNativeMethods.SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }
        public static int Decrypt(byte[] input, int inputOffset, long inputLength, byte[] output, byte[] nonce, byte[] secret)
        {
            if (input == null) throw new ArgumentNullException();
            if (inputOffset < 0) throw new ArgumentException();
            if (inputLength < 1) throw new ArgumentException();
            if (output == null) throw new ArgumentNullException();
            if (nonce == null) throw new ArgumentNullException();
            if (secret == null) throw new ArgumentNullException();

            fixed (byte* inPtr = input)
                return SafeNativeMethods.SecretBoxOpenEasy(output, inPtr + inputOffset, inputLength, nonce, secret);
        }
    }
}
