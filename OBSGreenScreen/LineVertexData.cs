using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace OBSGreenScreen
{
    public struct LineVertexData
    {
        // TODO: Add comments for each field to show what shader attribute they map to
        public Vector2 VertexPos; // a_vertexPos

        public Color Color; // a_color

        private static uint TotalElements() => 6;

        public static uint GetTotalBytes() => TotalElements() * sizeof(float);

        public IEnumerable<float> ToArray()
        {
            var result = new List<float>();

            result.AddRange(VertexPos.ToArray());
            result.AddRange(Color.ToArray());

            return result.ToArray();
        }
    }
}
