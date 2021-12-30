using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    [GPUBufferName("Line")]
    [SpriteBatchSize(Game.BatchSize)]
    public class LineGPUBuffer : GPUBufferBase<Line>
    {
        private LineData[] _lineData;

        public SizeF ViewPortSize { get; set; }

        protected override void SetupVAO()
        {
            unsafe
            {
                var stride = LineVertexData.GetTotalBytes();

                // TODO: Send color in pixel units and create code to convert to NDC units in shader

                // Vertex Pos
                var vertexPosSize = 2u * sizeof(float);
                GLInvoker.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
                GLInvoker.EnableVertexAttribArray(0);

                // Color
                var colorOffset = vertexPosSize;
                GLInvoker.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)colorOffset);
                GLInvoker.EnableVertexAttribArray(1);
            }
        }

        protected override uint[] GenerateIndices()
        {
            var result = new List<uint>();

            for (var i = 0u; i < BatchSize; i++)
            {
                var maxIndex = result.Count <= 0 ? 0 : result.Max() + 1;

                result.AddRange(new []
                {
                    maxIndex,
                    maxIndex + 1u,
                    maxIndex + 2u,
                    maxIndex + 2u,
                    maxIndex + 1u,
                    maxIndex + 3u,
                });
            }

            return result.ToArray();
        }

        protected override void UpdateVertexData(Line line, uint batchIndex)
        {
            GLInvoker.BeginGroup($"Update Line {batchIndex} Vertex Data");

            var lineData = _lineData[batchIndex];

            var halfLineThickness = line.LineThickness / 2f;

            Line SubLength(Line line, float desiredLength)
            {
                var currentLength = line.GetLength();
                var lengthToSubtract = currentLength - desiredLength;

                var result = default(Line);
                result.Start = line.Start;
                result.Stop = new Vector2(
                    line.Stop.X - (line.Stop.X - line.Start.X) / currentLength * lengthToSubtract,
                    line.Stop.Y - (line.Stop.Y - line.Start.Y) / currentLength * lengthToSubtract
                );

                return result;
            }

            var topLeftLineStop = line.Stop.RotateAround(line.Start, 90, false);
            var topLeftLine = default(Line);
            topLeftLine.Start = line.Start;
            topLeftLine.Stop = topLeftLineStop;
            topLeftLine = SubLength(topLeftLine, halfLineThickness);

            var bottomLeftLineStop = line.Stop.RotateAround(line.Start, 90, true);
            var bottomLeftLine = default(Line);
            bottomLeftLine.Start = line.Start;
            bottomLeftLine.Stop = bottomLeftLineStop;
            bottomLeftLine = SubLength(bottomLeftLine, halfLineThickness);

            var bottomRightLineStop = line.Start.RotateAround(line.Stop, 90, false);
            var bottomRightLine = default(Line);
            bottomRightLine.Start = line.Stop;
            bottomRightLine.Stop = bottomRightLineStop;
            bottomRightLine = SubLength(bottomRightLine, halfLineThickness);

            var topRightLineStop = line.Start.RotateAround(line.Stop, 90, true);
            var topRightLine = default(Line);
            topRightLine.Start = line.Stop;
            topRightLine.Stop = topRightLineStop;
            topRightLine = SubLength(topRightLine, halfLineThickness);

            lineData.Vertex1.VertexPos = topLeftLine.Stop.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            lineData.Vertex2.VertexPos = bottomLeftLine.Stop.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            lineData.Vertex3.VertexPos = topRightLine.Stop.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            lineData.Vertex4.VertexPos = bottomRightLine.Stop.ToNDC(ViewPortSize.Width, ViewPortSize.Height);

            lineData = ProcessColor(lineData, line);

            var totalBytes = lineData.GetTotalBytes();
            var data = lineData.ToArray();
            var offset = totalBytes * batchIndex;

            BindVBO();

            GLInvoker.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset, totalBytes, data);

            UnbindVBO();

            GLInvoker.EndGroup();
        }

        public void OnResize(Vector2D<int> size)
        {
            ViewPortSize = new SizeF(size.X <= 0f ? 1f : size.X, size.Y <= 0f ? 1f : size.Y);
        }

        private LineData ProcessColor(LineData lineData, Line line)
        {
            if (line.ApplyGradient is false)
            {
                lineData.Vertex1.Color = line.Color;
                lineData.Vertex2.Color = line.Color;
                lineData.Vertex3.Color = line.Color;
                lineData.Vertex4.Color = line.Color;
                return lineData;
            }

            switch (line.GradientType)
            {
                case Gradient.Horizontal:
                    lineData.Vertex1.Color = line.GradientStart;
                    lineData.Vertex2.Color = line.GradientStop;
                    lineData.Vertex3.Color = line.GradientStart;
                    lineData.Vertex4.Color = line.GradientStop;
                    break;
                case Gradient.Vertical:
                    lineData.Vertex1.Color = line.GradientStart; // BOTTOM LEFT
                    lineData.Vertex2.Color = line.GradientStart; // BOTTOM RIGHT
                    lineData.Vertex3.Color = line.GradientStop; // TOP RIGHT
                    lineData.Vertex4.Color = line.GradientStop; // BOTTOM RIGHT
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return lineData;
        }

        protected override void PrepareForUse()
        {
            BindVAO();
        }

        protected override float[] GenerateData()
        {
            var result = new List<LineData>();

            for (var i = 0u; i < BatchSize; i++)
            {
                result.AddRange(new []
                {
                    new LineData
                    {
                        Vertex1 = default, // TOP LEFT
                        Vertex2 = default, // BOTTOM LEFT
                        Vertex3 = default, // TOP RIGHT
                        Vertex4 = default, // BOTTOM RIGHT
                    }
                });
            }

            _lineData = result.ToArray();

            return result.ToVertexArray();
        }
    }
}
