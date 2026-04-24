using System.IO;
using System.Media;

namespace AudioDevSwitcher.Helpers;

/// <summary>
/// Plays a short synthesized "ding" after a small delay so the tone is
/// heard through the newly-selected default audio device.
/// </summary>
public static class ConfirmationTonePlayer
{
    private const int SampleRate = 44100;
    private const int DelayMs = 400;
    private static readonly byte[] ToneWav = BuildToneWav();

    public static void PlayAsync()
    {
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DelayMs);
                using var ms = new MemoryStream(ToneWav);
                using var player = new SoundPlayer(ms);
                player.PlaySync();
            }
            catch
            {
                // Tone playback is a nicety; never surface errors to the user.
            }
        });
    }

    private static byte[] BuildToneWav()
    {
        // Two short overlapping sine tones (A5 -> E6) for a pleasant "ding".
        const int durationMs = 220;
        int sampleCount = SampleRate * durationMs / 1000;
        var samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double t = i / (double)SampleRate;
            // Attack/release envelope to avoid clicks.
            double env = Math.Min(1.0, Math.Min(t * 40.0, (durationMs / 1000.0 - t) * 10.0));
            env = Math.Max(0.0, env);
            double s =
                Math.Sin(2 * Math.PI * 880.0 * t) * 0.5 +
                Math.Sin(2 * Math.PI * 1318.5 * t) * 0.3;
            samples[i] = (short)(s * env * short.MaxValue * 0.5);
        }

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        int dataSize = samples.Length * 2;

        bw.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
        bw.Write(36 + dataSize);
        bw.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });
        bw.Write(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
        bw.Write(16);               // fmt chunk size
        bw.Write((short)1);         // PCM
        bw.Write((short)1);         // mono
        bw.Write(SampleRate);
        bw.Write(SampleRate * 2);   // byte rate
        bw.Write((short)2);         // block align
        bw.Write((short)16);        // bits per sample
        bw.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
        bw.Write(dataSize);
        foreach (var s in samples)
            bw.Write(s);

        return ms.ToArray();
    }
}
