// 集中创建和配置玻璃 UI 材质、文字与按钮，减少界面参数分散。
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    // Shared by scene-authored panels and runtime-created application pages.
    public static class GlassStyle
    {
        public const float CornerRadius = 14f;
        public static readonly Color Backdrop = new Color(.16f, .21f, .23f, .78f);
        public static readonly Color Inset = new Color(.22f, .28f, .29f, .48f);


        public static void Material(Material material, Vector2 size, bool backdrop = false)
        {
            GlassPalette.Configure(material, backdrop);
            material.SetFloat("_Aspect", Mathf.Max(1, size.x) / Mathf.Max(1, size.y));
            material.SetFloat("_Radius", Mathf.Min(CornerRadius, Mathf.Min(size.x, size.y) * .5f) / Mathf.Max(1, size.y));
            material.SetFloat("_InteractionHighlight", 0);
            material.SetFloat("_Selected", 0);
            material.SetFloat("_Pressed", 0);
        }


        public static void Button(Button button)
        {
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.16f, 1.16f, 1);
            colors.pressedColor = new Color(.76f, .9f, .91f, 1);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(.55f, .6f, .6f, .5f);
            colors.fadeDuration = .065f;
            button.colors = colors;
            if (!button.GetComponent<CZCIT.GlassUI.GlassButtonFeedback>())
                button.gameObject.AddComponent<CZCIT.GlassUI.GlassButtonFeedback>();
        }


        public static void Text(Text text)
        {
            text.color = text.fontSize >= 24 || text.GetComponentInParent<Button>() ? GlassPalette.Primary : GlassPalette.Secondary;
            var font = Resources.Load<Font>(text.fontSize >= 27 ? "Fonts/GlowSansSC-Medium" : "Fonts/GlowSansSC-Regular");
            if (font)
                text.font = font;
        }
    }
}
