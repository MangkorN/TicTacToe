using UnityEngine;

namespace CoreLib.Utilities
{
    /// <summary>
    /// HorizontalLine attribute.
    /// Attribute is used to draw a horizontal line in the inspector.
    /// </summary>
    public class HorizontalLineAttribute : PropertyAttribute { }

    /// <summary>
    /// HorizontalBoldLine attribute.
    /// Attribute is used to draw a bold horizontal line in the inspector.
    /// </summary>
    public class HorizontalLineBoldAttribute : PropertyAttribute
    {
        public Color LineColor;

        public HorizontalLineBoldAttribute(float r = 0.5f, float g = 0.5f, float b = 0.5f, float a = 1)
        {
            this.LineColor = new Color(r, g, b, a);
        }
    }
}