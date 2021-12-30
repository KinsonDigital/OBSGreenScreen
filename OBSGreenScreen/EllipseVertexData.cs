using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct EllipseVertexData
    {
        public Vector2 VertexPos; // a_quadVert

        public Vector2 ShapePosition; // a_shape.x and a_shape.y

        public Vector2 Radius; // a_shape.z and a_shape.w

        public Color Color;  // a_color

        public bool IsFilled; // a_isFilled

        public float BorderThickness; // a_borderThickness

        private static uint TotalElements() => 12u;

        public static uint GetTotalBytes() => TotalElements() * sizeof(float);

        public float[] ToArray()
        {
            // NOTE: The order of the array elements are extremely important.
            // They determine the layout of each stride of vertex data and the layout
            // here has to match the layout told to OpenGL using the VertexAttribLocation() calls

            var result = new List<float>();

            result.AddRange(VertexPos.ToArray());
            result.AddRange(ShapePosition.ToArray());
            result.AddRange(Radius.ToArray());
            result.AddRange(Color.ToArray());

            result.Add(IsFilled ? 1f : 0f);
            result.Add(BorderThickness);

            return result.ToArray();
        }
    }
}
