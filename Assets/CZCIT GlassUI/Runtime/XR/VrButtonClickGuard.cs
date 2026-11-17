// 包装原始按钮动作，对 VR 连续触发去抖，同时保留桌面点击行为。
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CZCIT.GlassUI
{
    [RequireComponent(typeof(Button))]
    public sealed class VrButtonClickGuard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        Button button;
        [SerializeField] Button.ButtonClickedEvent original;
        [SerializeField] bool bound;
        Button.ButtonClickedEvent guarded;
        bool trackedPress;
        int releaseFrame = -1;
        float nextClick;
        public Button.ButtonClickedEvent Actions => bound && original != null ? original : GetComponent<Button>().onClick;

        void Awake()
        {
            Bind();
        }

        void OnEnable()
        {
            if (!button)
                Bind();
        }

        void Bind()
        {
            button = GetComponent<Button>();
            // 保存原事件而不复制监听器，使 Inspector 绑定也能经过同一去抖入口。
            if (!bound || original == null)
            {
                original = button.onClick;
                bound = true;
            }
            guarded = new Button.ButtonClickedEvent();
            guarded.AddListener(Click);
            button.onClick = guarded;
        }

        public void OnPointerDown(PointerEventData data)
        {
            trackedPress = data is TrackedDeviceEventData;
            releaseFrame = -1;
        }

        public void OnPointerUp(PointerEventData data)
        {
            releaseFrame = Time.frameCount;
        }

        void Click()
        {
            bool vr = trackedPress && (releaseFrame < 0 || releaseFrame == Time.frameCount);
            // 去抖只作用于追踪设备；使用非缩放时间，暂停菜单中仍能正常点击。
            if (vr && Time.unscaledTime < nextClick)
                return;
            if (vr)
                nextClick = Time.unscaledTime + .22f;
            GetComponent<CZCIT.GlassUI.IGlassClickFeedback>()?.ConfirmClick();
            original.Invoke();
        }

        void OnDisable()
        {
            trackedPress = false;
            releaseFrame = -1;
        }

        void OnDestroy()
        {
            // 仅撤销自己安装的包装，避免覆盖其他组件后来替换的事件。
            if (button && button.onClick == guarded)
                button.onClick = original;
        }

        public static void Install(Canvas canvas)
        {
            if (!Application.isPlaying)
                return;
            foreach (var button in canvas.GetComponentsInChildren<Button>(true))
                if (!button.GetComponent<VrButtonClickGuard>())
                    button.gameObject.AddComponent<VrButtonClickGuard>();
        }
    }
}
