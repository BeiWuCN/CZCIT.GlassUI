using UnityEngine;

namespace CZCIT.GlassUI
{
    [DefaultExecutionOrder(1000)]
    public sealed class GlassTypographyRuntime : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (FindObjectOfType<GlassTypographyRuntime>()) return;
            var root = new GameObject("UI typography");
            DontDestroyOnLoad(root);
            root.AddComponent<GlassTypographyRuntime>();
        }

        void LateUpdate() { GlassTypography.RefreshDynamic(); }
    }
}
