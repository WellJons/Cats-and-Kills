using System;
using UnityEngine;

namespace CatsAndKills.Audio
{
    public static class ProceduralAudioFactory
    {
        private const int SampleRate = 44100;

        private static AudioClip _rifle;
        private static AudioClip _pistol;
        private static AudioClip _shotgun;
        private static AudioClip _machineGun;
        private static AudioClip _explosion;
        private static AudioClip _reload;
        private static AudioClip _pin;
        private static AudioClip _casing;
        private static AudioClip _collar;

        private static AudioClip _ambient;
        private static AudioClip _alert;
        private static AudioClip _combat;

        public static AudioClip RifleShot =>
            _rifle ??= CreateGunshot("CK Rifle", 0.34f, 108f, 1.05f, 31);

        public static AudioClip PistolShot =>
            _pistol ??= CreateGunshot("Service Pistol", 0.27f, 154f, 0.82f, 37);

        public static AudioClip ShotgunShot =>
            _shotgun ??= CreateGunshot("Shotgun", 0.52f, 72f, 1.55f, 41);

        public static AudioClip MachineGunShot =>
            _machineGun ??= CreateGunshot("Machine Gun", 0.32f, 92f, 1.12f, 43);

        public static AudioClip Explosion =>
            _explosion ??= CreateExplosion();

        public static AudioClip Reload =>
            _reload ??= CreateMechanical(
                "Reload",
                0.72f,
                new[] { 0.02f, 0.27f, 0.53f },
                new[] { 900f, 520f, 1250f },
                53);

        public static AudioClip GrenadePin =>
            _pin ??= CreateMechanical(
                "Grenade Pin",
                0.23f,
                new[] { 0.01f, 0.10f },
                new[] { 2600f, 1700f },
                59);

        public static AudioClip Casing =>
            _casing ??= CreateMechanical(
                "Casing",
                0.18f,
                new[] { 0.01f, 0.07f },
                new[] { 2200f, 3000f },
                61);

        public static AudioClip Collar =>
            _collar ??= CreateCollarPulse();

        public static AudioClip AmbientMusic =>
            _ambient ??= CreateMusic("Ambient Layer", 0);

        public static AudioClip AlertMusic =>
            _alert ??= CreateMusic("Alert Layer", 1);

        public static AudioClip CombatMusic =>
            _combat ??= CreateMusic("Combat Layer", 2);

        private static AudioClip CreateGunshot(
            string name,
            float duration,
            float bodyFrequency,
            float crack,
            int seed)
        {
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[length];
            var random = new System.Random(seed);

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);

                float transient =
                    noise *
                    Mathf.Exp(-t * 72f) *
                    1.25f *
                    crack;

                float body =
                    Mathf.Sin(2f * Mathf.PI * bodyFrequency * t) *
                    Mathf.Exp(-t * 12f);

                float low =
                    Mathf.Sin(2f * Mathf.PI * bodyFrequency * 0.52f * t) *
                    Mathf.Exp(-t * 8f) *
                    0.5f;

                float mechanical =
                    Mathf.Sin(2f * Mathf.PI * 1800f * t) *
                    Mathf.Exp(-Mathf.Pow((t - 0.055f) / 0.012f, 2f)) *
                    0.16f;

                float sample =
                    (transient + body + low + mechanical) *
                    Mathf.Exp(-t * 5f);

                data[i] = (float)Math.Tanh(sample * 0.82f) * 0.82f;
            }

