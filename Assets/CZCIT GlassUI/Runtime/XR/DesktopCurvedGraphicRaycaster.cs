using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CZCIT.GlassUI {
// Desktop preview renders exactly the VR curvature and projects the mouse onto it.
public sealed class DesktopCurvedGraphicRaycaster:GraphicRaycaster {
public float radius=1400;
public override void Raycast(PointerEventData data,List<RaycastResult> results){var camera=eventCamera;if(!camera){base.Raycast(data,results);return;}var original=data.position;var ray=camera.ScreenPointToRay(original);if(!CurvedPanelProjection.TryProject(transform,radius,ray,out var flat))return;int start=results.Count;data.position=camera.WorldToScreenPoint(flat);try{base.Raycast(data,results);}finally{data.position=original;}for(int i=start;i<results.Count;i++){var hit=results[i];hit.worldPosition=transform.TransformPoint(CurvedGraphicMesh.Bend(transform.InverseTransformPoint(hit.worldPosition),radius));hit.distance=Vector3.Distance(ray.origin,hit.worldPosition);hit.screenPosition=original;results[i]=hit;}CZCIT.GlassUI.WorldUiOcclusion.Filter(data,results,start,transform,camera);}
}}
