// 承接玻璃表面材质的生命周期，销毁时释放组件管理的材质资源。
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    // Reveal and button feedback share one owned material instead of replacing
    // each other's instances or modifying a project material asset.
    [DisallowMultipleComponent]
    public sealed class GlassSurfaceMaterial : MonoBehaviour
    {
        Material owned;
        Image surface;

        public static Material For(Image image)
        {
            var owner = image.GetComponent<GlassSurfaceMaterial>();
            if (!owner)
                owner = image.gameObject.AddComponent<GlassSurfaceMaterial>();
            if (!owner.owned)
            {
                owner.owned = new Material(image.material);
                image.material = owner.owned;
            }
            owner.surface=image;owner.SyncGeometry();return owner.owned;
        }

        void LateUpdate(){SyncGeometry();}
        void SyncGeometry()
        {
            if(!surface||!owned)return;
            if(surface.type!=Image.Type.Filled){surface.sprite=null;surface.overrideSprite=null;surface.type=Image.Type.Simple;}
            var rect=surface.rectTransform.rect;var scale=surface.transform.lossyScale;
            float width=Mathf.Abs(rect.width*scale.x),height=Mathf.Max(.00001f,Mathf.Abs(rect.height*scale.y));
            float aspect=width/height;
            float radius=Mathf.Min(14*Mathf.Min(Mathf.Abs(scale.x),Mathf.Abs(scale.y)),Mathf.Min(width,height)*.5f)/height;
            if(owned.HasProperty("_Aspect")&&!Mathf.Approximately(owned.GetFloat("_Aspect"),aspect))owned.SetFloat("_Aspect",aspect);
            if(owned.HasProperty("_Radius")&&!Mathf.Approximately(owned.GetFloat("_Radius"),radius))owned.SetFloat("_Radius",radius);
        }

        void OnDestroy()
        {
            if (owned)
            {if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);}
        }
    }
}
