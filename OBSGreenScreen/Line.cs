using System;
using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct Line
    {
        private Color _color;

        public Vector2 Start { get; set; }

        public Vector2 Stop { get; set; }

        public Color Color
        {
            get => _color;
            set
            {
                // This is to make sure that no color is rendered to the screen
                // if the start and stop are the same value.  This is due to the math
                // performed in the shader that results in the fragment/pixel ALWAYS
                // being contained in the line rectangle.  This check is also being
                // performed and mitigated in the fragment shader.
                if (Start == Stop)
                {
                    _color = Color.Transparent;
                }
                else
                {
                    _color = value;
                }
            }
        }

        public uint LineThickness { get; set; }

        public bool ApplyGradient { get; set; }

        public Gradient GradientType { get; set; }

        public Color GradientStart { get; set; }

        public Color GradientStop { get; set; }

        public float GetLength()
        {
            return (float)Math.Sqrt(Math.Pow(Stop.X - Start.X, 2f) + Math.Pow(Stop.Y - Start.Y, 2f));
        }

        public bool IsEmpty()
        {
            return Start == Vector2.Zero &&
                Stop == Vector2.Zero &&
                Color == Color.Empty &&
                ApplyGradient == false &&
                GradientType == Gradient.Horizontal &&
                GradientStart.IsEmpty &&
                GradientStop.IsEmpty;
        }

        public void Empty()
        {
            _color = Color.Empty;
            Start = Vector2.Zero;
            Stop = Vector2.Zero;
            LineThickness = 0u;
            ApplyGradient = false;
            GradientType = Gradient.Horizontal;
            GradientStart = Color.Empty;
            GradientStop = Color.Empty;
        }
    }
}
