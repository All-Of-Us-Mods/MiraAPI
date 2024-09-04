using System;
using System.IO;
using System.Reflection;
using System.Text;
using Cpp2IL.Core.Extensions;
using Reactor.Utilities.Extensions;
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
    /// <exception cref="FileNotFoundException">Stream failed to load. Check if the name of your asset was correct.</exception>
    public override AudioClip LoadAsset()
    {
        var assetStream = _assembly.GetManifestResourceStream(path) ??
                          throw new FileNotFoundException(
                              "Stream failed to load. Check if the name of your asset was correct.");
        var audioBytes = assetStream.ReadFully();

        var riffHeader = Encoding.ASCII.GetString(audioBytes.SubArray(0, 4));
        var waveHeader = Encoding.ASCII.GetString(audioBytes.SubArray(8, 4));

        if (riffHeader != "RIFF" || waveHeader != "WAVE")
        {
            throw new NotSupportedException($"Attempted to load an Audio file in non WAV format '{path}'.");
        }

        int channels = BitConverter.ToInt16(audioBytes, 22);
        var sampleRate = BitConverter.ToInt32(audioBytes, 24);
        var dataSize = BitConverter.ToInt32(audioBytes, 40);

        var audioData = new float[dataSize / 2];
        for (var i = 0; i < audioData.Length; i++)
        {
            audioData[i] = BitConverter.ToInt16(audioBytes, 44 + i * 2) / 32768.0f;
        }

        var audioClip = AudioClip.Create(
            "WavClip",
            audioData.Length,
            channels,
            sampleRate,
            false
        );
        audioClip.SetData(audioData, 0);

        return audioClip;
    }
}
