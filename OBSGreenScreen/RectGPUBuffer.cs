using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    [GPUBufferName("Rectangle")]
    [SpriteBatchSize(Game.BatchSize)]
    public class RectGPUBuffer : GPUBufferBase<Rectangle>
    {
        private RectData[] _rectData;

        protected override void SetupVAO()
        {
            GLInvoker.BeginGroup("Setup Rectangle Vertex Attributes");

            unsafe
            {
                var stride = RectVertexData.Stride();

                // Vertex Position
                const uint vertexPosSize = 2u * sizeof(float);
                GLInvoker.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
                GLInvoker.EnableVertexAttribArray(0);

                // Rectangle
                var rectOffset = vertexPosSize;
                const uint rectSize = 4u * sizeof(float);
                GLInvoker.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)rectOffset);
                GLInvoker.EnableVertexAttribArray(1);

                // Color
                var colorOffset = rectOffset + rectSize;
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

                // Top Left Corner Radius
                var topLeftRadiusOffset = borderThicknessOffset + borderThicknessSize;
                const uint topLeftRadiusSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, stride, (void*)topLeftRadiusOffset);
                GLInvoker.EnableVertexAttribArray(5);

                // Bottom Left Corner Radius
                var bottomLeftRadiusOffset = topLeftRadiusOffset + topLeftRadiusSize;
                const uint bottomLeftRadiusSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(6, 1, VertexAttribPointerType.Float, false, stride, (void*)bottomLeftRadiusOffset);
                GLInvoker.EnableVertexAttribArray(6);

                // Bottom Right Corner Radius
                var bottomRightRadiusOffset = bottomLeftRadiusOffset + bottomLeftRadiusSize;
                const uint bottomRightRadiusSize = 1u * sizeof(float);
                GLInvoker.VertexAttribPointer(7, 1, VertexAttribPointerType.Float, false, stride, (void*)bottomRightRadiusOffset);
                GLInvoker.EnableVertexAttribArray(7);

                // Bottom Left Corner Radius
                var topRightRadiusOffset = bottomRightRadiusOffset + bottomRightRadiusSize;
                GLInvoker.VertexAttribPointer(8, 1, VertexAttribPointerType.Float, false, stride, (void*)topRightRadiusOffset);
                GLInvoker.EnableVertexAttribArray(8);
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

        protected override void UpdateVertexData(Rectangle rect, uint batchIndex)
        {
            GLInvoker.BeginGroup($"Update Rectangle {batchIndex} Vertex Data");

            var rectData = _rectData[batchIndex];
            var halfWidth = rect.Width / 2f;
            var halfHeight = rect.Height / 2f;

            var left = rect.Position.X - halfWidth;
            var bottom = rect.Position.Y + halfHeight;
            var right = rect.Position.X + halfWidth;
            var top = rect.Position.Y - halfHeight;

            var topLeft = new Vector2(left, top);
            var bottomLeft = new Vector2(left, bottom);
            var bottomRight = new Vector2(right, bottom);
            var topRight = new Vector2(right, top);

            rectData.Vertex1.VertexPos = topLeft.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            rectData.Vertex2.VertexPos = bottomLeft.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            rectData.Vertex3.VertexPos = topRight.ToNDC(ViewPortSize.Width, ViewPortSize.Height);
            rectData.Vertex4.VertexPos = bottomRight.ToNDC(ViewPortSize.Width, ViewPortSize.Height);

            rectData.Vertex1.Rectangle = new Vector4(rect.Position.X, rect.Position.Y, rect.Width, rect.Height);
            rectData.Vertex2.Rectangle = new Vector4(rect.Position.X, rect.Position.Y, rect.Width, rect.Height);
            rectData.Vertex3.Rectangle = new Vector4(rect.Position.X, rect.Position.Y, rect.Width, rect.Height);
            rectData.Vertex4.Rectangle = new Vector4(rect.Position.X, rect.Position.Y, rect.Width, rect.Height);

            rectData = ProcessColor(rectData, rect);

            rectData.Vertex1.IsFilled = rect.IsFilled;
            rectData.Vertex2.IsFilled = rect.IsFilled;
            rectData.Vertex3.IsFilled = rect.IsFilled;
            rectData.Vertex4.IsFilled = rect.IsFilled;

            rectData.Vertex1.BorderThickness = rect.BorderThickness;
            rectData.Vertex2.BorderThickness = rect.BorderThickness;
            rectData.Vertex3.BorderThickness = rect.BorderThickness;
            rectData.Vertex4.BorderThickness = rect.BorderThickness;

            rectData.Vertex1.TopLeftCornerRadius = rect.TopLeftCornerRadius;
            rectData.Vertex2.TopLeftCornerRadius = rect.TopLeftCornerRadius;
            rectData.Vertex3.TopLeftCornerRadius = rect.TopLeftCornerRadius;
            rectData.Vertex4.TopLeftCornerRadius = rect.TopLeftCornerRadius;

            rectData.Vertex1.BottomLeftCornerRadius = rect.BottomLeftCornerRadius;
            rectData.Vertex2.BottomLeftCornerRadius = rect.BottomLeftCornerRadius;
            rectData.Vertex3.BottomLeftCornerRadius = rect.BottomLeftCornerRadius;
            rectData.Vertex4.BottomLeftCornerRadius = rect.BottomLeftCornerRadius;

            rectData.Vertex1.BottomRightCornerRadius = rect.BottomRightCornerRadius;
            rectData.Vertex2.BottomRightCornerRadius = rect.BottomRightCornerRadius;
            rectData.Vertex3.BottomRightCornerRadius = rect.BottomRightCornerRadius;
            rectData.Vertex4.BottomRightCornerRadius = rect.BottomRightCornerRadius;

            rectData.Vertex1.TopRightCornerRadius = rect.TopRightCornerRadius;
            rectData.Vertex2.TopRightCornerRadius = rect.TopRightCornerRadius;
            rectData.Vertex3.TopRightCornerRadius = rect.TopRightCornerRadius;
            rectData.Vertex4.TopRightCornerRadius = rect.TopRightCornerRadius;

            var totalBytes = rectData.GetTotalBytes();
            var data = rectData.ToArray();
            var offset = totalBytes * batchIndex;

            BindVBO();

            GLInvoker.BufferSubData(BufferTargetARB.ArrayBuffer, (nint)offset, totalBytes, data);

            UnbindVBO();

            GLInvoker.EndGroup();
        }

        private RectData ProcessColor(RectData rectData, Rectangle rect)
        {
            if (rect.ApplyGradient is false)
            {
                rectData.Vertex1.Color = rect.Color;
                rectData.Vertex2.Color = rect.Color;
                rectData.Vertex3.Color = rect.Color;
                rectData.Vertex4.Color = rect.Color;
                return rectData;
            }

            switch (rect.GradientType)
            {
                case Gradient.Horizontal:
                    rectData.Vertex1.Color = rect.GradientStart; // BOTTOM LEFT
                    rectData.Vertex2.Color = rect.GradientStart; // BOTTOM RIGHT
                    rectData.Vertex3.Color = rect.GradientStop; // TOP RIGHT
                    rectData.Vertex4.Color = rect.GradientStop; // BOTTOM RIGHT
                    break;
                case Gradient.Vertical:
                    rectData.Vertex1.Color = rect.GradientStart;
                    rectData.Vertex2.Color = rect.GradientStop;
                    rectData.Vertex3.Color = rect.GradientStart;
                    rectData.Vertex4.Color = rect.GradientStop;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return rectData;
        }

        protected override void PrepareForUse()
        {
            BindVAO();
        }

        protected override float[] GenerateData()
        {
            var result = new List<RectData>();

            for (var i = 0u; i < BatchSize; i++)
            {
                result.AddRange(new RectData[]
                {
                    new ()
                    {
                        Vertex1 = new RectVertexData
                        {
                            VertexPos = new Vector2(-1.0f, 1.0f),
                            BorderThickness = 1f,
                        },
                        Vertex2 = new RectVertexData
                        {
                            VertexPos = new Vector2(-1.0f, -1.0f),
                            BorderThickness = 1f,
                        },
                        Vertex3 = new RectVertexData
                        {
                            VertexPos = new Vector2(1.0f, 1.00f),
                            BorderThickness = 1f,
                        },
                        Vertex4 = new RectVertexData
                        {
                            VertexPos = new Vector2(1.0f, -1.0f),
                            BorderThickness = 1f,
                        }
                    }
                });
            }

            _rectData = result.ToArray();

            return result.ToVertexArray();
        }
    }
}
