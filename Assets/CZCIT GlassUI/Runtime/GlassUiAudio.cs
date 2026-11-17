// 按 UI 提示类型播放共享音效，并维护预加载和音源生命周期。
using UnityEngine;

namespace CZCIT.GlassUI
{
    public enum GlassUiCue
    {
        Hover, Select, Confirm, Open, Close
    }

    public sealed class GlassUiAudio : MonoBehaviour
    {
        static GlassUiAudio instance;
        readonly AudioClip[] clips = new AudioClip[5];
        AudioSource detail;
        AudioSource action;
        AudioSource panel;
        float detailGain;
        float actionGain;
        float panelGain;
        float nextHover;
        float nextPanel;
        float lastAction = -1;
        public static GlassUiCue LastCue
        {
            get; private set;
        }
        public static int PlayedCount
        {
            get; private set;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            instance = null;
            PlayedCount = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preload()
        {
            Get();
        }

        static GlassUiAudio Get()
        {
            if (instance)
                return instance;
            instance = new GameObject("Glass UI audio").AddComponent<GlassUiAudio>();
            DontDestroyOnLoad(instance.gameObject);
            instance.detail = instance.Source();
            instance.action = instance.Source();
            instance.panel = instance.Source();
            foreach (GlassUiCue cue in System.Enum.GetValues(typeof(GlassUiCue)))
                instance.clips[(int)cue] = Resources.Load<AudioClip>("GlassUI/" + cue);
            return instance;
        }

        AudioSource Source()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.spatialBlend = 0;
            s.priority = 32;
            s.dopplerLevel = 0;
            return s;
        }

        public static void Play(GlassUiCue cue)
        {
            if (GlassAudioPreferences.Interface <= 0 || GlassAudioPreferences.Muted || GlassAudioPreferences.Master <= 0)
                return;
            var audio = Get();
            float now = Time.unscaledTime;
            var source = audio.action;
            float gain = cue == GlassUiCue.Confirm ? .62f : cue == GlassUiCue.Select ? .48f : cue == GlassUiCue.Hover ? .18f : cue == GlassUiCue.Open ? .32f : .28f;
            if (cue == GlassUiCue.Hover)
            {
                if (now < audio.nextHover || now - audio.lastAction < .14f)
                    return;
                audio.nextHover = now + .09f;
                source = audio.detail;
            }
            else if (cue == GlassUiCue.Open || cue == GlassUiCue.Close)
            {
                if (now < audio.nextPanel)
                    return;
                audio.nextPanel = now + .10f;
                source = audio.panel;
            }
            else
            {
                audio.lastAction = now;
                audio.detail.Stop();
            }
            var clip = audio.clips[(int)cue];
            if (!clip)
                return;
            if (source == audio.detail)
                audio.detailGain = gain;
            else if (source == audio.panel)
                audio.panelGain = gain;
            else
                audio.actionGain = gain;
            source.Stop();
            source.clip = clip;
            source.volume = gain * GlassAudioPreferences.Interface;
            source.Play();
            LastCue = cue;
            PlayedCount++;
        }

        void Update()
        {
            if (detail && detail.isPlaying)
                detail.volume = detailGain * GlassAudioPreferences.Interface;
            if (action && action.isPlaying)
                action.volume = actionGain * GlassAudioPreferences.Interface;
            if (panel && panel.isPlaying)
                panel.volume = panelGain * GlassAudioPreferences.Interface;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
