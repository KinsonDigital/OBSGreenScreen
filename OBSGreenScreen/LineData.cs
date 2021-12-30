using System.Collections.Generic;

namespace OBSGreenScreen
{
    public struct LineData
    {
        public LineVertexData Vertex1;

        public LineVertexData Vertex2;

        public LineVertexData Vertex3;

        public LineVertexData Vertex4;

        public uint GetTotalBytes() => LineVertexData.GetTotalBytes() * 4;

        public float[] ToArray()
        {
            var result = new List<float>();
            result.AddRange(Vertex1.ToArray());
            result.AddRange(Vertex2.ToArray());
            result.AddRange(Vertex3.ToArray());
            result.AddRange(Vertex4.ToArray());

            return result.ToArray();
        }

        public override string ToString()
        {
            return $"{Vertex1} <|> {Vertex2} <|> {Vertex3} <|> {Vertex4}";
        }
    }
}
