using System;
using System.Collections.Generic;
using System.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    [SpriteBatchSize(Game.BatchSize)]
    public class SpriteBatch
    {
        private readonly Dictionary<uint, (bool shouldRender, Ellipse ellipse)> _batchedEllipseItems = new ();
        private readonly Dictionary<uint, (bool shouldRender, Rectangle rect)> _batchedRectItems = new ();
        private readonly Dictionary<uint, (bool shouldRender, Line line)> _batchedLineItems = new ();
        private readonly ShaderProgram _lineShader;
        private readonly ShaderProgram _rectShader;
        private readonly ShaderProgram _ellipseShader;
        private readonly LineGPUBuffer _lineBuffer;
        private readonly RectGPUBuffer _rectBuffer;
        private readonly EllipseGPUBuffer _ellipseBuffer;
        private readonly uint _batchSize;
        private bool _batchHasBegun;
        private uint _currentLineBatchIndex;
        private uint _currentRectBatchIndex;
        private uint _currentEllipseBatchIndex;
        private readonly EllipseBatchService _ellipseBatchService;
        private readonly RectangleBatchService _rectBatchService;
        private readonly LineBatchService _lineBatchService;

        public SpriteBatch(
            ShaderProgram lineShader,
            ShaderProgram rectShader,
            ShaderProgram ellipseShader,
            LineGPUBuffer lineBuffer,
            RectGPUBuffer rectBuffer,
            EllipseGPUBuffer ellipseBuffer,
            LineBatchService lineBatchService,
            RectangleBatchService rectBatchService,
            EllipseBatchService ellipseBatchService)
        {
            _lineShader = lineShader;
            _rectShader = rectShader;
            _ellipseShader = ellipseShader;
            _lineBuffer = lineBuffer;
            _rectBuffer = rectBuffer;
            _ellipseBuffer = ellipseBuffer;

            var customAttrs = Attribute.GetCustomAttributes(typeof(SpriteBatch));
            var batchSizeAttr = (SpriteBatchSizeAttribute) customAttrs[0];

            _batchSize = batchSizeAttr.BatchSize;

            _lineBatchService = lineBatchService;
            _lineBatchService.BatchSize = _batchSize;
            _lineBatchService.BatchFilled += LineBatchServiceOnBatchFull;

            _rectBatchService = rectBatchService;
            _rectBatchService.BatchSize = _batchSize;
            _rectBatchService.BatchFilled += RectBatchServiceOnBatchFilled;

            _ellipseBatchService = ellipseBatchService;
            _ellipseBatchService.BatchSize = _batchSize;
            _ellipseBatchService.BatchFilled += EllipseBatchServiceOnBatchFull;

            Init();
        }

        public Color ClearColor
        {
            get
            {
                var data = new float[4];
                GLInvoker.GetFloat(GetPName.ColorClearValue, data);

                var red = (byte)data[0].MapValue(0f, 1f, 0f, 255f);
                var green = (byte)data[1].MapValue(0f, 1f, 0f, 255f);
                var blue = (byte)data[2].MapValue(0f, 1f, 0f, 255f);
                var alpha = (byte)data[3].MapValue(0f, 1f, 0f, 255f);

                return Color.FromArgb(alpha, red, green, blue);
            }
            set
            {
                var red = value.R.MapValue(0f, 255f, 0f, 1f);
                var green = value.G.MapValue(0f, 255f, 0f, 1f);
                var blue = value.B.MapValue(0f, 255f, 0f, 1f);
                var alpha = value.A.MapValue(0f, 255f, 0f, 1f);

                GLInvoker.ClearColor(red, green, blue, alpha);
            }
        }

        public bool Enabled { get; set; } = true;

        public void Begin()
        {
            _batchHasBegun = true;
        }

        public void RenderLine(Line line)
        {
            if (Enabled is false || line.IsEmpty())
            {
                return;
            }

            if (_batchHasBegun is false)
            {
                throw new Exception("The batch begin() method must be invoked first.");
            }

            _lineBatchService.Add(line);
        }

        public void RenderRectangle(Rectangle rect)
        {
            if (Enabled is false || rect.IsEmpty())
            {
                return;
            }

            if (_batchHasBegun is false)
            {
                throw new Exception("The batch begin() method must be invoked first.");
            }

            _rectBatchService.Add(rect);
        }

        public void RenderEllipse(Ellipse ellipse)
        {
            if (Enabled is false || ellipse.IsEmpty())
            {
                return;
            }

            if (_batchHasBegun is false)
            {
                throw new Exception("The batch begin() method must be invoked first.");
            }

            _ellipseBatchService.Add(ellipse);
        }

        public bool RenderAll { get; set; } = true;

        public void OnResize(Vector2D<int> size)
        {
            var viewPortSize = new SizeF(size.X <= 0f ? 1f : size.X, size.Y <= 0f ? 1f : size.Y);

            _lineBuffer.ViewPortSize = viewPortSize;
            _rectBuffer.ViewPortSize = viewPortSize;
            _ellipseBuffer.ViewPortSize = viewPortSize;
        }

        private void Init()
        {
            _lineBuffer?.Init();
            _rectBuffer?.Init();
            _ellipseBuffer?.Init();
        }

        private void LineBatchServiceOnBatchFull(object? sender, EventArgs e)
        {
            if (Enabled is false)
            {
                return;
            }

            if (_lineBatchService.TotalItemsToRender <= 0)
            {
                return;
            }

            GLInvoker.BeginGroup("Render Lines");

            _lineShader.Use();

            var itemsToRender = _lineBatchService.RenderableItems;

            foreach (var (batchIndex, line) in itemsToRender)
            {
                _lineBuffer.UpdateData(line, batchIndex);
            }

            unsafe
            {
                var totalIndices = 6u * (uint)itemsToRender.Count;

                GLInvoker.DrawElements(PrimitiveType.Triangles, totalIndices, DrawElementsType.UnsignedInt, (void*)0);
            }

            _lineBatchService.EmptyBatch();

            GLInvoker.EndGroup();
        }

        private void RectBatchServiceOnBatchFilled(object? sender, EventArgs e)
        {
            if (Enabled is false)
            {
                return;
            }

            if (_rectBatchService.TotalItemsToRender <= 0)
            {
                return;
            }

            GLInvoker.BeginGroup("Render Rectangles");

            _rectShader.Use();

            var itemsToRender = _rectBatchService.RenderableItems;

            foreach (var (batchIndex, rect) in itemsToRender)
            {
                _rectBuffer.UpdateData(rect, batchIndex);
            }

            unsafe
            {
                var totalIndices = 6u * (uint)itemsToRender.Count;

                var callStack = GLInvoker.GLCallStack;

                var formattedCallStack = string.Empty;

                foreach (var call in callStack)
                {
                    formattedCallStack += $"{call}|";
                }

                GLInvoker.DrawElements(PrimitiveType.Triangles, totalIndices, DrawElementsType.UnsignedInt, (void*)0);
            }

            _rectBatchService.EmptyBatch();

            GLInvoker.EndGroup();
        }

        private void EllipseBatchServiceOnBatchFull(object? sender, EventArgs e)
        {
            if (Enabled is false)
            {
                return;
            }

            if (_ellipseBatchService.TotalItemsToRender <= 0)
            {
                return;
            }

            GLInvoker.BeginGroup("Render Ellipses");

            _ellipseShader.Use();

            var itemsToRender = _ellipseBatchService.RenderableItems;

            foreach (var (batchIndex, ellipse) in itemsToRender)
            {
                _ellipseBuffer.UpdateData(ellipse, batchIndex);
            }

            unsafe
            {
                var totalIndices = 6u * (uint)itemsToRender.Count;

                GLInvoker.DrawElements(PrimitiveType.Triangles, totalIndices, DrawElementsType.UnsignedInt, (void*)0);
            }

            _ellipseBatchService.EmptyBatch();

            GLInvoker.EndGroup();
        }

        public void End()
        {
            LineBatchServiceOnBatchFull(_lineBatchService, EventArgs.Empty);
            RectBatchServiceOnBatchFilled(_rectBatchService, EventArgs.Empty);
            EllipseBatchServiceOnBatchFull(_ellipseBatchService, EventArgs.Empty);

            _batchHasBegun = false;
        }

        public void Clear()
        {
            //Clear the color screen
            GLInvoker.Clear((uint) ClearBufferMask.ColorBufferBit);
            _lineBatchService.EmptyBatch();
        }
    }
}
