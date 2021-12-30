using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct Rectangle
    {
        private float _topLeftCornerRadius;
        private float _width;
        private float _height;
        private float _borderThickness;
        private float _bottomLeftCornerRadius;
        private float _topRightCornerRadius;
        private float _bottomRightCornerRadius;

        public Vector2 Position { get; set; }

        public float Width
        {
            get => _width;
            set
            {
                value = value < 0f ? 0f : value;

                _width = value;
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                value = value < 0f ? 0f : value;

                _height = value;
            }
        }

        public Color Color { get; set; }

        public bool IsFilled { get; set; }

        public float BorderThickness
        {
            get => _borderThickness;
            set
            {
                var largestValueAllowed = (Width <= Height ? Width : Height) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 1f ? 1f : value;

                _borderThickness = value;
            }
        }

        public float TopLeftCornerRadius
        {
            get => _topLeftCornerRadius;
            set
            {
                var largestValueAllowed = (Width <= Height ? Width : Height) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 0f ? 0f : value;

                _topLeftCornerRadius = value;
            }
        }

        public float BottomLeftCornerRadius
        {
            get => _bottomLeftCornerRadius;
            set
            {
                var largestValueAllowed = (Width <= Height ? Width : Height) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 0f ? 0f : value;

                _bottomLeftCornerRadius = value;
            }
        }

        public float BottomRightCornerRadius
        {
            get => _bottomRightCornerRadius;
            set
            {
                var largestValueAllowed = (Width <= Height ? Width : Height) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 0f ? 0f : value;

                _bottomRightCornerRadius = value;
            }
        }

        public float TopRightCornerRadius
        {
            get => _topRightCornerRadius;
            set
            {
                var largestValueAllowed = (Width <= Height ? Width : Height) / 2f;

                value = value > largestValueAllowed ? largestValueAllowed : value;
                value = value < 0f ? 0f : value;

                _topRightCornerRadius = value;
            }
        }

        public bool ApplyGradient { get; set; }

        public Gradient GradientType { get; set; }

        public Color GradientStart { get; set; }

        public Color GradientStop { get; set; }

        public bool IsEmpty()
        {
            return Position == Vector2.Zero &&
                   Width == 0 &&
                   Height == 0 &&
                   Color.IsEmpty &&
                   IsFilled is false &&
                   BorderThickness == 0f &&
                   TopLeftCornerRadius == 0f &&
                   ApplyGradient == false &&
                   GradientType == Gradient.Horizontal &&
                   GradientStart.IsEmpty &&
                   GradientStop.IsEmpty;
        }

        public void Empty()
        {
            Position = Vector2.Zero;
            Width = 0;
            Height = 0;
            Color = Color.Empty;
            IsFilled = false;
            BorderThickness = 0u;
            TopLeftCornerRadius = 0f;
            ApplyGradient = false;
            GradientType = Gradient.Horizontal;
            GradientStart = Color.Empty;
            GradientStop = Color.Empty;
        }
    }
}
