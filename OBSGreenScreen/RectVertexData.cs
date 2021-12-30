using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct RectVertexData
    {
        public Vector2 VertexPos; // a_quadVert

        public Vector4 Rectangle; // a_shape

        public Color Color;  // a_color

        public bool IsFilled; // a_isFilled

        public float BorderThickness; // a_borderThickness

        public float TopLeftCornerRadius; // a_topLeftCornerRadius

        public float BottomLeftCornerRadius; // a_bottomLeftCornerRadius

        public float BottomRightCornerRadius; // a_bottomRightCornerRadius

        public float TopRightCornerRadius; // a_topRightCornerRadius

        private static uint TotalElements() => 16u;

        // TODO: Use custom attribute to calculate the total bytes.
        // This can also be cached and done once to save CPU cycles.
        public static uint Stride() => TotalElements() * sizeof(float);

        public float[] ToArray()
        {
            // NOTE: The order of the array elements are extremely important.
            // They determine the layout of each stride of vertex data and the layout
            // here has to match the layout told to OpenGL using the VertexAttribLocation() calls

            var result = new List<float>();

            result.AddRange(VertexPos.ToArray());
            result.AddRange(Rectangle.ToArray());
            result.AddRange(Color.ToArray());

            result.Add(IsFilled ? 1f : 0f);
            result.Add(BorderThickness);
            result.Add(TopLeftCornerRadius);
            result.Add(BottomLeftCornerRadius);
            result.Add(BottomRightCornerRadius);
            result.Add(TopRightCornerRadius);

            return result.ToArray();
        }
    }
}
