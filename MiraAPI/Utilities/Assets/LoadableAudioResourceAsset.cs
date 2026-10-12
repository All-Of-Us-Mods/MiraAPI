using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading .WAV audio assets from the DLL's embedded resources.
/// </summary>
/// <param name="path">The path of the wave file.</param>
public class LoadableAudioResourceAsset(string path) : LoadableAsset<AudioClip>
{
    private readonly Assembly _assembly = Assembly.GetCallingAssembly();

    /// <summary>
    /// Loads the asset from embedded resources.
    /// </summary>
    /// <returns>The asset to load.</returns>
    /// <exception cref="NotSupportedException">Attempted to load an Audio file in non WAV format.</exception>
    /// <exception cref="MissingManifestResourceException">Stream failed to load. Check if the name of your asset was correct.</exception>
    public override AudioClip LoadAsset()
    {
        using var assetStream = _assembly.GetManifestResourceStream(path)
            ?? throw new MissingManifestResourceException($"Stream failed to load. Check if the asset name is correct: {path}");

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
        var audioClip = AudioClip.Create(path, samples.Length / channels, channels, sampleRate, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }

    // This is somewhat magic to me, I just copied it from elsewhere.
    private static float[] ConvertPcmToFloat(byte[] pcmData, int bitsPerSample)
    {
        var sampleCount = pcmData.Length / (bitsPerSample / 8);
        var floatData = new float[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            var byteIndex = i * (bitsPerSample / 8);

            switch (bitsPerSample)
            {
                case 8:
                    floatData[i] = (pcmData[byteIndex] - 128) / 128f; // 8-bit PCM is unsigned
                    break;
                case 16:
                    floatData[i] = BitConverter.ToInt16(pcmData, byteIndex) / 32768f;
                    break;
                case 24:
                    var sample24 = pcmData[byteIndex] | (pcmData[byteIndex + 1] << 8) | (pcmData[byteIndex + 2] << 16);
                    if (sample24 > 0x7FFFFF) sample24 -= 0x1000000; // Convert from unsigned to signed
                    floatData[i] = sample24 / 8388608f;
                    break;
                case 32:
                    floatData[i] = BitConverter.ToInt32(pcmData, byteIndex) / 2147483648f;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported bit depth: {bitsPerSample}");
            }
        }

        return floatData;
    }
}
