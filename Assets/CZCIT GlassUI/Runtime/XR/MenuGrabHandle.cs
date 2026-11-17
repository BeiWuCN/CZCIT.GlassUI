// 将指针或抓取事件转给菜单宿主，统一管理拖动开始、结束和悬停。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CZCIT.GlassUI
{
    public class MenuGrabHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        CZCIT.GlassUI.IGlassDragTarget menu;
        Transform controller;
        Vector3 initialRay;
        Vector3 initialOffset;
        Vector3 smoothedDirection;
        bool hovered;
        bool mouseDragging;
        Camera mouseCamera;
        bool selected;
        XRBaseInteractable grab;
        Transform hoveredController;
        XRNode hoveredNode;
        bool lastGrip,gripCaptured;
        Image image;

        void Awake()
        {
            BindMenu();
            var bar = transform.Find("WhiteGrabBar");
            image = bar ? bar.GetComponent<Image>() : null;
            grab = GetComponent<XRBaseInteractable>();
            if(Application.isPlaying && grab is XRGrabInteractable detachable)
            {
                detachable.enabled=false;
                var fixedHandle=gameObject.AddComponent<XRSimpleInteractable>();
                fixedHandle.interactionLayers=detachable.interactionLayers;
                grab=fixedHandle;
                Destroy(detachable);
            }
            if (grab)
            {
                grab.selectEntered.AddListener(Grab);
                grab.selectExited.AddListener(Release);
            }
        }

        void BindMenu(){
            foreach (var component in GetComponentsInParent<MonoBehaviour>(true))
                if (component is CZCIT.GlassUI.IGlassDragTarget target)
                {
                    menu = target;
                    break;
                }
        }

        void Start(){BindMenu();var bar=transform.Find("WhiteGrabBar");if(bar)image=bar.GetComponent<Image>();}

        void Grab(SelectEnterEventArgs args)
        {
            selected = true;
            var interactor=args.interactorObject as XRRayInteractor;
            Begin(interactor && interactor.rayOriginTransform ? interactor.rayOriginTransform : args.interactorObject.transform);
        }

        void Release(SelectExitEventArgs args)
        {
            selected = false;
            if(!gripCaptured)End();
        }

        Vector3 AimDirection()
        {
            var camera = Camera.main;
            return (controller.position + controller.forward * menu.FollowDistance - camera.transform.position).normalized;
        }

        void Begin(Transform hand)
        {
            if(menu==null)BindMenu();
            if (menu == null || controller || !hand || !Camera.main)
                return;
            controller = hand;
            initialRay = AimDirection();
            initialOffset = (menu.PanelTransform.position - Camera.main.transform.position).normalized;
            smoothedDirection = initialOffset;
            menu.BeginHandleDrag();
        }

        void End()
        {
            controller = null;mouseDragging=false;
            menu?.EndHandleDrag();
        }

        public bool GripDragging => gripCaptured && controller;
        public void ProcessGrip(bool grip)
        {
            if(hovered&&grip&&!lastGrip&&!controller&&hoveredController){gripCaptured=true;Begin(hoveredController);}
            if(gripCaptured&&!grip){gripCaptured=false;if(!selected)End();}
            lastGrip=grip;
        }

        void Update()
        {
            if(hoveredController || gripCaptured)
            {
                var device=InputDevices.GetDeviceAtXRNode(hoveredNode);
                device.TryGetFeatureValue(CommonUsages.gripButton,out bool button);
                device.TryGetFeatureValue(CommonUsages.grip,out float analog);
                bool grip=device.isValid&&(button||analog>(lastGrip ? .4f : .65f));
                ProcessGrip(grip);
            }
            RefreshDrag(Time.unscaledDeltaTime);
            if (image)
                image.color = Color.Lerp(
                    image.color,
                    (controller||mouseDragging) ? new Color(.35f, .85f, 1) : hovered ? new Color(.7f, .95f, 1) : Color.white,
                    1 - Mathf.Exp(-14 * Time.unscaledDeltaTime));
        }

        public void RefreshDrag(float deltaTime)
        {
            if (controller && Camera.main)
            {
                var target = Quaternion.FromToRotation(initialRay, AimDirection()) * initialOffset;
                smoothedDirection = Vector3.Slerp(smoothedDirection, target, 1 - Mathf.Exp(-18 * deltaTime));
                menu.PositionAroundViewer(smoothedDirection);
            }
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (e is TrackedDeviceEventData xr && xr.interactor is Component hand)
                Begin(hand is XRRayInteractor ray&&ray.rayOriginTransform?ray.rayOriginTransform:hand.transform);
            else if(e.button==PointerEventData.InputButton.Left)
            {
                if(menu==null)BindMenu();mouseCamera=e.pressEventCamera?e.pressEventCamera:Camera.main;if(menu==null||!mouseCamera)return;
                menu.BeginHandleDrag();mouseDragging=true;initialRay=mouseCamera.ScreenPointToRay(e.position).direction;initialOffset=(menu.PanelTransform.position-Camera.main.transform.position).normalized;
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!selected && !gripCaptured)
                End();
        }

        public void OnDrag(PointerEventData e)
        {
            if(mouseDragging&&mouseCamera&&menu!=null)menu.PositionAroundViewer(Quaternion.FromToRotation(initialRay,mouseCamera.ScreenPointToRay(e.position).direction)*initialOffset);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if(e is TrackedDeviceEventData data && data.interactor is Component component)
            {
                var ray=component as XRRayInteractor;
                hoveredController=ray&&ray.rayOriginTransform?ray.rayOriginTransform:component.transform;
                hoveredNode=XRNode.RightHand;
                for(var t=component.transform;t;t=t.parent)if(t.name.IndexOf("Left",System.StringComparison.OrdinalIgnoreCase)>=0){hoveredNode=XRNode.LeftHand;break;}
            }
            if (!hovered)
            {
                hovered = true;
                menu?.SetButtonHover(true);
            }
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (hovered)
            {
                hovered = false;
                menu?.SetButtonHover(false);
            }
        }

        void OnDisable()
        {
            if (hovered)
            {
                hovered = false;
                menu?.SetButtonHover(false);
            }
            selected = false;gripCaptured=false;hoveredController=null;lastGrip=false;
            End();
        }

        void OnDestroy()
        {
            if (grab)
            {
                grab.selectEntered.RemoveListener(Grab);
                grab.selectExited.RemoveListener(Release);
            }
        }
    }
}
