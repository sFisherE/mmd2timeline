using UnityEngine;
using UnityEngine.UI;

namespace mmd2timeline
{
    partial class Mmd2TimelinePersonAtomPlugin
    {
        Text CreateLabel(string v, bool rightSide, Color color,bool bold=true)
        {
            var header = CreateSpacer(rightSide);
            if (header)
            {
                header.height = 40;
                var text = header.gameObject.AddComponent<Text>();
                text.text = v;
                text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                text.fontSize = 30;
                text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                text.color = color;
                return text;
            }
            return null;
        }

        void CreateHeader(string v, bool rightSide, Color color,bool bold=true)
        {
            var header = CreateSpacer(rightSide);
            if (header != null)
            {
                header.height = 40;
                var text = header.gameObject.AddComponent<Text>();
                text.text = v;
                text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                text.fontSize = 30;
                text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
                text.color = color;
            }
        }

    }
}
