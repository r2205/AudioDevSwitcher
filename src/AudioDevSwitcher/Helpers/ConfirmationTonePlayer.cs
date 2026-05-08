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
    private const int DelayMs = 800;
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
        // Two-note "bi-bing" (A5 then E6) — long enough that Bluetooth
        // headsets have time to fully wake their audio link mid-playback.
        const int durationMs = 700;
        const double durationSec = durationMs / 1000.0;
        const double note1Freq = 880.0;   // A5
        const double note2Freq = 1318.5;  // E6
        const double note2Start = 0.30;   // seconds — when the second note begins
        int sampleCount = SampleRate * durationMs / 1000;
        var samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double t = i / (double)SampleRate;
            // 80ms fade in, 200ms fade out — gentle enough for Bluetooth path opens.
            double fadeIn = Math.Min(1.0, t / 0.08);
            double fadeOut = Math.Min(1.0, (durationSec - t) / 0.20);
            double env = Math.Max(0.0, Math.Min(fadeIn, fadeOut));

            double note1 = Math.Sin(2 * Math.PI * note1Freq * t);
            double note2Env = t >= note2Start ? Math.Min(1.0, (t - note2Start) / 0.04) : 0.0;
            double note2 = Math.Sin(2 * Math.PI * note2Freq * t) * note2Env;
            double s = note1 * 0.5 + note2 * 0.4;

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
