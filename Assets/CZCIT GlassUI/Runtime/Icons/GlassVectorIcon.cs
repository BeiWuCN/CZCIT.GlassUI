using UnityEngine;
using UnityEngine.UI;
namespace CZCIT.GlassUI {
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GlassVectorIcon : MaskableGraphic {
        public string kind;
        protected override void OnPopulateMesh(VertexHelper mesh) { Draw(mesh,kind,rectTransform.rect.center,Mathf.Min(rectTransform.rect.width,rectTransform.rect.height),color); }
        public static bool Draw(VertexHelper mesh,string kind,Vector2 center,float size,Color tint) {
            int index=System.Array.IndexOf(GlassVectorIconData.Names,kind);
            if(index<0)return false;
            Draw(mesh,index,center,size,tint);return true;
        }
        public static void Draw(VertexHelper mesh,int index,Vector2 center,float size,Color tint) {
            mesh.Clear();
            float scale=size/24f;
            float radius=.82f*scale;
            foreach(var path in GlassVectorIconData.Paths[Mathf.Clamp(index,0,GlassVectorIconData.Paths.Length-1)])
            {
                for(int i=0;i<path.Length;i++)
                {
                    Vector2 p=path[i]*scale+center;
                    if(i>0)
                    {
                        Vector2 previous=path[i-1]*scale+center;
                        Vector2 n=new Vector2(-(p-previous).y,(p-previous).x).normalized*radius;
                        int start=mesh.currentVertCount;
                        mesh.AddVert(previous-n,tint,Vector2.zero);mesh.AddVert(previous+n,tint,Vector2.zero);
                        mesh.AddVert(p+n,tint,Vector2.zero);mesh.AddVert(p-n,tint,Vector2.zero);
                        mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
                    }
                    int disc=mesh.currentVertCount;mesh.AddVert(p,tint,Vector2.zero);
                    const int sides=12;
                    for(int j=0;j<sides;j++)
                    {
                        float angle=j*Mathf.PI*2/sides;
                        mesh.AddVert(p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint,Vector2.zero);
                    }
                    for(int j=0;j<sides;j++)mesh.AddTriangle(disc,disc+1+j,disc+1+(j+1)%sides);
                }
            }
        }
    }
}
