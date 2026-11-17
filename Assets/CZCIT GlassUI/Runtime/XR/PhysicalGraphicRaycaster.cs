using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CZCIT.GlassUI
{
    // Desktop pointers use the same geometry rule as VR rays on world-space screens.
    public sealed class PhysicalGraphicRaycaster : GraphicRaycaster
    {
        public override void Raycast(PointerEventData data,List<RaycastResult> results)
        {
            int start=results.Count;base.Raycast(data,results);
            CZCIT.GlassUI.WorldUiOcclusion.Filter(data,results,start,transform,eventCamera);
        }
    }
}
