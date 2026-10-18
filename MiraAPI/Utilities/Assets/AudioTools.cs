using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for various audio-related operations.
/// </summary>
public static class AudioTools
{
    /// <summary>
    /// Loads and returns an audio clip from a resource path using the specified <see cref="Assembly"/>.
    /// </summary>
    /// <param name="resourcePath">The path to the resource within the <see cref="Assembly"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> from which to load the resource.</param>
    /// <returns>A <see cref="AudioClip"/> object loaded from the specified resource path.</returns>
    /// <exception cref="NotSupportedException">Attempted to load an Audio file in non WAV format.</exception>
    /// <exception cref="MissingManifestResourceException">Stream failed to load. Check if the name of your asset was correct.</exception>
    public static AudioClip LoadAudioFromResources(string resourcePath, Assembly assembly)
    {
        using var assetStream = assembly.GetManifestResourceStream(resourcePath)
            ?? throw new MissingManifestResourceException($"Stream failed to load. Check if the asset name is correct: {resourcePath}");

        using var reader = new BinaryReader(assetStream);

        // Read RIFF header
        if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "RIFF")
        {
            throw new NotSupportedException("Invalid WAV file (missing RIFF header).");
        }

        reader.ReadInt32(); // File size (ignore)

        if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE")
        {
            throw new NotSupportedException("Invalid WAV file (missing WAVE header).");
        }

        // Read chunks
        int channels = 0, sampleRate = 0, bitsPerSample = 0;
        byte[]? audioData = null;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var chunkSize = reader.ReadInt32();

            // Format chunk
            if (chunkId == "fmt ")
            {
                int audioFormat = reader.ReadInt16();
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // Byte rate (ignore)
                reader.ReadInt16(); // Block align (ignore)
                bitsPerSample = reader.ReadInt16();

                if (audioFormat != 1) // Only PCM supported
                    throw new NotSupportedException("Only PCM WAV files are supported.");
            }

            // Data chunk
            else if (chunkId == "data")
            {
                audioData = reader.ReadBytes(chunkSize);
                break; // No need to read further
            }
            else
            {
                reader.BaseStream.Seek(chunkSize, SeekOrigin.Current); // Skip unknown chunks
            }
        }

        if (audioData == null)
        {
            throw new InvalidOperationException("WAV file does not contain audio data.");
        }

        if (channels == 0 || sampleRate == 0 || bitsPerSample == 0)
        {
            throw new InvalidOperationException("WAV file does not contain valid format information.");
        }

        // Convert PCM data to float array
        var samples = ConvertPcmToFloat(audioData, bitsPerSample);

        // Create the AudioClip
        var audioClip = AudioClip.Create(resourcePath, samples.Length / channels, channels, sampleRate, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }

    // This is somewhat magic to me, I just copied it from elsewhere.
    // Could have been written better, so here - Az
    private static float[] ConvertPcmToFloat(byte[] pcmData, int bitsPerSample)
    {
        var sampleCount = pcmData.Length / (bitsPerSample / 8);
        var floatData = new float[sampleCount];

        ReadOnlySpan<byte> source = pcmData;
        Span<float> destination = floatData;

        switch (bitsPerSample)
        {
            case 8:
                for (var i = 0; i < sampleCount; i++)
                {
                    destination[i] = (source[i] - 128) / 128f; // 8-bit PCM is unsigned
                }

                break;

            case 16:
                var shortSource = MemoryMarshal.Cast<byte, short>(source);

                for (var i = 0; i < sampleCount; i++)
                {
                    destination[i] = shortSource[i] / 32768f;
                }

                break;

            case 24:
                for (int i = 0, byteIndex = 0; i < sampleCount; i++, byteIndex += 3)
                {
                    var sample24 = source[byteIndex] | (source[byteIndex + 1] << 8) | (source[byteIndex + 2] << 16);

                    // Convert from unsigned to signed
                    if (sample24 > 0x7FFFFF)
                        sample24 -= 0x1000000;

                    destination[i] = sample24 / 8388608f;
                }

                break;

            case 32:
                var intSource = MemoryMarshal.Cast<byte, int>(source);

                for (var i = 0; i < sampleCount; i++)
                {
                    destination[i] = intSource[i] / 2147483648f;
                }

                break;

            default:
                throw new NotSupportedException($"Unsupported bit depth: {bitsPerSample}");
        }

        return floatData;
    }
}
