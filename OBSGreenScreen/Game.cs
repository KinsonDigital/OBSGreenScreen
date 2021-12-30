using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using OBSGreenScreen.VelaptorStuff;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using MouseButton = Silk.NET.Input.MouseButton;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    public class Game
    {
        private FileSystem _fileSystem = new ();
        private static DebugProc? _debugCallback;
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 600;
        private IWindow _glWindow;
        private Line _line = new ();
        private SpriteBatch _spriteBatch;
        private ShaderProgram _lineShader;
        private ShaderProgram _rectShader;
        private ShaderProgram _ellipseShader;
        private LineGPUBuffer _lineBuffer;
        private RectGPUBuffer _rectBuffer;
        private EllipseGPUBuffer _ellipseBuffer;
        private KeyboardState _currentKeyState;
        private KeyboardState _previousKeyState;
        private MouseState _currentMouseState = new();
        private MouseState _previousMouseState = new();
        private Keyboard _keyboard;
        private Mouse _mouse;
        private PerfRunner _fps;
        private bool _startPerfCheck;
        private double _avePerf;
        private List<Ellipse> _ellipses = new();
        private List<Rectangle> _rects = new();
        private List<Line> _lines = new();
        private bool _mouseDown;
        private Vector2 _mouseDownLocation = Vector2.Zero;
        private Line _currentLine = default;

        public Game()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(SCREEN_WIDTH, SCREEN_HEIGHT);
            options.Title = "LearnOpenGL with Silk.NET";
            options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(4, 5))
            {
                Profile = ContextProfile.Core
            };
            _glWindow = Window.Create(options);

            _glWindow.Load += OnLoad;
            _glWindow.Render += OnRender;
            _glWindow.Update += OnUpdate;
            _glWindow.Closing += OnClose;
            _glWindow.Resize += OnResize;
            _glWindow.Title = "Moving Vertex A";

            _line.Start = new Vector2(0.5f, 0.5f);
            _line.Stop = new Vector2(0.5f, -0.5f);

            _fps = new PerfRunner
            {
                TotalSamples = 500,
                Enabled = true
            };
        }

        public const uint BatchSize = 100;

        public void Run() => _glWindow.Run();

        private void OnLoad()
        {
            // Getting the opengl api for drawing to the screen.
            GLInvoker.SetGLObject(Silk.NET.OpenGL.GL.GetApi(_glWindow));
            _glWindow.Position = new Vector2D<int>(600, 250);

            SetupErrorCallback();

            var shaderSrcTemplateService = new ShaderTemplateProcessorService();
            var resourceLoaderService = new EmbeddedResourceLoaderService();
            var shaderSrcLoader = new ShaderResourceLoaderService(
                shaderSrcTemplateService,
                resourceLoaderService,
                _fileSystem.Path);

            GLInvoker.Enable(EnableCap.Blend);
            GLInvoker.Enable(EnableCap.LineSmooth);
            GLInvoker.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            SetupLineBufferAndShader(shaderSrcLoader);
            SetupRectBufferAndShader(shaderSrcLoader);
            SetupEllipseBufferAndShader(shaderSrcLoader);

            var lineBatchService = new LineBatchService();
            var ellipseBatchService = new EllipseBatchService();
            var rectBatchService = new RectangleBatchService();

            _spriteBatch = new SpriteBatch(
                _lineShader,
                _rectShader,
                _ellipseShader,
                _lineBuffer,
                _rectBuffer,
                _ellipseBuffer,
                lineBatchService,
                rectBatchService,
                ellipseBatchService);

            _spriteBatch.ClearColor = Color.FromArgb(255, 43, 43, 43);
            _glWindow.Resize += _spriteBatch.OnResize;

            SetupLineData();
            SetupRectData();
            SetupEllipseData();

            GLInvoker.SetViewPortSize(SCREEN_WIDTH, SCREEN_HEIGHT);

            SetupKeyboard();
            SetupMouse();
        }

        private void SetupLineData()
        {
            for (var i = 0; i < 1000; i++)
            {
                break;
                _lines.Add(GenerateRandomLine());
            }

            // _lines.Add(new Line
            // {
            //     Start = new Vector2(100, 250),
            //     Stop = new Vector2(700, 450),
            //     Color = Color.IndianRed,
            //     LineThickness = 12,
            // });

            _lines.Add(new Line
            {
                Start = new Vector2(400, 300),
                Stop = new Vector2(400, 100),
                Color = Color.CornflowerBlue.SetAlpha(255),
                LineThickness = 6,
                ApplyGradient = true,
                GradientType = Gradient.Vertical,
                GradientStart = Color.LightGreen,
                GradientStop = Color.DarkGreen,
            });
        }

        private void SetupRectData()
        {
            for (var i = 0; i < 1000; i++)
            {
                break;
                _rects.Add(GenerateRandomRect());
            }

            // _rects.Add(new Rectangle
            // {
            //     DataChanged = true,
            //     Position = new Vector2(SCREEN_WIDTH / 2f, SCREEN_HEIGHT / 2f),
            //     Width = 100,
            //     Height = 100,
            //     Color = Color.DarkOrange,
            //     IsFilled = true,
            //     BorderThickness = 1,
            //     CornerRadius = 0f,
            //     ApplyGradient = false,
            // });

            _rects.Add(new Rectangle
            {
                Position = new Vector2(SCREEN_WIDTH / 2f, SCREEN_HEIGHT / 4f * 3),
                Width = 100,
                Height = 100,
                Color = Color.DarkOrange,
                IsFilled = true,
                BorderThickness = 1,
                TopLeftCornerRadius = 0f,
                BottomLeftCornerRadius = 10,
                BottomRightCornerRadius = 20,
                TopRightCornerRadius = 30,
                ApplyGradient = true,
                GradientType = Gradient.Horizontal,
                GradientStart = Color.Red,
                GradientStop = Color.Blue,
            });
        }

        private void SetupEllipseData()
        {
            for (var i = 0; i < 1000; i++)
            {
                break;
                _ellipses.Add(GenerateRandomEllipse());
            }

            // TODO: Figure out gradients
            // _ellipses.Add(new Ellipse
            // {
            //     Position =  new Vector2(SCREEN_WIDTH / 4f, SCREEN_HEIGHT / 2f),
            //     RadiusX = 50,
            //     RadiusY = 25,
            //     Color =  Color.DarkOrange,
            //     IsFilled = false,
            //     BorderThickness = 1,
            // });

            _ellipses.Add(new Ellipse
            {
                Position =  new Vector2(SCREEN_WIDTH / 4f * 3f, SCREEN_HEIGHT / 2f),
                RadiusX = 50,
                RadiusY = 25,
                Color =  Color.MediumPurple,
                IsFilled = true,
                BorderThickness = 1,
                ApplyGradient = true,
                GradientType = Gradient.Horizontal,
                GradientStart = Color.MediumPurple,
                GradientStop = Color.Purple,
            });
        }

        private void SetupLineBufferAndShader(ShaderResourceLoaderService shaderSrcLoader)
        {
            _lineShader = new ShaderProgram(shaderSrcLoader);
            _lineShader.LoadShader("Line", BatchSize);

            _lineBuffer = new LineGPUBuffer();
            _lineBuffer.ViewPortSize = new SizeF(SCREEN_WIDTH, SCREEN_HEIGHT);
        }

        private void SetupRectBufferAndShader(ShaderResourceLoaderService shaderSrcLoader)
        {
            _rectShader = new ShaderProgram(shaderSrcLoader);
            _rectShader.LoadShader("Rect", BatchSize);

            _rectBuffer = new RectGPUBuffer();
            _rectBuffer.ViewPortSize = new SizeF(SCREEN_WIDTH, SCREEN_HEIGHT);
        }

        private void SetupEllipseBufferAndShader(ShaderResourceLoaderService shaderSrcLoader)
        {
            _ellipseShader = new ShaderProgram(shaderSrcLoader);
            _ellipseShader.LoadShader("Ellipse", BatchSize);

            _ellipseBuffer = new EllipseGPUBuffer();
            _ellipseBuffer.ViewPortSize = new SizeF(SCREEN_WIDTH, SCREEN_HEIGHT);
        }

        private void OnUpdate(double obj)
        {
            _fps.Start();

            _currentKeyState = _keyboard.GetState();
            _currentMouseState = _mouse.GetState();

            if (_currentMouseState.IsDown(MouseButton.Left))
            {
                if (_mouseDownLocation == Vector2.Zero)
                {
                    _mouseDownLocation = new Vector2(_currentMouseState.X, _currentMouseState.Y);
                }

                _currentLine.Start = _mouseDownLocation;
                _currentLine.Stop = new Vector2(_currentMouseState.X, _currentMouseState.Y);
                _currentLine.Color = Color.CornflowerBlue;
                _currentLine.LineThickness = 2;

                _mouseDown = true;
            }

            // Save the current line to the list of lines
            if (_currentMouseState.IsUp(MouseButton.Left) && _previousMouseState.IsDown(MouseButton.Left))
            {
                _mouseDownLocation = Vector2.Zero;
                _mouseDown = false;
                _lines.Add(_currentLine);
                _currentLine.Empty();
            }

            // If the right mouse button was clicked, clear all of the shapes
            if (_currentMouseState.IsUp(MouseButton.Right) && _previousMouseState.IsDown(MouseButton.Right))
            {
                _lines.Clear();
                _currentLine.Empty();
            }

            _previousKeyState = _currentKeyState;
            _previousMouseState = _currentMouseState;
        }

        [Benchmark]
        private void OnRender(double obj)
        {
            GLInvoker.BeginGroup("Render Process");

            _spriteBatch.Clear();

            _spriteBatch.Begin();

            if (_currentLine.IsEmpty() is false)
            {
                _spriteBatch.RenderLine(_currentLine);
            }

            foreach (var line in _lines)
            {
                _spriteBatch.RenderLine(line);
            }

            foreach (var rect in _rects)
            {
                break;
                _spriteBatch.RenderRectangle(rect);
            }

            foreach (var circle in _ellipses)
            {
                break;
                _spriteBatch.RenderEllipse(circle);
            }

            _spriteBatch.End();

            GLInvoker.EndGroup();

            _fps.Stop();
            _fps.Record();
            _fps.Reset();
        }

        private void OnClose()
        {
            _ellipseShader?.Dispose();
        }

        private void OnResize(Vector2D<int> size)
        {
            GLInvoker.SetViewPortSize((uint)size.X, (uint)size.Y);

            _spriteBatch.Enabled = size.X > 0 && size.Y > 0;
        }

        private void MoveRect()
        {
            if (_rects is null || _rects.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsDown(Key.ShiftLeft) ||
                _currentKeyState.IsDown(Key.ControlLeft) ||
                _currentKeyState.IsDown(Key.AltLeft))
            {
                return;
            }

            const float moveAmount = 20f;
            var shapeToMove = _rects[^1];

            if (_currentKeyState.IsDown(Key.Left))
            {
                shapeToMove.Position = new Vector2(shapeToMove.Position.X - moveAmount, shapeToMove.Position.Y);
            }

            if (_currentKeyState.IsDown(Key.Right))
            {
                shapeToMove.Position = new Vector2(shapeToMove.Position.X + moveAmount, shapeToMove.Position.Y);
            }

            if (_currentKeyState.IsDown(Key.Up))
            {
                shapeToMove.Position = new Vector2(shapeToMove.Position.X, shapeToMove.Position.Y - moveAmount);
            }

            if (_currentKeyState.IsDown(Key.Down))
            {
                shapeToMove.Position = new Vector2(shapeToMove.Position.X, shapeToMove.Position.Y + moveAmount);
            }

            _rects[^1] = shapeToMove;
        }

        private void MoveEllipse()
        {
            if (_ellipses is null || _ellipses.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsDown(Key.ShiftLeft) ||
                _currentKeyState.IsDown(Key.ControlLeft) ||
                _currentKeyState.IsDown(Key.AltLeft))
            {
                return;
            }

            const float moveAmount = 20f;
            var shapeToChange = _ellipses[^1];

            if (_currentKeyState.IsDown(Key.Left))
            {
                shapeToChange.Position = new Vector2(shapeToChange.Position.X - moveAmount, shapeToChange.Position.Y);
            }

            if (_currentKeyState.IsDown(Key.Right))
            {
                shapeToChange.Position = new Vector2(shapeToChange.Position.X + moveAmount, shapeToChange.Position.Y);
            }

            if (_currentKeyState.IsDown(Key.Up))
            {
                shapeToChange.Position = new Vector2(shapeToChange.Position.X, shapeToChange.Position.Y - moveAmount);
            }

            if (_currentKeyState.IsDown(Key.Down))
            {
                shapeToChange.Position = new Vector2(shapeToChange.Position.X, shapeToChange.Position.Y + moveAmount);
            }

            _ellipses[^1] = shapeToChange;
        }

        private void MoveLine()
        {
            if (_lines is null || _lines.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsDown(Key.ShiftLeft) ||
                _currentKeyState.IsDown(Key.ControlLeft) ||
                _currentKeyState.IsDown(Key.AltLeft))
            {
                return;
            }

            const float moveAmount = 2f;
            var shapeToMove = _lines[^1];

            if (_currentKeyState.IsDown(Key.Left))
            {
                shapeToMove.Start = new Vector2(shapeToMove.Start.X - moveAmount, shapeToMove.Start.Y);
                shapeToMove.Stop = new Vector2(shapeToMove.Stop.X - moveAmount, shapeToMove.Stop.Y);
            }

            if (_currentKeyState.IsDown(Key.Right))
            {
                shapeToMove.Start = new Vector2(shapeToMove.Start.X + moveAmount, shapeToMove.Start.Y);
                shapeToMove.Stop = new Vector2(shapeToMove.Stop.X + moveAmount, shapeToMove.Stop.Y);
            }

            if (_currentKeyState.IsDown(Key.Up))
            {
                shapeToMove.Start = new Vector2(shapeToMove.Start.X, shapeToMove.Start.Y - moveAmount);
                shapeToMove.Stop = new Vector2(shapeToMove.Stop.X, shapeToMove.Stop.Y - moveAmount);
            }

            if (_currentKeyState.IsDown(Key.Down))
            {
                shapeToMove.Start = new Vector2(shapeToMove.Start.X, shapeToMove.Start.Y + moveAmount);
                shapeToMove.Stop = new Vector2(shapeToMove.Stop.X, shapeToMove.Stop.Y + moveAmount);
            }

            _lines[^1] = shapeToMove;
        }

        private void RotateLine()
        {
            if (_lines is null || _lines.Count <= 0)
            {
                return;
            }

            var isCtrlDown = _currentKeyState.IsDown(Key.ControlLeft);

            if (isCtrlDown is false)
            {
                return;
            }

            const float amountToRotate = 1f;
            var lineToUpdate = _lines[^1];

            if (_currentKeyState.IsDown(Key.Right))
            {
                lineToUpdate.Stop = lineToUpdate.Stop.RotateAround(lineToUpdate.Start, amountToRotate);
            }

            if (_currentKeyState.IsDown(Key.Left))
            {
                lineToUpdate.Stop = lineToUpdate.Stop.RotateAround(lineToUpdate.Start, -amountToRotate);
            }

            _lines[^1] = lineToUpdate;
        }

        private void ChangeLineThickness()
        {
            if (_lines is null || _lines.Count <= 0)
            {
                return;
            }

            var isShiftDown = _currentKeyState.IsDown(Key.ShiftLeft);

            if (isShiftDown is false)
            {
                return;
            }

            const uint amountToChange = 1;

            var lineToUpdate = _lines[^1];

            if (_currentKeyState.IsDown(Key.Up))
            {
                lineToUpdate.LineThickness += 1;
            }

            if (_currentKeyState.IsDown(Key.Down))
            {
                lineToUpdate.LineThickness = ((int)lineToUpdate.LineThickness - (int)amountToChange) >= 1
                    ? lineToUpdate.LineThickness - amountToChange
                    : 1;
            }

            _lines[^1] = lineToUpdate;
        }

        private void SetRectFilled()
        {
            if (_rects is null || _rects.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsUp(Key.F) && _previousKeyState.IsDown(Key.F))
            {
                var shapeToChange = _rects[^1];
                shapeToChange.IsFilled = !shapeToChange.IsFilled;

                _rects[^1] = shapeToChange;
            }
        }

        private void SetEllipseFilled()
        {
            if (_ellipses is null || _ellipses.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsUp(Key.F) && _previousKeyState.IsDown(Key.F))
            {
                var shapeToChange = _ellipses[^1];
                shapeToChange.IsFilled = !shapeToChange.IsFilled;

                _ellipses[^1] = shapeToChange;
            }
        }

        private void ChangeRectSize()
        {
            if (_rects is null || _rects.Count <= 0)
            {
                return;
            }

            const int resizeAmount = 1;

            var shapeToMove = _rects[^1];

            var shiftIsDown = _currentKeyState.IsDown(Key.ShiftLeft);

            if (shiftIsDown && _currentKeyState.IsDown(Key.Right))
            {
                shapeToMove.Width += resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Left))
            {
                shapeToMove.Width -= resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Up))
            {
                shapeToMove.Height += resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Down))
            {
                shapeToMove.Height -= resizeAmount;
            }

            _rects[^1] = shapeToMove;
        }

        private void ChangeEllipseSize()
        {
            if (_ellipses is null || _ellipses.Count <= 0)
            {
                return;
            }

            const int resizeAmount = 1;

            var shapeToMove = _ellipses[^1];

            var shiftIsDown = _currentKeyState.IsDown(Key.ShiftLeft);

            if (shiftIsDown && _currentKeyState.IsDown(Key.Right))
            {
                shapeToMove.RadiusX += resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Left))
            {
                shapeToMove.RadiusX -= resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Up))
            {
                shapeToMove.RadiusY += resizeAmount;
            }

            if (shiftIsDown && _currentKeyState.IsDown(Key.Down))
            {
                shapeToMove.RadiusY -= resizeAmount;
            }

            _ellipses[^1] = shapeToMove;
        }

        private int _cornerNumber = 1;

        private void ChangeRectRadius()
        {
            if (_rects is null || _rects.Count <= 0)
            {
                return;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Number1))
            {
                _cornerNumber = 1;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Number2))
            {
                _cornerNumber = 2;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Number3))
            {
                _cornerNumber = 3;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Number4))
            {
                _cornerNumber = 4;
            }

            const int resizeAmount = 1;

            var shapeToChange = _rects[^1];

            if (_currentKeyState.IsDown(Key.ControlLeft) && _currentKeyState.IsDown(Key.Up))
            {
                switch (_cornerNumber)
                {
                    case 1:
                        shapeToChange.TopLeftCornerRadius += resizeAmount;
                        break;
                    case 2:
                        shapeToChange.BottomLeftCornerRadius += resizeAmount;
                        break;
                    case 3:
                        shapeToChange.BottomRightCornerRadius += resizeAmount;
                        break;
                    case 4:
                        shapeToChange.TopRightCornerRadius += resizeAmount;
                        break;
                }
            }

            if (_currentKeyState.IsDown(Key.ControlLeft) && _currentKeyState.IsDown(Key.Down))
            {
                switch (_cornerNumber)
                {
                    case 1:
                        shapeToChange.TopLeftCornerRadius -= resizeAmount;
                        break;
                    case 2:
                        shapeToChange.BottomLeftCornerRadius -= resizeAmount;
                        break;
                    case 3:
                        shapeToChange.BottomRightCornerRadius -= resizeAmount;
                        break;
                    case 4:
                        shapeToChange.TopRightCornerRadius -= resizeAmount;
                        break;
                }
            }

            _rects[^1] = shapeToChange;
        }

        private void ChangeRectBorderThickness()
        {
            if (_rects is null || _rects.Count <= 0)
            {
                return;
            }

            const int resizeAmount = 1;

            var shapeToChange = _rects[^1];

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Up))
            {
                shapeToChange.BorderThickness += resizeAmount;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Down))
            {
                shapeToChange.BorderThickness -= resizeAmount;
            }

            _rects[^1] = shapeToChange;
        }

        private void ChangeEllipseBorderThickness()
        {
            if (_ellipses is null || _ellipses.Count <= 0)
            {
                return;
            }

            const int resizeAmount = 1;

            var shapeToChange = _ellipses[^1];

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Up))
            {
                shapeToChange.BorderThickness += resizeAmount;
            }

            if (_currentKeyState.IsDown(Key.AltLeft) && _currentKeyState.IsDown(Key.Down))
            {
                shapeToChange.BorderThickness -= resizeAmount;
            }

            _ellipses[^1] = shapeToChange;
        }

        private Line GenerateRandomLine()
        {
            var random = new Random();

            Color RandomClr()
            {
                return Color.FromArgb
                (
                    random.Next(62, 255),
                    random.Next(0, 255),
                    random.Next(0, 255),
                    random.Next(0, 255)
                );
            }

            var startX = random.Next(0, SCREEN_WIDTH);
            var startY = random.Next(0, SCREEN_HEIGHT);
            var stopX = random.Next(0, SCREEN_WIDTH);
            var stopY = random.Next(0, SCREEN_HEIGHT);

            return new Line()
            {
                Start = new Vector2(startX, startY),
                Stop = new Vector2(stopX, stopY),
                Color = RandomClr(),
                LineThickness = (uint)random.Next(3, 15),
            };
        }

        private Rectangle GenerateRandomRect()
        {
            // TODO: Test out random alpha values
            var random = new Random();

            Color RandomClr()
            {
                return Color.FromArgb(
                    random.Next(62, 255),
                    random.Next(55, 255),
                    random.Next(55, 255),
                    random.Next(55, 255));
            }

            var width = (uint) random.Next(10, 150);
            var height = (uint) random.Next(10, 150);
            var halfWidth = width / 2.0f;
            var halfHeight = height / 2.0f;

            var x = random.Next((int)halfWidth, _glWindow.Size.X - (int)halfWidth);
            var y = random.Next((int)halfHeight, _glWindow.Size.Y - (int)halfHeight);

            var isFilled = random.Next(0, 100) <= 50;
            var applyGradient= random.Next(0, 100) <= 50;

            return new Rectangle()
            {
                Position = new Vector2(x, y),
                Width = width,
                Height =height,
                Color = RandomClr(),
                IsFilled = isFilled,
                BorderThickness = (uint)random.Next(1, width < height ? (int)(width / 3f) : (int)(height / 3f)),
                ApplyGradient = applyGradient,
                GradientType = random.Next(0, 100) <= 50 ? Gradient.Horizontal : Gradient.Vertical,
                GradientStart = RandomClr(),
                GradientStop = RandomClr(),
            };
        }

        private Ellipse GenerateRandomEllipse()
        {
            // TODO: Test out random alpha values
            var random = new Random();

            Color RandomClr()
            {
                return Color.FromArgb(
                    random.Next(125, 255),
                    random.Next(55, 255),
                    random.Next(55, 255),
                    random.Next(55, 255));
            }

            var width = (uint) random.Next(10, 75);
            var height = (uint) random.Next(10, 75);

            var x = random.Next((int)width / 2, _glWindow.Size.X - (int)(width / 2));
            var y = random.Next((int)width / 2, _glWindow.Size.Y - (int)(width / 2));

            var isFilled = random.Next(0, 100) <= 50;
            // var applyGradient= random.Next(0, 100) <= 50;

            var color = RandomClr();

            // TODO: Setup gradients
            return new Ellipse()
            {
                Position = new Vector2(x, y),
                RadiusX = width,
                RadiusY = height,
                Color = color,
                IsFilled = isFilled,
                BorderThickness = 1,
                // ApplyGradient = applyGradient,
                // GradientType = random.Next(0, 100) <= 50 ? Gradient.Horizontal : Gradient.Vertical,
                // GradientStart = RandomClr(),
                // GradientStop = RandomClr(),
            };
        }

        private void SetupKeyboard()
        {
            _keyboard = new Keyboard();

            var input = _glWindow.CreateInput();

            foreach (var keyboard in input.Keyboards)
            {
                keyboard.KeyDown += _keyboard.KeyDown;
                keyboard.KeyUp += _keyboard.KeyUp;
            }
        }

        private void SetupMouse()
        {
            _mouse = new Mouse();

            var input = _glWindow.CreateInput();

            foreach (var mouse in input.Mice)
            {
                mouse.MouseDown += _mouse.ButtonDown;
                mouse.MouseUp += _mouse.ButtonUp;
                mouse.MouseMove += _mouse.MouseMove;
            }
        }

        private void SetupErrorCallback()
        {
            if (_debugCallback == null)
            {
                _debugCallback = DebugCallback;

                /*NOTE:
                 * This is here to help prevent an issue with an obscure System.ExecutionException from occurring.
                 * The garbage collector performs a collect on the delegate passed into _gl.DebugMesageCallback()
                 * without the native system knowing about it which causes this exception. The GC.KeepAlive()
                 * method tells the garbage collector to not collect the delegate to prevent this from happening.
                 */
                GC.KeepAlive(_debugCallback);

                GLInvoker.Enable(EnableCap.DebugOutput);
                GLInvoker.DebugMessageCallback(_debugCallback, Marshal.StringToHGlobalAnsi(string.Empty));
            }
        }

        private void DebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
        {
            var errorMessage = Marshal.PtrToStringAnsi(message);

            errorMessage += $"\n\tSrc: {source}";
            errorMessage += $"\n\tType: {type}";
            errorMessage += $"\n\tID: {id}";
            errorMessage += $"\n\tSeverity: {severity}";
            errorMessage += $"\n\tLength: {length}";
            errorMessage += $"\n\tUser Param: {Marshal.PtrToStringAnsi(userParam)}";

            if (severity != GLEnum.DebugSeverityNotification)
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
