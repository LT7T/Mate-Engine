using System;
using System.IO;
using UnityEngine;

namespace MateEngine.Voice.Utils
{
    /// <summary>
    /// Utility class for audio format conversions and audio processing
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// Convert WAV byte array to AudioClip (stub implementation)
        /// </summary>
        public static AudioClip WavToAudioClip(byte[] wavData)
        {
            // TODO: Implement proper WAV parsing if needed
            Debug.LogWarning("AudioUtils.WavToAudioClip is a stub. Implement WAV parsing as needed.");
            return null;
        }
        /// <summary>
        /// Convert AudioClip to WAV format bytes
        /// </summary>
        public static byte[] AudioClipToWav(AudioClip clip)
        {
            if (clip == null) return null;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            return ConvertSamplesToWav(samples, clip.frequency, clip.channels);
        }

        /// <summary>
        /// Convert float samples to WAV format
        /// </summary>
        public static byte[] ConvertSamplesToWav(float[] samples, int frequency, int channels)
        {
            int bytesPerSample = 2; // 16-bit
            int dataLength = samples.Length * bytesPerSample;
            int headerLength = 44;
            
            byte[] wav = new byte[headerLength + dataLength];
            
            // WAV header
            WriteString(wav, 0, "RIFF");
            WriteInt32(wav, 4, 36 + dataLength);
            WriteString(wav, 8, "WAVE");
            WriteString(wav, 12, "fmt ");
            WriteInt32(wav, 16, 16); // PCM format chunk size
            WriteInt16(wav, 20, 1);  // PCM format
            WriteInt16(wav, 22, (short)channels);
            WriteInt32(wav, 24, frequency);
            WriteInt32(wav, 28, frequency * channels * bytesPerSample); // byte rate
            WriteInt16(wav, 32, (short)(channels * bytesPerSample)); // block align
            WriteInt16(wav, 34, (short)(bytesPerSample * 8)); // bits per sample
            WriteString(wav, 36, "data");
            WriteInt32(wav, 40, dataLength);
            
            // Convert samples to 16-bit PCM
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)(Mathf.Clamp(samples[i], -1f, 1f) * short.MaxValue);
                WriteInt16(wav, headerLength + i * bytesPerSample, sample);
            }
            
            return wav;
        }

        /// <summary>
        /// Create AudioClip from PCM byte data
        /// </summary>
        public static AudioClip CreateAudioClipFromPCM(byte[] pcmData, int frequency, int channels, string name = "GeneratedClip")
        {
            int bytesPerSample = 2; // 16-bit
            int samples = pcmData.Length / bytesPerSample / channels;
            
            AudioClip clip = AudioClip.Create(name, samples, channels, frequency, false);
            float[] floatData = new float[samples * channels];
            
            for (int i = 0; i < samples * channels; i++)
            {
                short sample = ReadInt16(pcmData, i * bytesPerSample);
                floatData[i] = sample / (float)short.MaxValue;
            }
            
            clip.SetData(floatData, 0);
            return clip;
        }

        /// <summary>
        /// Trim silence from the beginning and end of an AudioClip
        /// </summary>
        public static AudioClip TrimSilence(AudioClip clip, float threshold = 0.01f)
        {
            if (clip == null) return null;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            int start = 0;
            int end = samples.Length - 1;

            // Find start of audio
            for (int i = 0; i < samples.Length; i++)
            {
                if (Mathf.Abs(samples[i]) > threshold)
                {
                    start = i;
                    break;
                }
            }

            // Find end of audio
            for (int i = samples.Length - 1; i >= 0; i--)
            {
                if (Mathf.Abs(samples[i]) > threshold)
                {
                    end = i;
                    break;
                }
            }

            if (start >= end) return null;

            int newLength = end - start + 1;
            float[] trimmedSamples = new float[newLength];
            Array.Copy(samples, start, trimmedSamples, 0, newLength);

            AudioClip trimmedClip = AudioClip.Create("TrimmedClip", newLength / clip.channels, clip.channels, clip.frequency, false);
            trimmedClip.SetData(trimmedSamples, 0);

            return trimmedClip;
        }

        /// <summary>
        /// Apply basic noise reduction to audio samples
        /// </summary>
        public static float[] ApplyNoiseReduction(float[] samples, float noiseThreshold = 0.05f)
        {
            float[] processed = new float[samples.Length];
            
            for (int i = 0; i < samples.Length; i++)
            {
                float sample = samples[i];
                
                // Simple noise gate
                if (Mathf.Abs(sample) < noiseThreshold)
                {
                    processed[i] = 0f;
                }
                else
                {
                    processed[i] = sample;
                }
            }
            
            return processed;
        }

        /// <summary>
        /// Calculate RMS (Root Mean Square) level of audio samples
        /// </summary>
        public static float CalculateRMS(float[] samples)
        {
            if (samples == null || samples.Length == 0) return 0f;
            
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }
            
            return Mathf.Sqrt(sum / samples.Length);
        }

        /// <summary>
        /// Check if audio contains speech-like characteristics
        /// </summary>
        public static bool ContainsSpeech(AudioClip clip, float speechThreshold = 0.02f)
        {
            if (clip == null) return false;

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            float rms = CalculateRMS(samples);
            return rms > speechThreshold;
        }

        /// <summary>
        /// Get audio level for visualization
        /// </summary>
        public static float GetAudioLevel(AudioClip clip, int position, int windowSize = 1024)
        {
            if (clip == null) return 0f;

            float[] samples = new float[windowSize];
            clip.GetData(samples, Mathf.Max(0, position - windowSize / 2));

            return CalculateRMS(samples);
        }

        // Helper methods for byte manipulation
        private static void WriteString(byte[] data, int offset, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                data[offset + i] = (byte)value[i];
            }
        }

        private static void WriteInt32(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
            data[offset + 2] = (byte)((value >> 16) & 0xFF);
            data[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static void WriteInt16(byte[] data, int offset, short value)
        {
            data[offset] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static short ReadInt16(byte[] data, int offset)
        {
            return (short)(data[offset] | (data[offset + 1] << 8));
        }
    }

    /// <summary>
    /// Audio format converter for various audio file formats
    /// </summary>
    public class AudioFormatConverter
    {
        /// <summary>
        /// Convert MP3 bytes to AudioClip (simplified - requires additional libraries for full implementation)
        /// </summary>
        public static AudioClip ConvertMP3ToAudioClip(byte[] mp3Data, string name = "ConvertedClip")
        {
            // This is a placeholder implementation
            // For full MP3 support, you would need libraries like NAudio MP3 decoder
            // or Unity's experimental audio import system
            
            Debug.LogWarning("[AudioFormatConverter] MP3 conversion not fully implemented. Using fallback.");
            
            // Return empty clip as fallback
            return AudioClip.Create(name, 1, 1, 22050, false);
        }

        /// <summary>
        /// Create a simple test tone for audio system testing
        /// </summary>
        public static AudioClip CreateTestTone(float frequency = 440f, float duration = 1f, int sampleRate = 44100)
        {
            int samples = (int)(duration * sampleRate);
            AudioClip clip = AudioClip.Create("TestTone", samples, 1, sampleRate, false);
            
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * 0.1f; // Low volume
            }
            
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Convert WAV byte array to AudioClip
        /// </summary>
        public static AudioClip WavToAudioClip(byte[] wavData)
        {
            if (wavData == null || wavData.Length < 44)
            {
                Debug.LogError("[AudioUtils] Invalid WAV data");
                return null;
            }

            try
            {
                // Parse WAV header
                int headerSize = 44;
                int channels = System.BitConverter.ToInt16(wavData, 22);
                int sampleRate = System.BitConverter.ToInt32(wavData, 24);
                int bitsPerSample = System.BitConverter.ToInt16(wavData, 34);
                int dataSize = System.BitConverter.ToInt32(wavData, 40);

                if (bitsPerSample != 16)
                {
                    Debug.LogError($"[AudioUtils] Unsupported bits per sample: {bitsPerSample}. Only 16-bit WAV is supported.");
                    return null;
                }

                // Extract audio data
                int sampleCount = dataSize / (bitsPerSample / 8);
                float[] samples = new float[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    int byteIndex = headerSize + i * 2;
                    if (byteIndex + 1 < wavData.Length)
                    {
                        short sample = System.BitConverter.ToInt16(wavData, byteIndex);
                        samples[i] = sample / 32768f; // Convert to float range [-1, 1]
                    }
                }

                // Create AudioClip
                AudioClip clip = AudioClip.Create("GeneratedSpeech", sampleCount / channels, channels, sampleRate, false);
                clip.SetData(samples, 0);

                return clip;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AudioUtils] Error converting WAV to AudioClip: {e.Message}");
                return null;
            }
        }
    }
}