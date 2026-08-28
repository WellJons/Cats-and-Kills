using UnityEngine;

namespace CatsAndKills.Core
{
    public static class GamePreferences
    {
        private const string MasterVolumeKey = "ck_master_volume";
        private const string ScreenShakeKey = "ck_screen_shake";
        private const string HapticsKey = "ck_haptics";

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, 0.9f);
            set
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
                Apply();
            }
        }

        public static float ScreenShake
        {
            get => PlayerPrefs.GetFloat(ScreenShakeKey, 1f);
            set => PlayerPrefs.SetFloat(ScreenShakeKey, Mathf.Clamp01(value));
        }

        public static bool Haptics
        {
            get => PlayerPrefs.GetInt(HapticsKey, 1) != 0;
            set => PlayerPrefs.SetInt(HapticsKey, value ? 1 : 0);
        }

        public static void Apply()
        {
            AudioListener.volume = MasterVolume;
            PlayerPrefs.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyOnLoad()
        {
            Apply();
        }
    }
}