            return MakeClip(name, data, 1);
        }

        private static AudioClip CreateExplosion()
        {
            const float duration = 1.35f;
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[length];
            var random = new System.Random(71);

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);

                float frequency = Mathf.Lerp(62f, 34f, t / duration);
                float boom =
                    Mathf.Sin(2f * Mathf.PI * frequency * t) *
                    Mathf.Exp(-t * 3.2f) *
                    1.55f;

                float blast =
                    noise *
                    Mathf.Exp(-t * 5.4f) *
                    1.15f;

                data[i] = (float)Math.Tanh(boom + blast) * 0.88f;
            }

            return MakeClip("Explosion", data, 1);
        }

        private static AudioClip CreateMechanical(
            string name,
            float duration,
            float[] times,
            float[] frequencies,
            int seed)
        {
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[length];
            var random = new System.Random(seed);

            for (int click = 0; click < times.Length; click++)
            {
                int start = Mathf.Clamp(
                    Mathf.RoundToInt(times[click] * SampleRate),
                    0,
                    length - 1);

                int clickLength = Mathf.Min(
                    Mathf.RoundToInt(0.075f * SampleRate),
                    length - start);

                for (int j = 0; j < clickLength; j++)
                {
                    float t = j / (float)SampleRate;
                    float noise = (float)(random.NextDouble() * 2.0 - 1.0);

                    float sample =
                        (Mathf.Sin(2f * Mathf.PI * frequencies[click] * t) +
                         noise * 0.22f) *
                        Mathf.Exp(-t * 52f) *
                        0.48f;

                    data[start + j] += sample;
                }
            }

            return MakeClip(name, data, 1);
        }

        private static AudioClip CreateCollarPulse()
        {
            const float duration = 0.32f;
            int length = Mathf.CeilToInt(duration * SampleRate);
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float wobble = Mathf.Sin(2f * Mathf.PI * 7f * t) * 45f;
                float carrier =
                    Mathf.Sin(2f * Mathf.PI * (780f + wobble) * t);

                data[i] =
                    carrier *
                    Mathf.Exp(-t * 9f) *
                    0.32f;
            }

            return MakeClip("Collar Pulse", data, 1);
        }

        private static AudioClip CreateMusic(string name, int intensity)
        {
            const float duration = 12f;
            const int channels = 2;
            int frames = Mathf.RoundToInt(duration * SampleRate);
            float[] data = new float[frames * channels];
            var random = new System.Random(101 + intensity * 7);

            float bpm = intensity == 0 ? 0f : intensity == 1 ? 92f : 126f;
            float beat = bpm > 0f ? 60f / bpm : 1f;

            for (int i = 0; i < frames; i++)
            {
                float t = i / (float)SampleRate;

                float left =
                    Mathf.Sin(2f * Mathf.PI * 43.65f * t) * 0.08f +
                    Mathf.Sin(2f * Mathf.PI * 65.41f * t + 0.4f) * 0.055f;

                float right =
                    Mathf.Sin(2f * Mathf.PI * 43.78f * t + 0.5f) * 0.08f +
                    Mathf.Sin(2f * Mathf.PI * 65.60f * t + 0.9f) * 0.055f;

                float slowMod =
                    0.72f +
                    Mathf.Sin(2f * Mathf.PI * 0.065f * t) * 0.18f;

                left *= slowMod;
                right *= slowMod;

                if (bpm > 0f)
                {
                    float beatPhase = Mathf.Repeat(t, beat);
                    float kickEnvelope = Mathf.Exp(-beatPhase * 24f);

                    float kick =
                        Mathf.Sin(
                            2f * Mathf.PI *
                            Mathf.Lerp(86f, 48f, Mathf.Clamp01(beatPhase * 8f)) *
                            beatPhase) *
                        kickEnvelope *
                        (intensity == 2 ? 0.24f : 0.15f);

                    left += kick;
                    right += kick;

                    if (intensity == 2)
                    {
                        float pulse =
                            Mathf.Pow(
                                Mathf.Max(
                                    0f,
                                    Mathf.Sin(2f * Mathf.PI * (bpm / 120f) * t)),
                                5f);

                        left += Mathf.Sin(2f * Mathf.PI * 73.42f * t) * pulse * 0.07f;
                        right += Mathf.Sin(2f * Mathf.PI * 73.60f * t + 0.08f) * pulse * 0.07f;
                    }
                }

                float hiss =
                    (float)(random.NextDouble() * 2.0 - 1.0) *
                    (intensity == 0 ? 0.007f : 0.012f);

                data[i * channels] = (float)Math.Tanh((left + hiss) * 1.25f) * 0.78f;
                data[i * channels + 1] = (float)Math.Tanh((right - hiss) * 1.25f) * 0.78f;
            }

            return MakeClip(name, data, channels);
        }

        private static AudioClip MakeClip(
            string name,
            float[] data,
            int channels)
        {
            int frames = data.Length / channels;

            AudioClip clip = AudioClip.Create(
                name,
                frames,
                channels,
                SampleRate,
                false);

            clip.SetData(data, 0);
            return clip;
        }
    }
}
