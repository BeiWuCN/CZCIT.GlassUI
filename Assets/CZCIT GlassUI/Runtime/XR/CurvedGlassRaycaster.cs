// 把指针命中适配到曲面 UI，保持视觉位置与交互位置对应。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CZCIT.GlassUI
{
    public class CurvedGlassRaycaster : TrackedDeviceGraphicRaycaster
    {
        public float radius = 1600;
        readonly List<Vector3> flattened = new List<Vector3>();
        // Declare the Unity message on the concrete component so the toolkit's
        // poke registry is initialized before this curved raycaster is disabled.
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            VrButtonClickGuard.Install(GetComponent<Canvas>());
        }

        public override void Raycast(PointerEventData data, List<RaycastResult> results)
        {
            if (!(data is TrackedDeviceEventData tracked) || tracked.rayPoints == null || tracked.rayPoints.Count < 2)
                return;
            var original = tracked.rayPoints;
            flattened.Clear();
            const int segments = 12;
            for (int i = 0; i < original.Count - 1; i++)
                for (int s = 0; s < segments; s++)
                    flattened.Add(transform.TransformPoint(CurvedGraphicMesh.Flatten(
                        transform.InverseTransformPoint(Vector3.Lerp(original[i], original[i + 1], (float)s / segments)),
                        radius)));
            flattened.Add(transform.TransformPoint(CurvedGraphicMesh.Flatten(transform.InverseTransformPoint(original[original.Count - 1]), radius)));
            int start = results.Count;
            tracked.rayPoints = flattened;
            try
            {
                base.Raycast(data, results);
            }
            finally
            {
                tracked.rayPoints = original;
            }
            for (int i = start; i < results.Count; i++)
            {
                var hit = results[i];
                hit.worldPosition = transform.TransformPoint(CurvedGraphicMesh.Bend(transform.InverseTransformPoint(hit.worldPosition), radius));
                hit.distance = Vector3.Distance(original[0], hit.worldPosition);
                // XRUIInputModule compares the press's world projection with this position.
                // Keeping the flattened screen coordinate falsely starts a drag and cancels clicks.
                if (eventCamera)
                    hit.screenPosition = eventCamera.WorldToScreenPoint(hit.worldPosition);
                results[i] = hit;
            }
            if (results.Count > start)
                tracked.rayHitIndex = Mathf.Clamp((tracked.rayHitIndex + segments - 1) / segments, 1, original.Count - 1);
            VrPressedButtonTolerance.Preserve(tracked, results, start, transform, radius);
            CZCIT.GlassUI.WorldUiOcclusion.Filter(data,results,start,transform,eventCamera);
        }

    }
}
