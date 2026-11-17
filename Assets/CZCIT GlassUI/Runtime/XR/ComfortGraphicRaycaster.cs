using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CZCIT.GlassUI
{
    public sealed class ComfortGraphicRaycaster : TrackedDeviceGraphicRaycaster
    {
        protected override void Awake() { base.Awake(); }
        public override void Raycast(PointerEventData data, List<RaycastResult> results)
        {
            int start = results.Count;
            base.Raycast(data, results);
            if (data is TrackedDeviceEventData tracked)
                VrPressedButtonTolerance.Preserve(tracked, results, start, transform, 0);
            CZCIT.GlassUI.WorldUiOcclusion.Filter(data,results,start,transform,eventCamera);
        }
    }

}
