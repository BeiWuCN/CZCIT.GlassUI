// 保存并应用曲面面板位置，恢复时使用原始锚点状态。
using UnityEngine;

namespace CZCIT.GlassUI
{
    public class CurvedPanelAnchor : MonoBehaviour
    {
        [SerializeField] bool stored;
        [SerializeField] Vector3 originalPosition;
        [SerializeField] Quaternion originalRotation;

        public void Apply(float radius)
        {
            if (!stored)
            {
                originalPosition = transform.localPosition;
                originalRotation = transform.localRotation;
                stored = true;
            }
            float angle = originalPosition.x / radius;
            transform.localPosition = new Vector3(
                Mathf.Sin(angle) * radius,
                originalPosition.y,
                originalPosition.z - radius * (1 - Mathf.Cos(angle)));
            transform.localRotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0) * originalRotation;
        }

        public void Restore()
        {
            if (stored)
            {
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
            }
        }
    }

    public static class CurvedGlassLayout
    {

        public static void Apply(Canvas canvas, float radius, string backgroundName)
        {
            foreach (var anchor in canvas.GetComponentsInChildren<CurvedPanelAnchor>(true))
                anchor.Restore();
            foreach (var old in canvas.GetComponentsInChildren<CurvedGlassSurface>(true))
                old.enabled = false;
            foreach (var graphic in canvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                var effect = graphic.GetComponent<CurvedGraphicMesh>();
                if (!effect)
                    effect = graphic.gameObject.AddComponent<CurvedGraphicMesh>();
                effect.root = canvas.transform;
                effect.enabled = true;
                effect.radius = radius;
                graphic.SetVerticesDirty();
            }
            foreach (var oldRaycaster in canvas.GetComponents<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>())
            {
                if (oldRaycaster is CurvedGlassRaycaster) continue;
                if (Application.isPlaying)
                    oldRaycaster.enabled = false;
                else
                    Object.DestroyImmediate(oldRaycaster);
            }
            var raycaster = canvas.GetComponent<CurvedGlassRaycaster>();
            if (!raycaster)
                raycaster = canvas.gameObject.AddComponent<CurvedGlassRaycaster>();
            raycaster.radius = radius;
            raycaster.enabled = true;
            VrButtonClickGuard.Install(canvas);
        }

        public static void ApplyFlat(Canvas canvas)
        {
            if (!canvas) return;
            foreach (var anchor in canvas.GetComponentsInChildren<CurvedPanelAnchor>(true))
                anchor.Restore();
            foreach (var effect in canvas.GetComponentsInChildren<CurvedGraphicMesh>(true))
                if (Application.isPlaying) effect.enabled = false;
                else Object.DestroyImmediate(effect);
            foreach (var raycaster in canvas.GetComponents<CurvedGlassRaycaster>())
                if (Application.isPlaying) raycaster.enabled = false;
                else Object.DestroyImmediate(raycaster);
        }
    }
}
