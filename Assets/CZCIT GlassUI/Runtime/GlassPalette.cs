// 集中提供玻璃 UI 的配色配置，供编辑器与运行时样式共同使用。
using UnityEngine;

namespace CZCIT.GlassUI
{
    public static class GlassPalette
    {
        public static readonly Color Primary = new Color(.95f, .97f, .97f, 1);
        public static readonly Color Secondary = new Color(.73f, .8f, .81f, 1);
        public static readonly Color Accent = new Color(.63f, .87f, .89f, 1);
        public static readonly Color Surface = new Color(.23f, .28f, .29f, .65f);
        public static readonly Color Selected = new Color(.28f, .53f, .56f, .78f);
        public static readonly Color Action = new Color(.37f, .64f, .66f, .88f);

        public static void Configure(Material material, bool backdrop = false)
        {
            material.SetFloat("_MenuLighting", 1);
            material.SetFloat("_BackgroundLimit", .42f);
            material.SetFloat("_Transmission", .82f);
            material.SetFloat("_TintStrength", .25f);
            material.SetFloat("_SheenStrength", .35f);
            material.SetFloat("_RimStrength", backdrop ? .24f : .18f);
            material.SetFloat("_Frost", backdrop ? .12f : .24f);
            material.SetFloat("_Blur", backdrop ? 2.2f : 1.5f);
        }
    }
}
