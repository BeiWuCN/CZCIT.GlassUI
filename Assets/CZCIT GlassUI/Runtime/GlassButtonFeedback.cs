// 汇总按钮悬停、按下和选中状态，统一驱动视觉与确认反馈。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    [RequireComponent(typeof(Image), typeof(Button))]
    public class GlassButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, CZCIT.GlassUI.IGlassClickFeedback
    {
        Material material;
        CZCIT.GlassUI.IGlassHoverReceiver menu;
        int hovering;
        bool pressed;
        float value;
        float pressValue;
        float selectionValue;
        Button button;
        float hoverEntered;
        bool hoverSoundPlayed;
        bool selected;
        bool selectionButton;
        float flashUntil;
        float appliedHighlight = float.NaN;
        float appliedPress = float.NaN;
        float appliedSelection = float.NaN;
        float appliedPulse = float.NaN;
        public bool IsSelected => selected;

        void Awake()
        {
            var image = GetComponent<Image>();
            material = GlassSurfaceMaterial.For(image);
            button = GetComponent<Button>();
            foreach (var component in GetComponentsInParent<MonoBehaviour>(true))
                if (component is CZCIT.GlassUI.IGlassHoverReceiver receiver)
                {
                    menu = receiver;
                    break;
                }
        }

        void Update()
        {
            bool enabled = button && button.IsInteractable();
            if (enabled && hovering > 0 && !hoverSoundPlayed && Time.unscaledTime - hoverEntered >= .055f)
            {
                hoverSoundPlayed = true;
                CZCIT.GlassUI.GlassUiAudio.Play(CZCIT.GlassUI.GlassUiCue.Hover);
            }
            float target = !enabled ? 0 : pressed ? .24f : hovering > 0 ? .18f : 0;
            float response = 1 - Mathf.Exp(-Time.unscaledDeltaTime * (target > value ? 48 : 22));
            value = Mathf.Lerp(value, target, response);
            pressValue = Mathf.Lerp(pressValue, enabled && pressed ? 1 : 0, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 36));
            selectionValue = Mathf.Lerp(selectionValue, selected ? 1 : 0, response);
            if (Mathf.Abs(value - target) < .0001f)
                value = target;
            if (Mathf.Abs(pressValue - (enabled && pressed ? 1 : 0)) < .0001f)
                pressValue = enabled && pressed ? 1 : 0;
            if (Mathf.Abs(selectionValue - (selected ? 1 : 0)) < .0001f)
                selectionValue = selected ? 1 : 0;
            Write("_InteractionHighlight", value, ref appliedHighlight);
            Write("_Pressed", pressValue, ref appliedPress);
            Write("_Selected", selectionValue, ref appliedSelection);
            Write("_ConfirmPulse", Mathf.Clamp01((flashUntil - Time.unscaledTime) / .18f), ref appliedPulse);
        }

        void Write(string property, float next, ref float previous)
        {
            if (next == previous)
                return;
            material.SetFloat(property, next);
            previous = next;
        }

        public void SetSelected(bool state)
        {
            selectionButton = true;
            selected = state;
        }

        public void ConfirmClick()
        {
            flashUntil = Time.unscaledTime + .18f;
            CZCIT.GlassUI.GlassUiAudio.Play(selectionButton ? CZCIT.GlassUI.GlassUiCue.Select : CZCIT.GlassUI.GlassUiCue.Confirm);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (hovering == 0)
            {
                hoverEntered = Time.unscaledTime;
                hoverSoundPlayed = false;
            }
            hovering++;
            menu?.SetButtonHover(true);
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (hovering > 0)
            {
                hovering--;
                menu?.SetButtonHover(false);
            }
            if (hovering == 0)
                hoverSoundPlayed = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData e)
        {
            pressed = true;
        }

        public void OnPointerUp(PointerEventData e)
        {
            pressed = false;
        }

        void OnDisable()
        {
            for (int i = 0; i < hovering; i++)
                menu?.SetButtonHover(false);
            hovering = 0;
            hoverSoundPlayed = false;
            pressed = false;
            flashUntil = 0;
            value = pressValue = 0;
            if (material)
            {
                Write("_InteractionHighlight", 0, ref appliedHighlight);
                Write("_Pressed", 0, ref appliedPress);
                Write("_ConfirmPulse", 0, ref appliedPulse);
            }
        }
    }
}
