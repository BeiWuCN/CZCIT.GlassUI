using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CZCIT.GlassUI
{
    public static class GlassTypography
    {
        static Font regular, heading, data;
        sealed class Binding
        {
            public Text text;
            public string value;
            public int size;
            public Font font;
        }
        static readonly Dictionary<Text, Binding> bindings = new Dictionary<Text, Binding>();
        static readonly List<Binding> tracked = new List<Binding>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            bindings.Clear(); tracked.Clear();
            regular = heading = data = null;
        }
        public static Font Regular => regular ? regular : regular = Resources.Load<Font>("Fonts/LunarTerminal-Regular");
        public static Font Heading => heading ? heading : heading = Resources.Load<Font>("Fonts/LunarTerminal-Heading");
        public static Font Data => data ? data : data = Resources.Load<Font>("Fonts/IBMPlexMono-Regular");

        public static Font For(Text text)
        {
            bool hasDigit = false, inTag = false;
            var mono = Data;
            foreach (char character in text.text)
            {
                if (text.supportRichText && character == '<') { inTag = true; continue; }
                if (inTag) { if (character == '>') inTag = false; continue; }
                if (char.IsDigit(character)) hasDigit = true;
                // Unity can report CJK glyphs through an OS fallback even for a Latin-only
                // font. Mixed Chinese status must always use our embedded composite face.
                bool dataCharacter = character <= 127 || character == '°' || character == '−' ||
                    character == '±' || character == '×' || character == '·' || character == 'µ';
                if (!char.IsWhiteSpace(character) && !dataCharacter)
                    return text.fontSize >= 27 ? Heading : Regular;
                if (!char.IsWhiteSpace(character) && (!mono || !mono.HasCharacter(character)))
                    return text.fontSize >= 27 ? Heading : Regular;
            }
            if (hasDigit) return mono;
            return text.fontSize >= 27 ? Heading : Regular;
        }

        public static void Apply(Text text)
        {
            if (!text) return;
            if (!Application.isPlaying)
            {
                var selected = For(text);
                if (selected && text.font != selected) text.font = selected;
                return;
            }
            if (!bindings.TryGetValue(text, out var binding))
            {
                binding = new Binding { text = text };
                bindings.Add(text, binding);
                tracked.Add(binding);
            }
            Update(binding);
        }

        static void Update(Binding binding)
        {
            var text = binding.text;
            if (binding.value == text.text && binding.size == text.fontSize && binding.font == text.font) return;
            var selected = For(text);
            // Preserve font size, rich text, color, raycasting and custom overlay materials.
            if (selected && text.font != selected) text.font = selected;
            binding.value = text.text;
            binding.size = text.fontSize;
            binding.font = text.font;
        }

        public static void RefreshDynamic()
        {
            // Most labels are unchanged; only updated content enters the glyph-selection path.
            for (int i = tracked.Count - 1; i >= 0; i--)
            {
                var binding = tracked[i];
                if (!binding.text)
                {
                    bindings.Remove(binding.text);
                    tracked.RemoveAt(i);
                }
                else Update(binding);
            }
        }
    }
}
