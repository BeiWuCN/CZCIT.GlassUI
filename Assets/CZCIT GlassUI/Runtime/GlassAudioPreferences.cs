// 统一保存主音量、音乐与界面音量设置，使不同页面共享同一份偏好。
using UnityEngine;

namespace CZCIT.GlassUI
{
    public static class GlassAudioPreferences
    {
        public static float Master { get; private set; } = 1;
        public static float Music { get; private set; } = 1;
        public static float Interface { get; private set; } = 1;
        public static float Video { get; private set; } = 1;
        public static event System.Action Changed;
        public static bool Muted
        {
            get; private set;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Master = Mathf.Clamp01(PlayerPrefs.GetFloat("Moon.Settings.Volume", 1));
            Music = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.MusicVolume", 1));
            Interface = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.InterfaceVolume", 1));
            Video = Mathf.Clamp01(PlayerPrefs.GetFloat("Audio.VideoVolume", 1));
            Muted = PlayerPrefs.GetInt("Audio.Muted", 0) != 0;
            ApplyMaster();
        }

        public static void SetMaster(float value)
        {
            Master = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Moon.Settings.Volume", Master);
            ApplyMaster();
            Changed?.Invoke();
        }

        public static void SetMusic(float value)
        {
            Music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Audio.MusicVolume", Music);
            Changed?.Invoke();
        }

        public static void SetInterface(float value)
        {
            Interface = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Audio.InterfaceVolume", Interface);
            Changed?.Invoke();
        }

        public static void SetVideo(float value)
        {
            Video = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("Audio.VideoVolume", Video);
            Changed?.Invoke();
        }

        public static void SetMuted(bool value)
        {
            Muted = value;
            PlayerPrefs.SetInt("Audio.Muted", value ? 1 : 0);
            ApplyMaster();
            Changed?.Invoke();
            PlayerPrefs.Save();
        }

        public static void ApplyMaster()
        {
            AudioListener.volume = Muted ? 0 : Master;
        }

        public static void Reset()
        {
            SetMaster(1);
            SetMusic(1);
            SetInterface(1);
            SetVideo(1);
            SetMuted(false);
            PlayerPrefs.Save();
        }
    }
}
