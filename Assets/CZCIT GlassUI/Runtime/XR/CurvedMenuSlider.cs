// 将鼠标拖动与追踪设备输入统一映射到曲面滑条数值。
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using CZCIT.GlassUI;
using UnityEngine.Events;

namespace CZCIT.GlassUI
{
    // Slider's stock screen-to-plane drag does not account for the bent UI mesh.
    public sealed class CurvedMenuSlider : Slider, IEndDragHandler
    {
        public float curveRadius = 2200;
        public UnityEvent onInteractionCompleted = new UnityEvent();

        public override void OnPointerDown(PointerEventData data)
        {
            if (data is TrackedDeviceEventData tracked)
            {
                if (data.button != PointerEventData.InputButton.Left || !IsActive() || !IsInteractable())
                    return;
                if (EventSystem.current)
                    EventSystem.current.SetSelectedGameObject(gameObject, data);
                UpdateTracked(tracked);
            }
            else
                base.OnPointerDown(data);
        }

        public override void OnDrag(PointerEventData data)
        {
            if (data is TrackedDeviceEventData tracked)
            {
                if (IsActive() && IsInteractable())
                    UpdateTracked(tracked);
            }
            else
                base.OnDrag(data);
        }

        void UpdateTracked(TrackedDeviceEventData data)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas || data.rayPoints == null)
                return;
            var root = canvas.transform;
            var area = handleRect ? (RectTransform)handleRect.parent : (RectTransform)transform;
            float z = root.InverseTransformPoint(area.position).z;
            for (int i = 0; i < data.rayPoints.Count - 1; i++)
            {
                Vector3 previous = CurvedGraphicMesh.Flatten(root.InverseTransformPoint(data.rayPoints[i]), curveRadius);
                for (int segment = 1; segment <= 12; segment++)
                {
                    var world = Vector3.Lerp(data.rayPoints[i], data.rayPoints[i + 1], segment / 12f);
                    var next = CurvedGraphicMesh.Flatten(root.InverseTransformPoint(world), curveRadius);
                    float dz = next.z - previous.z;
                    if (Mathf.Abs(dz) > .00001f)
                    {
                        float t = (z - previous.z) / dz;
                        if (t >= 0 && t <= 1)
                        {
                            var local = area.InverseTransformPoint(root.TransformPoint(Vector3.Lerp(previous, next, t)));
                            normalizedValue = Mathf.InverseLerp(area.rect.xMin, area.rect.xMax, local.x);
                            return;
                        }
                    }
                    previous = next;
                }
            }
        }

        public override void OnPointerUp(PointerEventData data)
        {
            base.OnPointerUp(data);
            onInteractionCompleted.Invoke();
        }

        public void OnEndDrag(PointerEventData data)
        {
            onInteractionCompleted.Invoke();
        }
    }
}
