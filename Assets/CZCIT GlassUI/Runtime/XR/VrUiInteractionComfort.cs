using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CZCIT.GlassUI
{
    // Covers scene-authored buttons and pages created after entering a scene.
    public sealed class VrUiInteractionComfort : MonoBehaviour
    {
        float nextRefresh;
        readonly Vector3[] corners = new Vector3[4];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (FindObjectOfType<VrUiInteractionComfort>()) return;
            var root = new GameObject("UI interaction comfort");
            DontDestroyOnLoad(root);
            root.AddComponent<VrUiInteractionComfort>();
        }

        void OnEnable() { SceneManager.sceneLoaded += SceneLoaded; }
        void OnDisable() { SceneManager.sceneLoaded -= SceneLoaded; }
        void SceneLoaded(Scene scene, LoadSceneMode mode) { nextRefresh = 0; }
        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + .75f;
            Refresh();
        }

        public void Refresh()
        {
            foreach (var canvas in FindObjectsOfType<Canvas>(true))
            {
                if(canvas.renderMode==RenderMode.WorldSpace)
                    foreach(var raw in canvas.GetComponents<GraphicRaycaster>())
                        if(raw.enabled&&raw.GetType()==typeof(GraphicRaycaster))
                        {
                            var physical=canvas.GetComponent<PhysicalGraphicRaycaster>()??canvas.gameObject.AddComponent<PhysicalGraphicRaycaster>();
                            physical.ignoreReversedGraphics=raw.ignoreReversedGraphics;physical.enabled=true;raw.enabled=false;
                        }
                foreach(var text in canvas.GetComponentsInChildren<Text>(true))
                    if(text.GetComponentInParent<Canvas>(true)==canvas)CZCIT.GlassUI.GlassTypography.Apply(text);
                // Curved canvases keep their flatten/bend mapping and use the
                // same release tolerance through their own raycaster.
                var tracked = canvas.GetComponent<TrackedDeviceGraphicRaycaster>();
                if (tracked && tracked.enabled && tracked.GetType() == typeof(TrackedDeviceGraphicRaycaster))
                {
                    var comfort = canvas.GetComponent<ComfortGraphicRaycaster>();
                    if (!comfort) comfort = canvas.gameObject.AddComponent<ComfortGraphicRaycaster>();
                    comfort.ignoreReversedGraphics = tracked.ignoreReversedGraphics;
                    comfort.checkFor2DOcclusion = tracked.checkFor2DOcclusion;
                    comfort.checkFor3DOcclusion = tracked.checkFor3DOcclusion;
                    comfort.blockingMask = tracked.blockingMask;
                    comfort.raycastTriggerInteraction = tracked.raycastTriggerInteraction;
                    tracked.enabled = false;
                }
                VrButtonClickGuard.Install(canvas);
                var buttons = canvas.GetComponentsInChildren<Selectable>(true);
                foreach (var button in buttons)
                {
                    if (button.GetComponentInParent<Canvas>() != canvas || !button.targetGraphic) continue;
                    var graphic = button.targetGraphic;
                    var rect = graphic.rectTransform;
                    if (rect.rect.width <= 0 || rect.rect.height <= 0) continue;
                    bool world = canvas.renderMode == RenderMode.WorldSpace;
                    float sx = Mathf.Max(.00001f, Mathf.Abs(rect.lossyScale.x));
                    float sy = Mathf.Max(.00001f, Mathf.Abs(rect.lossyScale.y));
                    float px = world ? Mathf.Clamp((.036f / sx - rect.rect.width) * .5f, .004f / sx, .008f / sx) : 8;
                    float py = world ? Mathf.Clamp((.036f / sy - rect.rect.height) * .5f, .004f / sy, .008f / sy) : 8;
                    Vector4 padding = new Vector4(px, py, px, py);
                    // Work in the target's frame so scaled/rotated handback
                    // panels get the same physical tolerance as world panels.
                    foreach (var other in buttons)
                    {
                        if (other == button || !other.gameObject.activeInHierarchy || !other.targetGraphic || other.GetComponentInParent<Canvas>() != canvas) continue;
                        other.targetGraphic.rectTransform.GetWorldCorners(corners);
                        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                        foreach (var corner in corners)
                        {
                            Vector3 point = rect.InverseTransformPoint(corner);
                            min = Vector2.Min(min, point); max = Vector2.Max(max, point);
                        }
                        Rect r = rect.rect;
                        if (min.y < r.yMax && max.y > r.yMin)
                        {
                            if (min.x >= r.xMax) padding.z = Mathf.Min(padding.z, (min.x - r.xMax) * .45f);
                            if (max.x <= r.xMin) padding.x = Mathf.Min(padding.x, (r.xMin - max.x) * .45f);
                        }
                        if (min.x < r.xMax && max.x > r.xMin)
                        {
                            if (min.y >= r.yMax) padding.w = Mathf.Min(padding.w, (min.y - r.yMax) * .45f);
                            if (max.y <= r.yMin) padding.y = Mathf.Min(padding.y, (r.yMin - max.y) * .45f);
                        }
                    }
                    graphic.raycastTarget = true;
                    graphic.raycastPadding = -padding;
                    foreach (var text in button.GetComponentsInChildren<Text>(true))
                        if (text != graphic && text.GetComponentInParent<Selectable>() == button && text.GetComponent<IPointerDownHandler>() == null) text.raycastTarget = false;
                }
            }
        }
    }

    public static class VrPressedButtonTolerance
    {
        public static void Preserve(TrackedDeviceEventData data, List<RaycastResult> results, int start, Transform canvas, float radius)
        {
            if (!data.pointerPress || results.Count <= start || data.dragging) return;
            var button = data.pointerPress.GetComponent<Button>();
            if (!button || !button.IsActive() || !button.IsInteractable() || !button.transform.IsChildOf(canvas)) return;
            var hit = results[start];
            var selectable = hit.gameObject.GetComponentInParent<Selectable>();
            if (selectable && selectable != button) return;
            // A slider, pull handle or menu drag surface must retain its hit.
            var drag = ExecuteEvents.GetEventHandler<IDragHandler>(hit.gameObject);
            if (drag && !button.transform.IsChildOf(drag.transform)) return;
            Vector3 point = hit.worldPosition;
            if (radius > 0) point = canvas.TransformPoint(CurvedGraphicMesh.Flatten(canvas.InverseTransformPoint(point), radius));
            var rect = (RectTransform)button.transform;
            point = rect.InverseTransformPoint(point);
            Rect bounds = rect.rect;
            float mx = .010f / Mathf.Max(.00001f, Mathf.Abs(rect.lossyScale.x));
            float my = .010f / Mathf.Max(.00001f, Mathf.Abs(rect.lossyScale.y));
            Vector4 padding = button.targetGraphic ? button.targetGraphic.raycastPadding : Vector4.zero;
            if (point.x < bounds.xMin + padding.x - mx || point.x > bounds.xMax - padding.z + mx || point.y < bounds.yMin + padding.y - my || point.y > bounds.yMax - padding.w + my) return;
            hit.gameObject = button.gameObject;
            hit.screenPosition = data.pointerPressRaycast.screenPosition;
            results.RemoveRange(start, results.Count - start);
            results.Add(hit);
        }
    }
}
