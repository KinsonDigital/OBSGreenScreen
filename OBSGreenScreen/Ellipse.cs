using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct Ellipse
    {
        private float _radiusX;
        private float _radiusY;
        private float _borderThickness;

        public Vector2 Position { get; set; }

        public float RadiusX
        {
            get => _radiusX;
            set
            {
                value = value < 0f ? 0f : value;

                _radiusX = value;
            }
        }

        public float RadiusY
        {
            get => _radiusY;
            set
            {
                value = value < 0f ? 0f : value;

                _radiusY = value;
            }
        }

        public Color Color { get; set; }

        public bool IsFilled { get; set; }

        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                var largestValueAllowed = (RadiusX <= RadiusY ? RadiusX : RadiusY) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 1f ? 1f : value;

                _borderThickness = value;
            }
        }

        public bool ApplyGradient { get; set; }

        public Gradient GradientType { get; set; }

        public Color GradientStart { get; set; }

        public Color GradientStop { get; set; }

        public bool IsEmpty()
        {
            return Position == Vector2.Zero &&
                   RadiusX == 0 &&
                   RadiusY == 0 &&
                   Color.IsEmpty &&
                   IsFilled is false &&
                   BorderThickness == 0f &&
                   ApplyGradient == false &&
                   GradientType == Gradient.Horizontal &&
                   GradientStart.IsEmpty &&
                   GradientStop.IsEmpty;
        }

        public void Empty()
        {
            Position = Vector2.Zero;
            RadiusX = 0;
            RadiusY = 0;
            Color = Color.Empty;
            IsFilled = false;
            BorderThickness = 0u;
            ApplyGradient = false;
            GradientType = Gradient.Horizontal;
            GradientStart = Color.Empty;
            GradientStop = Color.Empty;
        }
    }
}
