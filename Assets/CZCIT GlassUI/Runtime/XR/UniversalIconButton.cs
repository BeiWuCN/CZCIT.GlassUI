using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CZCIT.GlassUI;
namespace CZCIT.GlassUI {
public sealed class UniversalUiIcon:MaskableGraphic {
public string kind;
protected override void OnPopulateMesh(VertexHelper v){v.Clear();string vector=kind switch {"back"=>"chevron-left","next"=>"chevron-right","skip"=>"player-skip-forward","close"=>"circle-x","center"=>"focus-centered","source"=>"external-link","switch"=>"arrows-exchange","play"=>"player-play","pause"=>"player-pause","home"=>"home","reset"=>"refresh",_=>null};
if(vector!=null&&GlassVectorIcon.Draw(v,vector,rectTransform.rect.center,28,color))return;
void L(float x,float y,float a,float b){Vector2 p=new Vector2(x,y),q=new Vector2(a,b),n=new Vector2(-(q-p).y,(q-p).x).normalized*1.3f;int i=v.currentVertCount;v.AddVert(p-n,color,Vector2.zero);v.AddVert(p+n,color,Vector2.zero);v.AddVert(q+n,color,Vector2.zero);v.AddVert(q-n,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
void Arrow(float sign){L(-5*sign,10,5*sign,0);L(5*sign,0,-5*sign,-10);}
switch(kind){case "back":Arrow(-1);break;case "next":Arrow(1);break;case "skip":Arrow(1);L(10,-10,10,10);break;case "close":L(-8,-8,8,8);L(-8,8,8,-8);break;case "center":for(int i=0;i<4;i++){float x=i%2==0?-11:11,y=i<2?-11:11;L(x,y,x*.45f,y);L(x,y,x,y*.45f);}L(-4,0,4,0);L(0,-4,0,4);break;case "source":L(-10,-10,7,-10);L(-10,-10,-10,7);L(-10,7,-3,7);L(7,-10,7,-3);L(-2,-2,10,10);L(1,10,10,10);L(10,10,10,1);break;case "switch":L(-12,5,12,5);L(12,5,6,11);L(-12,-5,12,-5);L(-12,-5,-6,-11);break;case "play":L(-6,-10,-6,10);L(-6,10,10,0);L(10,0,-6,-10);break;case "pause":L(-5,-10,-5,10);L(5,-10,5,10);break;case "home":L(-12,0,0,11);L(0,11,12,0);L(-8,1,-8,-10);L(8,1,8,-10);L(-8,-10,8,-10);break;case "motionAll":case "motionReduced":case "motionOff":case "motion":if(kind=="motionOff")L(-11,-11,11,11);if(kind!="motionOff"){L(-9,-14,-6,-14);if(kind=="motionAll"){L(-1,-14,2,-14);L(7,-14,10,-14);}}L(-10,0,0,10);L(0,10,10,0);L(10,0,0,-10);L(0,-10,-10,0);L(-13,9,-8,9);L(-10.5f,6.5f,-10.5f,11.5f);break;case "reset":for(int i=0;i<28;i++){float a=(i*10+35)*Mathf.Deg2Rad,b=((i+1)*10+35)*Mathf.Deg2Rad;L(Mathf.Cos(a)*10,Mathf.Sin(a)*10,Mathf.Cos(b)*10,Mathf.Sin(b)*10);}L(8,8,0,8);L(8,8,8,0);break;}
}}
public sealed class UniversalIconButton:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler {
public bool externalHint;
Text source,tooltip;UniversalUiIcon icon;
static string Kind(string text){switch(text){case "上一步":case "上一页":case "返回正文":return "back";case "进入天宫空间站":case "下一步":case "下一页（预览）":case "下一页":case "继续体验":return "next";case "跳过此步":return "skip";case "居中":case "回正":return "center";case "×":return "close";case "换侧":return "switch";case "动效：全部":return "motionAll";case "动效：少量":return "motionReduced";case "动效：关闭":return "motionOff";case "动效：标准":case "动效：减少":return "motion";case "任务与科学资料":case "科学资料来源":case "图片来源":case "NASA 资料":return "source";case "进入主菜单":return "home";case "播放":return "play";case "暂停":return "pause";case "复位":return "reset";default:return null;}}
public static void Apply(Button button){if(button.GetComponent<UniversalIconButton>())return;var text=button.GetComponentInChildren<Text>();if(!text||Kind(text.text)==null)return;var behavior=button.gameObject.AddComponent<UniversalIconButton>();behavior.source=text;behavior.Build();}
void Build(){source.enabled=false;var go=new GameObject("Action icon",typeof(RectTransform),typeof(CanvasRenderer));go.layer=5;go.transform.SetParent(transform,false);((RectTransform)go.transform).sizeDelta=new Vector2(30,30);icon=go.AddComponent<UniversalUiIcon>();icon.kind=Kind(source.text);icon.material=source.material;icon.raycastTarget=false;icon.color=Color.white;
var tip=new GameObject("Action hint",typeof(RectTransform),typeof(Text));tip.layer=5;tip.transform.SetParent(transform,false);tooltip=tip.GetComponent<Text>();tooltip.rectTransform.sizeDelta=new Vector2(260,32);tooltip.rectTransform.anchoredPosition=new Vector2(0,((RectTransform)transform).rect.height*.5f+20);tooltip.font=GlassTypography.Regular;tooltip.fontSize=17;tooltip.alignment=TextAnchor.MiddleCenter;tooltip.color=Color.white;tooltip.raycastTarget=false;tooltip.gameObject.SetActive(false);
var canvas=GetComponentInParent<Canvas>();var curved=canvas?canvas.GetComponent<CurvedGlassRaycaster>():null;if(curved){foreach(var graphic in new Graphic[]{icon,tooltip}){var effect=graphic.gameObject.AddComponent<CurvedGraphicMesh>();effect.root=canvas.transform;effect.radius=curved.radius;}}}
void LateUpdate(){if(!source||!icon)return;var next=Kind(source.text);if(next==null){source.enabled=true;icon.enabled=false;return;}source.enabled=false;icon.enabled=true;if(icon.kind!=next){icon.kind=next;icon.SetVerticesDirty();}if(tooltip.gameObject.activeSelf)tooltip.text=source.text;
if(GetComponentInParent<IGlassMotionPreference>()!=null)tooltip.rectTransform.anchoredPosition=new Vector2(0,-((RectTransform)transform).rect.height*.5f-22);var button=GetComponent<Button>();icon.color=button.IsInteractable()?Color.white:new Color(1,1,1,.28f);}
public void OnPointerEnter(PointerEventData e){if(externalHint)return;if(tooltip){tooltip.text=source.text;tooltip.gameObject.SetActive(true);}}
public void OnPointerExit(PointerEventData e){if(tooltip)tooltip.gameObject.SetActive(false);}
void OnDisable(){if(tooltip)tooltip.gameObject.SetActive(false);}
}
}
