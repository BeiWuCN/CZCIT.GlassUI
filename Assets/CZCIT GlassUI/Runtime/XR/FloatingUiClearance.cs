using UnityEngine;
using UnityEngine.UI;
namespace CZCIT.GlassUI
{
    // Only opt-in floating panels move. Equipment screens keep their authored anchors.
    [DefaultExecutionOrder(12000)]
    [RequireComponent(typeof(RectTransform))]
    public sealed class FloatingUiClearance : MonoBehaviour
    {
        static readonly float[] angles={0,-20,20,-40,40,-60,60};
        readonly Vector3[] corners=new Vector3[4];
        RectTransform rect;Vector3 requestedPosition,requestedScale,lastPosition,lastScale;Quaternion requestedRotation,lastRotation;
        bool initialized;float nextCheck;
        Vector2 lastSize;Vector3 lastViewerPosition;Quaternion lastViewerRotation;
        public int AvoidanceCount {get;private set;}
        public static FloatingUiClearance Protect(RectTransform panel)
        {
            return panel.GetComponent<FloatingUiClearance>()??panel.gameObject.AddComponent<FloatingUiClearance>();
        }
        void OnEnable(){initialized=false;nextCheck=0;}
        void LateUpdate(){Refresh(false);}
        public void Refresh(bool force)
        {
            if(!rect)rect=(RectTransform)transform;
            var canvas=GetComponentInParent<Canvas>();if(canvas&&canvas.renderMode!=RenderMode.WorldSpace)return;
            var viewer=canvas&&canvas.worldCamera?canvas.worldCamera:Camera.main;if(!viewer)return;
            bool changed=!initialized||rect.rect.size!=lastSize;
            if(!initialized||Vector3.Distance(transform.position,lastPosition)>.001f){changed|=Vector3.Distance(transform.position,requestedPosition)>.001f;requestedPosition=transform.position;}
            if(!initialized||Quaternion.Angle(transform.rotation,lastRotation)>.05f){changed|=Quaternion.Angle(transform.rotation,requestedRotation)>.05f;requestedRotation=transform.rotation;}
            if(!initialized||Vector3.Distance(transform.localScale,lastScale)>.000001f){changed|=Vector3.Distance(transform.localScale,requestedScale)>.000001f;requestedScale=transform.localScale;}
            initialized=true;
            bool viewerMoved=Vector3.Distance(viewer.transform.position,lastViewerPosition)>.03f||Quaternion.Angle(viewer.transform.rotation,lastViewerRotation)>3;
            if(!force&&!changed&&!viewerMoved&&Time.unscaledTime<nextCheck)
            {
                transform.SetPositionAndRotation(lastPosition,lastRotation);transform.localScale=lastScale;return;
            }
            nextCheck=Time.unscaledTime+.15f;
            var view=viewer.transform.position;
            var controls=GetComponentsInChildren<Selectable>();
            float best=-10,bestFraction=1;Quaternion bestYaw=Quaternion.identity;
            for(int candidate=0;candidate<angles.Length;candidate++)
            {
                var yaw=Quaternion.AngleAxis(angles[candidate],Vector3.up);
                transform.SetPositionAndRotation(view+yaw*(requestedPosition-view),yaw*requestedRotation);transform.localScale=requestedScale;
                rect.GetWorldCorners(corners);float fraction=1;
                // All four edges plus the centre must stay in front of physical geometry.
                for(int i=0;i<4;i++)
                {
                    fraction=Mathf.Min(fraction,WorldUiOcclusion.ClearFraction(view,corners[i],transform,viewer));
                    fraction=Mathf.Min(fraction,WorldUiOcclusion.ClearFraction(view,(corners[i]+corners[(i+1)%4])*.5f,transform,viewer));
                }
                fraction=Mathf.Min(fraction,WorldUiOcclusion.ClearFraction(view,(corners[0]+corners[2])*.5f,transform,viewer));
                for(int x=1;x<4;x++)for(int y=1;y<4;y++)
                {
                    var left=Vector3.Lerp(corners[0],corners[1],y*.25f);var right=Vector3.Lerp(corners[3],corners[2],y*.25f);
                    fraction=Mathf.Min(fraction,WorldUiOcclusion.ClearFraction(view,Vector3.Lerp(left,right,x*.25f),transform,viewer));
                }
                foreach(var control in controls)if(control&&control.IsActive())
                    fraction=Mathf.Min(fraction,WorldUiOcclusion.ClearFraction(view,((RectTransform)control.transform).TransformPoint(((RectTransform)control.transform).rect.center),transform,viewer));
                float score=fraction-Mathf.Abs(angles[candidate])*.003f;
                if(score>best){best=score;bestFraction=fraction;bestYaw=yaw;}
                if(candidate==0&&fraction>.995f)break;
            }
            // Uniform scaling with distance preserves the panel's apparent size and hit layout.
            float ratio=Mathf.Clamp(bestFraction,.025f,1);
            transform.SetPositionAndRotation(view+bestYaw*(requestedPosition-view)*ratio,bestYaw*requestedRotation);
            transform.localScale=requestedScale*ratio;
            if(ratio<.995f||Quaternion.Angle(bestYaw,Quaternion.identity)>.1f)AvoidanceCount++;
            lastPosition=transform.position;lastRotation=transform.rotation;lastScale=transform.localScale;
            lastSize=rect.rect.size;lastViewerPosition=viewer.transform.position;lastViewerRotation=viewer.transform.rotation;
        }
    }
}
