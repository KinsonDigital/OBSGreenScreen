using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace OBSGreenScreen
{
    public static class GLInvoker
    {
        private static Queue<string> glCallStack = new ();
        private static GL gl;

        public static string[] GLCallStack
        {
            get { return glCallStack.ToArray(); }
        }

        public static void SetGLObject(GL gl)
        {
            GLInvoker.gl = gl;
        }

        public static void BeginGroup(string name)
        {
            if (gl is null)
            {
                throw new NullReferenceException("The GL object is null");
            }

            AddToGLCallStack(nameof(BeginGroup));
            gl.PushDebugGroup(DebugSource.DebugSourceApplication, 100, (uint)name.Length, name);
        }

        public static void EndGroup()
        {
            if (gl is null)
            {
                throw new NullReferenceException("The GL object is null");
            }

            AddToGLCallStack(nameof(EndGroup));
            gl.PopDebugGroup();
        }

        public static void LabelShader(uint shaderId, string label)
        {
            AddToGLCallStack(nameof(LabelShader));
            gl.ObjectLabel(ObjectIdentifier.Shader, shaderId, (uint) label.Length, label);
        }

        public static void LabelShaderProgram(uint shaderId, string label)
        {
            AddToGLCallStack(nameof(LabelShaderProgram));
            gl.ObjectLabel(ObjectIdentifier.Program, shaderId, (uint) label.Length, label);
        }

        public static void LabelVertexArray(uint vertexArrayId, string label, Action bindVertexArrayObj)
        {
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            var newLabel = $"{label} VAO";

            bindVertexArrayObj();
            AddToGLCallStack(nameof(LabelVertexArray));
            gl.ObjectLabel(ObjectIdentifier.VertexArray, vertexArrayId, (uint)newLabel.Length, newLabel);
        }

        public static void LabelBuffer(uint bufferId, string label, BufferType bufferType, Action bindBufferObj)
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
            AddToGLCallStack(nameof(LabelBuffer));
            gl.ObjectLabel(ObjectIdentifier.Buffer, bufferId, (uint)newLabel.Length, newLabel);
        }

        public static void DebugMessageCallback(DebugProc callback, IntPtr userparam)
        {
            AddToGLCallStack(nameof(DebugMessageCallback));
            gl.DebugMessageCallback(callback, userparam);
        }

        public static uint CreateProgram()
        {
            AddToGLCallStack(nameof(CreateProgram));
            return gl.CreateProgram();
        }

        public static void UseProgram(uint program)
        {
            AddToGLCallStack(nameof(UseProgram));
            gl.UseProgram(program);
        }

        public static void DeleteProgram(uint program)
        {
            AddToGLCallStack(nameof(DeleteProgram));
            gl.DeleteProgram(program);
        }

        public static uint CreateShader(ShaderType type)
        {
            AddToGLCallStack(nameof(CreateShader));
            return gl.CreateShader(type);
        }

        public static void ShaderSource(uint shader, string @string)
        {
            AddToGLCallStack(nameof(ShaderSource));
            gl.ShaderSource(shader, @string);
        }

        public static void CompileShader(uint shader)
        {
            AddToGLCallStack(nameof(CompileShader));
            gl.CompileShader(shader);
        }

        public static void AttachShader(uint program, uint shader)
        {
            AddToGLCallStack(nameof(AttachShader));
            gl.AttachShader(program, shader);
        }

        public static string GetShaderInfoLog(uint shader)
        {
            AddToGLCallStack(nameof(GetShaderInfoLog));
            return gl.GetShaderInfoLog(shader);
        }

        public static string GetProgramInfoLog(uint program)
        {
            AddToGLCallStack(nameof(GetProgramInfoLog));
            return gl.GetProgramInfoLog(program);
        }

        public static void LinkProgram(uint program)
        {
            AddToGLCallStack(nameof(LinkProgram));
            gl.LinkProgram(program);
        }

        public static void GetProgram(uint program, GLEnum pname, out int status)
        {
            AddToGLCallStack(nameof(GetProgram));
            gl.GetProgram(program, pname, out status);
        }

        public static void Enable(EnableCap cap)
        {
            AddToGLCallStack(nameof(Enable));
            gl.Enable(cap);
        }

        public static void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
        {
            AddToGLCallStack(nameof(BlendFunc));
            gl.BlendFunc(sfactor, dfactor);
        }

        public static void GetInteger(GetPName pname, Span<int> data)
        {
            AddToGLCallStack(nameof(GetInteger));
            gl.GetInteger(pname, data);
        }

        public static void Viewport(int x, int y, uint width, uint height)
        {
            AddToGLCallStack(nameof(Viewport));
            gl.Viewport(x, y, width, height);
        }

        public static void SetViewPortSize(uint width, uint height)
        {
            /*
             * [0] = X
             * [1] = Y
             * [3] = Width
             * [4] = Height
             */
            var data = new int[4];

            AddToGLCallStack(nameof(SetViewPortSize));
            GetInteger(GetPName.Viewport, data);
            Viewport(data[0], data[1], width, height);
        }

        public static unsafe void VertexAttribPointer(
            uint index,
            int size,
            VertexAttribPointerType type,
            bool normalized,
            uint stride,
            void* pointer)
        {
            AddToGLCallStack(nameof(VertexAttribPointer));
            gl.VertexAttribPointer(index, size, type, normalized, stride, pointer);
        }

        public static void EnableVertexAttribArray(uint index)
        {
            AddToGLCallStack(nameof(EnableVertexAttribArray));
            gl.EnableVertexAttribArray(index);
        }

        public static void BufferSubData(BufferTargetARB target, nint offset, nuint totalBytes, float[] data)
        {
            AddToGLCallStack(nameof(BufferSubData));
            unsafe
            {
                fixed(void* dataPtr = data)
                {
                    gl.BufferSubData(BufferTargetARB.ArrayBuffer, offset, totalBytes, dataPtr);
                }
            }
        }

        public static void BufferData(BufferTargetARB target, nuint size, ReadOnlySpan<float> data, BufferUsageARB usage)
        {
            AddToGLCallStack(nameof(BufferData));
            gl.BufferData(target, size, data, usage);
        }

        public static void BufferData(BufferTargetARB target, nuint size, ReadOnlySpan<uint> data, BufferUsageARB usage)
        {
            AddToGLCallStack(nameof(BufferData));
            gl.BufferData(target, size, data, usage);
        }

        public static uint GenVertexArray()
        {
            AddToGLCallStack(nameof(GenVertexArray));
            return gl.GenVertexArray();
        }

        public static uint GenBuffer()
        {
            AddToGLCallStack(nameof(GenBuffer));
            return gl.GenBuffer();
        }

        public static void BindVertexArray(uint array)
        {
            AddToGLCallStack($"{(array <= 0 ? "Unb" : "B")}ind VAO");
            gl.BindVertexArray(array);
        }

        public static void BindBuffer(BufferTargetARB target, uint buffer)
        {
            var firstSection = $"{(buffer <= 0u ? "Un" : string.Empty)}";
            var secondSection = target == BufferTargetARB.ArrayBuffer ? "VBO" : "EBO";

            AddToGLCallStack($"{firstSection}Bind {secondSection}");
            gl.BindBuffer(target, buffer);
        }

        public static void GetFloat(GetPName pname, Span<float> data)
        {
            AddToGLCallStack(nameof(GetFloat));
            gl.GetFloat(pname, data);
        }

        public static void ClearColor(float red, float green, float blue, float alpha)
        {
            AddToGLCallStack(nameof(ClearColor));
            gl.ClearColor(red, green, blue, alpha);
        }

        public static unsafe void DrawElements(PrimitiveType mode, uint count, DrawElementsType type, void* indices)
        {
            AddToGLCallStack(nameof(DrawElements));
            gl.DrawElements(mode, count, type, indices);
        }

        public static void Clear(uint mask)
        {
            AddToGLCallStack(nameof(Clear));
            gl.Clear(mask);
        }

        public static void DeleteVertexArray(uint arrays)
        {
            AddToGLCallStack(nameof(DeleteVertexArray));
            gl.DeleteVertexArray(arrays);
        }

        public static void DeleteBuffer(uint buffer)
        {
            AddToGLCallStack(nameof(DeleteBuffer));
            gl.DeleteBuffer(buffer);
        }

        public static void DetachShader(uint program, uint shader)
        {
            AddToGLCallStack(nameof(DetachShader));
            gl.DetachShader(program, shader);
        }

        public static void DeleteShader(uint shader)
        {
            AddToGLCallStack(nameof(DeleteShader));
            gl.DeleteShader(shader);
        }

        private static void AddToGLCallStack(string glFunctionName)
        {
            glCallStack.Enqueue(glFunctionName);

            if (glCallStack.Count >= 200)
            {
                glCallStack.Dequeue();
            }
        }
    }
}
