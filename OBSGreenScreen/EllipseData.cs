using System.Collections.Generic;

namespace OBSGreenScreen
{
    public struct EllipseData
    {
        public EllipseVertexData Vertex1;

        public EllipseVertexData Vertex2;

        public EllipseVertexData Vertex3;

        public EllipseVertexData Vertex4;

        public uint GetTotalBytes()
        {
            return EllipseVertexData.GetTotalBytes() +
                   EllipseVertexData.GetTotalBytes() +
                   EllipseVertexData.GetTotalBytes() +
                   EllipseVertexData.GetTotalBytes();
        }

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
