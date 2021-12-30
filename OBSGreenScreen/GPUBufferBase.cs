using System;
using System.Drawing;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    public abstract class GPUBufferBase<T> : IDisposable
        where T : struct
    {
        private string _bufferName;
        private uint _vao; // Vertex Array Object
        private uint _vbo; // Vertex Buffer Object
        private uint _ebo; // Element Buffer Object
        private uint[] _indices;
        private bool _isVAOBound;
        private bool _isDisposed;

        ~GPUBufferBase() => Dispose();

        protected uint BatchSize { get; private set; }

        public SizeF ViewPortSize { get; set; }

        public virtual void Init()
        {
            ProcessCustomAttributes();

            // TODO: For IoC reasons, instead of using a ctor param for buffer name, use a custom attribute

            // Generate the VAO and VBO with only 1 object each
            _vao = GLInvoker.GenVertexArray();
            GLInvoker.LabelVertexArray(_vao, _bufferName, BindVAO);

            _vbo = GLInvoker.GenBuffer();
            GLInvoker.LabelBuffer(_vbo, _bufferName, BufferType.VertexBufferObject, BindVBO);

            _ebo = GLInvoker.GenBuffer();
            GLInvoker.LabelBuffer(_ebo, _bufferName, BufferType.IndexArrayObject, BindEBO);

            GLInvoker.BeginGroup($"Setup {_bufferName} Data");
            GLInvoker.BeginGroup($"Upload {_bufferName} Vertex Data");

            var vertBufferData = GenerateData();
            var vertData = new ReadOnlySpan<float>(vertBufferData);
            var totalBytes = (uint)vertBufferData.Length * sizeof(float);

            GLInvoker.BufferData(BufferTargetARB.ArrayBuffer, totalBytes, vertData, BufferUsageARB.DynamicDraw);

            GLInvoker.EndGroup();

            GLInvoker.BeginGroup($"Upload {_bufferName} Indices Data");

            _indices = GenerateIndices();

            var indicesData = new ReadOnlySpan<uint>(_indices);

            // Configure the Vertex Attribute so that OpenGL knows how to read the VBO
            GLInvoker.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(sizeof(uint) * _indices.Length), indicesData, BufferUsageARB.StaticDraw);
            GLInvoker.EndGroup();

            SetupVAO();

            UnbindVBO();
            UnbindVAO();
            UnbindEBO();
            GLInvoker.EndGroup();
        }

        public void UpdateData(T data, uint batchIndex)
        {
            PrepareForUse();
            UpdateVertexData(data, batchIndex);
        }

        protected abstract void UpdateVertexData(T data, uint batchIndex);

        protected abstract void PrepareForUse();

        protected abstract float[] GenerateData();

        protected abstract void SetupVAO();

        protected abstract uint[] GenerateIndices();

        protected void BindVBO() => GLInvoker.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        protected void UnbindVBO() => GLInvoker.BindBuffer(BufferTargetARB.ArrayBuffer, 0); // Unbind the VBO

        protected void BindEBO() => GLInvoker.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        /// <summary>
        /// NOTE: Make sure to unbind AFTER you unbind the VAO.  This is because the EBO is stored
        /// inside of the VAO.  Unbinding the EBO before unbinding, (or without unbinding the VAO),
        /// you are telling OpenGL that you don't want your VAO to use the EBO.
        /// </summary>
        protected void UnbindEBO()
        {
            if (_isVAOBound)
            {
                throw new Exception("Cannot unbind the EBO before unbinding the VAO.");
            }

            GLInvoker.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        protected void BindVAO()
        {
            GLInvoker.BindVertexArray(_vao);
            _isVAOBound = true;
        }

        protected void UnbindVAO()
        {
            GLInvoker.BindVertexArray(0); // Unbind the VAO
            _isVAOBound = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            GLInvoker.DeleteVertexArray(_vao);
            GLInvoker.DeleteBuffer(_vbo);
            GLInvoker.DeleteBuffer(_ebo);

            _isDisposed = true;

            GC.SuppressFinalize(this);
        }

        private void ProcessCustomAttributes()
        {
            Attribute[] attributes;
            var currentType = GetType();

            if (currentType == typeof(LineGPUBuffer))
            {
                attributes = Attribute.GetCustomAttributes(typeof(LineGPUBuffer));
            }
            else if (currentType == typeof(RectGPUBuffer))
            {
                attributes = Attribute.GetCustomAttributes(typeof(RectGPUBuffer));
            }
            else if (currentType == typeof(EllipseGPUBuffer))
            {
                attributes = Attribute.GetCustomAttributes(typeof(EllipseGPUBuffer));
            }
            else
            {
                throw new Exception("The GPU Buffer is unrecognized");
            }

            if (attributes.Length <= 0)
            {
                BatchSize = 100;
                _bufferName = "UNKNOWN BUFFER";
                return;
            }

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case SpriteBatchSizeAttribute sizeAttribute:
                        BatchSize = sizeAttribute.BatchSize;
                        break;
                    case GPUBufferNameAttribute nameAttribute:
                        _bufferName = nameAttribute.Name;
                        break;
                }
            }
        }
    }
}
