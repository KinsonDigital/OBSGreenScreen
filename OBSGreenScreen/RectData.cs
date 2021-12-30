using System.Collections.Generic;

namespace OBSGreenScreen
{
    public struct RectData
    {
        public RectVertexData Vertex1;
        public RectVertexData Vertex2;
        public RectVertexData Vertex3;
        public RectVertexData Vertex4;

        public uint GetTotalBytes() => RectVertexData.Stride() * 4u;

        public float[] ToArray()
        {
            var result = new List<float>();

            result.AddRange(Vertex1.ToArray());
            result.AddRange(Vertex2.ToArray());
            result.AddRange(Vertex3.ToArray());
            result.AddRange(Vertex4.ToArray());

            return result.ToArray();
        }
    }
}
