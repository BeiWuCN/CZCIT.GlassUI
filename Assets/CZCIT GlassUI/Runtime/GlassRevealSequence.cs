// 管理内容逐步显现及原始状态恢复，供面板显隐动画复用。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    // Short, ordered signal-lock beats. Geometry and hit targets stay stationary.
    public sealed class GlassRevealSequence
    {
        struct Caption
        {
            public CanvasGroup group;
            public float alpha;
        }
        struct Layer
        {
            public CanvasGroup group;
            public float alpha;
            public float y;
            public float x;
            public bool frame;
            public Material surface;
            public List<Caption> captions;
        }
        readonly List<Layer> layers = new List<Layer>();

        public void Refresh(Transform root)
        {
            Restore();
            layers.Clear();
            foreach (Transform child in root)
            {
                if (!child.gameObject.activeSelf || child.GetComponent<CZCIT.GlassUI.GlassUiMotion>() || child.GetComponent<Canvas>())
                    continue;
                var rect = child as RectTransform;
                if (!rect)
                    continue;
                var group = child.GetComponent<CanvasGroup>();
                if (!group)
                    group = child.gameObject.AddComponent<CanvasGroup>();
                bool frame = child.name == "Background" || child.name == "FrostedBackdrop" || child.name == "PageSurface";
                var image = child.GetComponent<Image>();
                Material surface = null;
                var captions = new List<Caption>();
                if (image && image.material && image.material.HasProperty("_Reveal"))
                {
                    surface = CZCIT.GlassUI.GlassSurfaceMaterial.For(image);
                    foreach (var text in child.GetComponentsInChildren<Text>())
                    {
                        var caption = text.GetComponent<CanvasGroup>();
                        if (!caption)
                            caption = text.gameObject.AddComponent<CanvasGroup>();
                        captions.Add(new Caption { group = caption, alpha = caption.alpha });
                    }
                }
                layers.Add(new Layer
                {
                    group = group,
                    alpha = group.alpha,
                    y = frame ? float.MaxValue : rect.anchoredPosition.y,
                    x = rect.anchoredPosition.x,
                    frame = frame,
                    surface = surface,
                    captions = captions
                });
            }
            layers.Sort((a, b) => a.y == b.y ? a.x.CompareTo(b.x) : b.y.CompareTo(a.y));
        }

        public void Apply(float progress)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (!layer.group)
                    continue;
                float delay = layer.frame ? 0 : .18f + Mathf.Min(i * .035f, .36f);
                float t = Mathf.Clamp01((progress - delay) / (layer.frame ? .52f : .44f));
                // Graphic carrier scans into place, then its text locks in. Never
                // translate, scale, scramble or flicker the text/hit rectangle.
                if (layer.surface)
                {
                    layer.surface.SetFloat("_Reveal", 1 - Mathf.Pow(1 - t, 3));
                    layer.group.alpha = t > 0 ? layer.alpha : 0;
                    float caption = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.48f, .72f, t));
                    foreach (var label in layer.captions)
                        if (label.group)
                            label.group.alpha = label.alpha * caption;
                }
                else
                    layer.group.alpha = layer.alpha * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.08f, .28f, t));
            }
        }

        public void Restore()
        {
            foreach (var layer in layers)
            {
                if (layer.group)
                    layer.group.alpha = layer.alpha;
                if (layer.surface)
                    layer.surface.SetFloat("_Reveal", 1);
                if (layer.captions != null)
                    foreach (var caption in layer.captions)
                        if (caption.group)
                            caption.group.alpha = caption.alpha;
            }
        }
    }

    public static class GlassRenderingBudget
    {
        public static bool SuspendCaptures;
    }
}
