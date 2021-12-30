using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using Silk.NET.OpenGL;

namespace OBSGreenScreen
{
    public static class ExtensionMethods
    {
        public static uint _totalDebugGroups;

        /// <summary>
        /// Maps the given <paramref name="value"/> from one range to another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromStart">The from starting range value.</param>
        /// <param name="fromStop">The from ending range value.</param>
        /// <param name="toStart">The to starting range value.</param>
        /// <param name="toStop">The to ending range value.</param>
        /// <returns>A value that has been mapped to a range between <paramref name="toStart"/> and <paramref name="toStop"/>.</returns>
        public static float MapValue(this byte value, float fromStart, float fromStop, float toStart, float toStop)
            => toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));

        /// <summary>
        /// Maps the given <paramref name="value"/> from one range to another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromStart">The from starting range value.</param>
        /// <param name="fromStop">The from ending range value.</param>
        /// <param name="toStart">The to starting range value.</param>
        /// <param name="toStop">The to ending range value.</param>
        /// <returns>A value that has been mapped to a range between <paramref name="toStart"/> and <paramref name="toStop"/>.</returns>
        public static float MapValue(this int value, float fromStart, float fromStop, float toStart, float toStop)
            => MapValue((float) value, fromStart, fromStop, toStart, toStop);

        public static float MapValue(this uint value, uint fromStart, uint fromStop, uint toStart, uint toStop)
            => MapValue((float) value, fromStart, fromStop, toStart, toStop);

        /// <summary>
        /// Maps the given <paramref name="value"/> from one range to another.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="fromStart">The from starting range value.</param>
        /// <param name="fromStop">The from ending range value.</param>
        /// <param name="toStart">The to starting range value.</param>
        /// <param name="toStop">The to ending range value.</param>
        /// <returns>A value that has been mapped to a range between <paramref name="toStart"/> and <paramref name="toStop"/>.</returns>
        public static float MapValue(this float value, float fromStart, float fromStop, float toStart, float toStop)
        {
            return toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));
        }

        public static float ToPixelX(this float value, float screenWidth) => value.MapValue(-1f, 1f, 0f, screenWidth);

        public static float ToPixelY(this float value, float screenHeight) => value.MapValue(1f, -1f, 0f, screenHeight);

        public static float ToNDCX(this float value, float screenWidth) => value.MapValue(0f, screenWidth, -1f, 1f);

        public static float ToNDCY(this float value, float screenHeight) => value.MapValue(0f, screenHeight, 1f, -1f);

        public static Color SetAlpha(this Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

        public static Vector2 RotateAround(this Vector2 vector, Vector2 origin, float angle, bool clockWise = true)
        {
            var angleRadians = clockWise ? angle.ToRadians() : angle.ToRadians() * -1;

            var cos = (float) Math.Cos(angleRadians);
            var sin = (float) Math.Sin(angleRadians);

            var dx = vector.X - origin.X; // The delta x
            var dy = vector.Y - origin.Y; // The delta y

            var tempX = (dx * cos) - (dy * sin);
            var tempY = (dx * sin) + (dy * cos);

            var x = tempX + origin.X;
            var y = tempY + origin.Y;

            return new Vector2(x, y);
        }

        public static Vector2 XY(this Vector4 vector) => new(vector.X, vector.Y);
        public static Vector2 ZW(this Vector4 vector) => new(vector.Z, vector.W);

        /// <summary>
        /// Converts the given <paramref name="degrees"/> value into radians.
        /// </summary>
        /// <param name="degrees">The value to convert.</param>
        /// <returns>The degrees converted into radians.</returns>
        public static float ToRadians(this float degrees) => degrees * (float) Math.PI / 180f;

        public static float ToDegrees(this float radians) => radians * 180.0f / (float) Math.PI;

        public static float DotProduct(this Vector2 vectorA, Vector2 vectorB)
        {
            return (vectorA.X * vectorB.X) + (vectorA.Y * vectorB.Y);
        }

        public static float GetLength(this Vector2 start, Vector2 stop)
        {
            return (float) Math.Sqrt(Math.Pow(start.X - stop.X, 2f) + Math.Pow(start.Y - stop.Y, 2f));
        }

        public static float GetAngle(this Vector2 vector)
        {
            var yAxis = new Line()
            {
                Start = new Vector2(0, 0),
                Stop = new Vector2(0, 10)
            };

            var yAxisLen = yAxis.Start.GetLength(yAxis.Start);
            var vectorLen = vector.Length();

            var cosTheta = yAxis.Stop.DotProduct(vector) / (yAxisLen * vectorLen);

            var angle = cosTheta.ToDegrees();

            return angle;
        }

        public static float AngleBetween(this Vector2 vecA, Vector2 vecB)
        {
            var vectorALen = GetLength(vecA, vecB);
            var vectorBLen = GetLength(vecA, vecB);

            if (vectorALen == 0f || vectorBLen == 0f)
            {
                return 1;
            }
            else
            {
                var angleRadians = (float) Math.Acos(DotProduct(vecA, vecB) / (vectorALen * vectorBLen));

                return angleRadians.ToDegrees();
            }
        }

        public static float GetAngle(this Line lineA)
        {
            var originRefPoint = new Vector2(0, -100);
            var lineTranslatedToOrigin = lineA.Stop - lineA.Start;

            var radiansBetween0And180 = 1f;

            var vectorALen = GetLength(Vector2.Zero, lineTranslatedToOrigin);
            var vectorRefLen = GetLength(Vector2.Zero, originRefPoint);

            if (vectorRefLen != 0f && vectorALen != 0f)
            {
                radiansBetween0And180 =
                    (float) Math.Acos(DotProduct(lineTranslatedToOrigin, originRefPoint) / (vectorRefLen * vectorALen));
            }

            // If line B stop X is to the right of the stop X of line A
            if (lineA.Stop.X >= lineA.Start.X)
            {
                return radiansBetween0And180.ToDegrees();
            }

            return 180f + (180f - radiansBetween0And180.ToDegrees());
        }

        public static float GetAngle(this Line line, Vector2 originRefPoint)
        {
            var refLine = new Line()
            {
                Start = line.Start,
                Stop = originRefPoint,
            };

            return AngleBetweenLines(refLine, line);
        }

        public static float AngleBetweenLines(this Line lineA, Line lineB)
        {
            var angleLineA = lineA.GetAngle();
            var angleDelta = lineB.GetAngle() - angleLineA;

            return angleDelta >= -(angleLineA) && angleDelta < 0
                ? 360 - (angleDelta * -1)
                : angleDelta;
        }

        /// <summary>
        /// Removes all occurrences of the given <paramref name="trimChar"/> from the left
        /// side of all occurrences of the given <paramref name="value"/>, inside of this string.
        /// </summary>
        /// <param name="content">The string data containing the values to trim.</param>
        /// <param name="value">The value to trim the characters from.</param>
        /// <param name="trimChar">The character to trim off the <paramref name="value"/>.</param>
        /// <returns>
        ///     The content with all occurrences of the <paramref name="value"/> trimmed.
        /// </returns>
        public static string TrimLeftOf(this string content, string value, char trimChar)
        {
            var result = new StringBuilder(content);

            while (result.Contains($"{trimChar}{value}"))
            {
                result.Replace($"{trimChar}{value}", value);
            }

            return result.ToString();
        }

        /// <summary>
        /// Removes all occurrences of the given <paramref name="trimChar"/> from the right
        /// side of all occurrences of the given <paramref name="value"/>, inside of this string.
        /// </summary>
        /// <param name="content">The string data containing the values to trim.</param>
        /// <param name="value">The value to trim the characters from.</param>
        /// <param name="trimChar">The character to trim off the <paramref name="value"/>.</param>
        /// <returns>
        ///     The content with all occurrences of the <paramref name="value"/> trimmed.
        /// </returns>
        public static string TrimRightOf(this string content, string value, char trimChar)
        {
            var result = new StringBuilder(content);

            while (result.Contains($"{value}{trimChar}"))
            {
                result.Replace($"{value}{trimChar}", value);
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a value indicating if the <see cref="StringBuilder"/> contains the given <paramref name="value"/>.
        /// </summary>
        /// <param name="builder">The string builder to check.</param>
        /// <param name="value">The value to check for.</param>
        /// <returns>
        ///     True if the <paramref name="value"/> is contained in the <see cref="StringBuilder"/>.
        /// </returns>
        private static bool Contains(this StringBuilder builder, string value) => builder.ToString().Contains(value);

        public static Vector2 ToNDC(this Vector2 pixelVector, float pixelScreenWidth, float pixelScreenHeight)
        {
            var ndcX = pixelVector.X.MapValue(0, pixelScreenWidth, -1f, 1f);
            var ndcY = pixelVector.Y.MapValue(0, pixelScreenHeight, 1f, -1f);
            return new Vector2(ndcX, ndcY);
        }

        public static float ToColorNDC(this float value) => ((float) value).MapValue(0f, 255f, 0f, 1f);

        public static float[] ToColorNDCArray(this Color value)
        {
            var colorComponents = value.ToArray();

            var result = new List<float>();

            foreach (float component in colorComponents)
            {
                result.Add(component.ToColorNDC());
            }

            return result.ToArray();
        }

        public static Vector2 ToStart(this Vector4 vector) => new Vector2(vector.X, vector.Y);

        public static Vector2 ToStop(this Vector4 vector) => new Vector2(vector.Z, vector.W);

        public static void LabelShader(this GL gl, uint shaderId, string label)
            => gl.ObjectLabel(ObjectIdentifier.Shader, shaderId, (uint) label.Length, label);

        public static void LabelShaderProgram(this GL gl, uint shaderId, string label)
            => gl.ObjectLabel(ObjectIdentifier.Program, shaderId, (uint) label.Length, label);

        public static void LabelVertexArray(this GL gl, uint vertexArrayId, string label, Action bindVertexArrayObj)
        {
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            var newLabel = $"{label} VAO";

            bindVertexArrayObj();
            gl.ObjectLabel(ObjectIdentifier.VertexArray, vertexArrayId, (uint)newLabel.Length, newLabel);
        }

        public static void LabelBuffer(this GL gl, uint bufferId, string label, BufferType bufferType, Action bindBufferObj)
        {
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            var bufferTypeAcronym = string.Empty;

            switch (bufferType)
            {
                case BufferType.VertexBufferObject:
                    bufferTypeAcronym = "VBO";
                    break;
                case BufferType.IndexArrayObject:
                    bufferTypeAcronym = "EBO";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bufferType), bufferType, null);
            }

            var newLabel = $"{label} {bufferTypeAcronym}";

            bindBufferObj();
            gl.ObjectLabel(ObjectIdentifier.Buffer, bufferId, (uint)newLabel.Length, newLabel);
        }

        public static void BeginGroup(this GL gl, string name)
        {
            if (gl is null)
            {
                throw new NullReferenceException("The GL object is null");
            }

            gl.PushDebugGroup(DebugSource.DebugSourceApplication, 100, (uint)name.Length, name);
            _totalDebugGroups += 1;
        }

        public static void EndGroup(this GL gl)
        {
            if (gl is null)
            {
                throw new NullReferenceException("The GL object is null");
            }

            gl.PopDebugGroup();
            _totalDebugGroups -= 1;
        }

        public static void BufferSubData(this GL gl, BufferTargetARB target, nint offset, nuint size, float[] data)
        {
            unsafe
            {
                fixed(void* dataPtr = data)
                {
                    gl.BufferSubData(BufferTargetARB.ArrayBuffer, offset, size, dataPtr);
                }
            }
        }

        public static uint TotalBytes(this IEnumerable<LineData> data) => (uint)data.Sum(d => d.GetTotalBytes());

        public static uint TotalBytes(this IEnumerable<RectData> data) => (uint)data.Sum(d => d.GetTotalBytes());

        public static uint TotalBytes(this IEnumerable<EllipseData> data) => (uint)data.Sum(d => d.GetTotalBytes());

        public static float[] ToArray(this Vector2 vector) => new[] { vector.X, vector.Y };

        public static float[] ToArray(this Vector4 vector) => new[] { vector.X, vector.Y, vector.Z, vector.W };

        public static float[] ToArray(this Color color)
        {
            return new float [] { color.R, color.G, color.B, color.A };
        }

        public static float[] ToVertexArray(this IEnumerable<LineData> lines)
        {
            var result = new List<float>();

            foreach (var vector in lines)
            {
                result.AddRange(vector.ToArray());
            }

            return result.ToArray();
        }

        public static float[] ToVertexArray(this IEnumerable<RectData> rects)
        {
            var result = new List<float>();

            foreach (var quad in rects)
            {
                result.AddRange(quad.ToArray());
            }

            return result.ToArray();
        }

        public static float[] ToVertexArray(this IEnumerable<EllipseData> rects)
        {
            var result = new List<float>();

            foreach (var quad in rects)
            {
                result.AddRange(quad.ToArray());
            }

            return result.ToArray();
        }

        public static SizeF GetViewPortSize(this GL gl)
        {
            /*
             * [0] = X
             * [1] = Y
             * [3] = Width
             * [4] = Height
             */
            var data = new int[4];

            gl.GetInteger(GetPName.Viewport, data);

            return new SizeF(data[2], data[3]);
        }

        public static void SetViewPortSize(this GL gl, uint width, uint height)
        {
            /*
             * [0] = X
             * [1] = Y
             * [3] = Width
             * [4] = Height
             */
            var data = new int[4];

            gl.GetInteger(GetPName.Viewport, data);
            gl.Viewport(data[0], data[1], width, height);
        }

        public static bool IsNotRenderable(this Dictionary<uint, (bool shouldRender, Line line)> items)
            => items.All(i => i.Value.shouldRender is false);

        public static bool IsNotRenderable(this Dictionary<uint, (bool shouldRender, Rectangle rect)> items)
            => items.All(i => i.Value.shouldRender is false);

        public static bool IsNotRenderable(this Dictionary<uint, (bool shouldRender, Ellipse ellipse)> items)
            => items.All(i => i.Value.shouldRender is false);
    }
}
