using System;
using System.Collections;
using UnityEngine;

namespace CZCIT.GlassUI
{
    // Fast terminal-like reveals; only inner page content moves, never VR canvases.
    [DisallowMultipleComponent]
    public sealed class GlassUiMotion : MonoBehaviour
    {
        CanvasGroup group;
        Canvas surface;
        Coroutine visibility;
        Coroutine page;
        bool visible = true;
        Vector3 pageRestPosition;
        bool pageShifted;
        readonly CZCIT.GlassUI.GlassRevealSequence reveal = new CZCIT.GlassUI.GlassRevealSequence();
        public bool IsAnimating => visibility != null || page != null;

        public static GlassUiMotion For(GameObject target)
        {
            var motion = target.GetComponent<GlassUiMotion>();
            if (!motion)
                motion = target.AddComponent<GlassUiMotion>();
            motion.Bind();
            return motion;
        }

        void Awake()
        {
            Bind();
        }

        void Bind()
        {
            group = GetComponent<CanvasGroup>();
            if (!group)
                group = gameObject.AddComponent<CanvasGroup>();
            surface = GetComponent<Canvas>();
        }

        public void SetVisible(bool show, bool immediate = false)
        {
            if (!immediate && visible == show && visibility == null)
                return;
            if (!show && page != null)
            {
                StopCoroutine(page);
                page = null;
                RestorePagePosition();
            }
            visible = show;
            if (!immediate && surface)
                CZCIT.GlassUI.GlassUiAudio.Play(show ? CZCIT.GlassUI.GlassUiCue.Open : CZCIT.GlassUI.GlassUiCue.Close);
            if (visibility != null)
                StopCoroutine(visibility);
            visibility = null;
            if (show && surface)
                surface.enabled = true;
            // 显隐动画完成后再恢复交互，防止尚不可见的控件接收射线点击。
            group.blocksRaycasts = group.interactable = false;
            if (immediate || !isActiveAndEnabled)
            {
                reveal.Restore();
                group.alpha = show ? 1 : 0;
                FinishVisibility();
                return;
            }
            visibility = StartCoroutine(AnimateVisibility(show));
        }

        IEnumerator Fade(float target, float duration)
        {
            float from = group.alpha;
            float length = Mathf.Max(.001f, duration * Mathf.Abs(target - from));
            float elapsed = 0;
            while (elapsed < length)
            {
                // 菜单在游戏暂停时也要完成动画，因此使用不受 timeScale 影响的时间。
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / length);
                group.alpha = Mathf.Lerp(from, target, 1 - Mathf.Pow(1 - t, 3));
                yield return null;
            }
            group.alpha = target;
        }

        IEnumerator AnimateVisibility(bool show)
        {
            if (show)
            {
                reveal.Refresh(transform);
                group.alpha = 1;
                for (float t = 0; t < .32f; t += Time.unscaledDeltaTime)
                {
                    reveal.Apply(t / .32f);
                    yield return null;
                }
                reveal.Restore();
            }
            else
            {
                yield return Fade(0, .065f);
                reveal.Restore();
            }
            visibility = null;
            FinishVisibility();
        }

        void FinishVisibility()
        {
            if (surface)
                surface.enabled = visible;
            group.blocksRaycasts = group.interactable = visible && page == null;
        }

        public void ReplaceContent(Action rebuild, bool animate = true)
        {
            if (page != null)
                StopCoroutine(page);
            RestorePagePosition();
            page = null;
            if (!animate || !visible || !isActiveAndEnabled)
            {
                rebuild();
                group.alpha = visible ? 1 : 0;
                FinishVisibility();
                return;
            }
            group.blocksRaycasts = group.interactable = false;
            page = StartCoroutine(Swap(rebuild));
        }

        IEnumerator Swap(Action rebuild)
        {
            yield return Fade(0, .045f);
            reveal.Restore();
            rebuild();
            reveal.Refresh(transform);
            group.alpha = 1;
            for (float t = 0; t < .24f; t += Time.unscaledDeltaTime)
            {
                reveal.Apply(t / .24f);
                yield return null;
            }
            reveal.Restore();
            page = null;
            FinishVisibility();
        }

        void OnDisable()
        {
            StopAllCoroutines();
            reveal.Restore();
            RestorePagePosition();
            visibility = page = null;
            if (group)
            {
                group.alpha = visible ? 1 : 0;
                FinishVisibility();
            }
        }

        void RestorePagePosition()
        {
            if (pageShifted)
            {
                transform.localPosition = pageRestPosition;
                pageShifted = false;
            }
        }
    }
}
