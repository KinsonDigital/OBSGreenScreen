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
    [SpriteBatchSize(Game.BatchSize)]
    [GPUBufferName("Ellipse")]
    public class EllipseGPUBuffer : GPUBufferBase<Ellipse>
    {
        private EllipseData[] _ellipseData;

        public SizeF ViewPortSize { get; set; }

        protected override void SetupVAO()
        {
            GLInvoker.BeginGroup("Setup Shape Vertex Attributes");

            unsafe
            {
                var stride = EllipseVertexData.GetTotalBytes();

                // Vertex Position
                const uint vertexPosSize = 2u * sizeof(float);
                GLInvoker.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
                GLInvoker.EnableVertexAttribArray(0);

                // Shape
                var ellipseOffset = vertexPosSize;
                const uint ellipseSize = 4u * sizeof(float);
                GLInvoker.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)ellipseOffset);
                GLInvoker.EnableVertexAttribArray(1);

                // Color
                var colorOffset = ellipseOffset + ellipseSize;
                const uint colorSize = 4u * sizeof(float);
                GLInvoker.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)colorOffset);
                GLInvoker.EnableVertexAttribArray(2);

                // IsFilled
                var isFilledOffset = colorOffset + colorSize;
                const uint isFilledSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, (void*)isFilledOffset);
                GLInvoker.EnableVertexAttribArray(3);

                // Border Thickness
                var borderThicknessOffset = isFilledOffset + isFilledSize;
                const uint borderThicknessSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, stride, (void*)borderThicknessOffset);
                GLInvoker.EnableVertexAttribArray(4);

                // Corner Radius
                var radiusOffset = borderThicknessOffset + borderThicknessSize;
                const uint radiusSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, stride, (void*)radiusOffset);
                GLInvoker.EnableVertexAttribArray(5);
            }

            GLInvoker.EndGroup();
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

        protected override void UpdateVertexData(Ellipse ellipse, uint batchIndex)
        {
            GLInvoker.BeginGroup($"Update Shape {batchIndex} Vertex Data");

            var shapeData = _ellipseData[batchIndex];

            var left = ellipse.Position.X - ellipse.RadiusX;
            var bottom = ellipse.Position.Y + ellipse.RadiusY;
            var right = ellipse.Position.X + ellipse.RadiusX;
            var top = ellipse.Position.Y - ellipse.RadiusY;

            var topLeft = new Vector2(left, top);
            var bottomLeft = new Vector2(left, bottom);
            var bottomRight = new Vector2(right, bottom);
            var topRight = new Vector2(right, top);

            shapeData.Vertex1.VertexPos = topLeft.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            shapeData.Vertex2.VertexPos = bottomLeft.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            shapeData.Vertex3.VertexPos = topRight.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            shapeData.Vertex4.VertexPos = bottomRight.ToNDC(ViewPortSize.Width, ViewPortSize.Height);

            shapeData = ProcessColor(shapeData, ellipse);

            shapeData.Vertex1.IsFilled = ellipse.IsFilled;
            shapeData.Vertex2.IsFilled = ellipse.IsFilled;
            shapeData.Vertex3.IsFilled = ellipse.IsFilled;
            shapeData.Vertex4.IsFilled = ellipse.IsFilled;

            shapeData.Vertex1.ShapePosition = ellipse.Position;
            shapeData.Vertex2.ShapePosition = ellipse.Position;
            shapeData.Vertex3.ShapePosition = ellipse.Position;
            shapeData.Vertex4.ShapePosition = ellipse.Position;

            shapeData.Vertex1.Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY);
            shapeData.Vertex2.Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY);
            shapeData.Vertex3.Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY);
            shapeData.Vertex4.Radius = new Vector2(ellipse.RadiusX, ellipse.RadiusY);

            shapeData.Vertex1.BorderThickness = ellipse.BorderThickness;
            shapeData.Vertex2.BorderThickness = ellipse.BorderThickness;
            shapeData.Vertex3.BorderThickness = ellipse.BorderThickness;
            shapeData.Vertex4.BorderThickness = ellipse.BorderThickness;

            var totalBytes = shapeData.GetTotalBytes();
            var data = shapeData.ToArray();
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

        private EllipseData ProcessColor(EllipseData ellipseData, Ellipse ellipse)
        {
            if (ellipse.ApplyGradient is false)
            {
                ellipseData.Vertex1.Color = ellipse.Color;
                ellipseData.Vertex2.Color = ellipse.Color;
                ellipseData.Vertex3.Color = ellipse.Color;
                ellipseData.Vertex4.Color = ellipse.Color;
                return ellipseData;
            }

            switch (ellipse.GradientType)
            {
                case Gradient.Horizontal:
                    ellipseData.Vertex1.Color = ellipse.GradientStart; // BOTTOM LEFT
                    ellipseData.Vertex2.Color = ellipse.GradientStart; // BOTTOM RIGHT
                    ellipseData.Vertex3.Color = ellipse.GradientStop; // TOP RIGHT
                    ellipseData.Vertex4.Color = ellipse.GradientStop; // BOTTOM RIGHT
                    break;
                case Gradient.Vertical:
                    ellipseData.Vertex1.Color = ellipse.GradientStart;
                    ellipseData.Vertex2.Color = ellipse.GradientStop;
                    ellipseData.Vertex3.Color = ellipse.GradientStart;
                    ellipseData.Vertex4.Color = ellipse.GradientStop;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return ellipseData;
        }

        protected override void PrepareForUse()
        {
            BindVAO();
        }

        protected override float[] GenerateData()
        {
            var result = new List<EllipseData>();

            for (var i = 0u; i < BatchSize; i++)
            {
                result.AddRange(new EllipseData[]
                {
                    new ()
                    {
                        Vertex1 = new EllipseVertexData
                        {
                            VertexPos = new Vector2(-1.0f, 1.0f),
                            BorderThickness = 1f,
                        },
                        Vertex2 = new EllipseVertexData
                        {
                            VertexPos = new Vector2(-1.0f, -1.0f),
                            BorderThickness = 1f,
                        },
                        Vertex3 = new EllipseVertexData
                        {
                            VertexPos = new Vector2(1.0f, 1.00f),
                            BorderThickness = 1f,
                        },
                        Vertex4 = new EllipseVertexData
                        {
                            VertexPos = new Vector2(1.0f, -1.0f),
                            BorderThickness = 1f,
                        }
                    }
                });
            }

            _ellipseData = result.ToArray();

            return result.ToVertexArray();
        }
    }
}
