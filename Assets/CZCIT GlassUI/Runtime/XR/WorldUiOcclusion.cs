using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
namespace CZCIT.GlassUI
{
    // Rendering and input share physical occlusion; UI triggers and the tracked body are excluded.
    public static class WorldUiOcclusion
    {
        static readonly RaycastHit[] hits=new RaycastHit[128];
        public static float ClearFraction(Vector3 origin,Vector3 destination,Transform surface,Camera viewer)
        {
            Vector3 delta=destination-origin;float distance=delta.magnitude;if(distance<.01f)return 1;
            int count=Physics.RaycastNonAlloc(origin,delta/distance,hits,distance+.025f,~0,QueryTriggerInteraction.Ignore);
            float clear=1;
            for(int i=0;i<count;i++)
            {
                var collider=hits[i].collider;if(!collider||collider is CharacterController||collider.gameObject.layer==5)continue;
                if(collider.GetComponentInParent<Canvas>())continue;
                if(viewer&&collider.transform.IsChildOf(viewer.transform.root))continue;
                if(surface&&collider.transform.IsChildOf(surface))continue;
                clear=Mathf.Min(clear,Mathf.Clamp01((hits[i].distance-.045f)/distance));
            }
            return clear;
        }
        public static void Filter(PointerEventData data,List<RaycastResult> results,int start,Transform surface,Camera viewer)
        {
            if(start>=results.Count)return;
            var canvas=surface?surface.GetComponent<Canvas>():null;if(canvas&&canvas.renderMode!=RenderMode.WorldSpace)return;
            Vector3 origin;
            if(data is TrackedDeviceEventData tracked&&tracked.rayPoints!=null&&tracked.rayPoints.Count>0)origin=tracked.rayPoints[0];
            else if(viewer)origin=viewer.ScreenPointToRay(data.position).origin;
            else return;
            for(int i=results.Count-1;i>=start;i--)
            {
                var point=results[i].worldPosition;
                if(IsBlocked(origin,point,surface,viewer))results.RemoveAt(i);
            }
        }
        public static bool IsBlocked(Vector3 origin,Vector3 destination,Transform surface,Camera viewer)
        {
            return Vector3.Distance(origin,destination)*(1-ClearFraction(origin,destination,surface,viewer))>.055f;
        }
    }
}
