using System;
using OBSGreenScreen.VelaptorStuff;
using Silk.NET.OpenGL;
using GL = OBSGreenScreen.GLInvoker;

namespace OBSGreenScreen
{
    public class ShaderProgram
    {
        protected readonly IShaderLoaderService<uint> _shaderLoaderService;

        // TODO: Make use of this bool field
        private bool _shaderActive;

        public ShaderProgram(IShaderLoaderService<uint> shaderLoaderService)
        {
            _shaderLoaderService = shaderLoaderService;
        }

        public uint ShaderId { get; private set; }

        public void Use()
        {
            GLInvoker.UseProgram(ShaderId);
            _shaderActive = true;
        }

        public void Dispose()
        {
            GLInvoker.DeleteProgram(ShaderId);
        }

        public virtual void LoadShader(string name, uint batchSize)
        {
            GLInvoker.BeginGroup($"Load {name} Vertex Shader");

            var vertShaderSrc = _shaderLoaderService.LoadVertSource(name, new (string name, uint value)[] { ("BATCH_SIZE", batchSize) });
            var vertShaderId = GLInvoker.CreateShader(ShaderType.VertexShader);

            GLInvoker.LabelShader(vertShaderId, $"{name} Vertex Shader");

            GLInvoker.ShaderSource(vertShaderId, vertShaderSrc);
            GLInvoker.CompileShader(vertShaderId);

            //Checking the shader for compilation errors.
            var infoLog = GLInvoker.GetShaderInfoLog(vertShaderId);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling vertex shader '{name}'\n{infoLog}");
            }

            GLInvoker.EndGroup();

            GLInvoker.BeginGroup($"Load {name} Fragment Shader");

            var fragShaderSrc = _shaderLoaderService.LoadFragSource(name, new (string name, uint value)[] { ("BATCH_SIZE", batchSize) });
            var fragShaderId = GLInvoker.CreateShader(ShaderType.FragmentShader);

            GLInvoker.LabelShader(fragShaderId, $"{name} Fragment Shader");

            GLInvoker.ShaderSource(fragShaderId, fragShaderSrc);
            GLInvoker.CompileShader(fragShaderId);

            //Checking the shader for compilation errors.
            infoLog = GLInvoker.GetShaderInfoLog(fragShaderId);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling fragment shader '{name}'\n{infoLog}");
            }

            GLInvoker.EndGroup();

            CreateProgram(name, vertShaderId, fragShaderId);
            CleanShadersIfReady(name, vertShaderId, fragShaderId);
        }

        private void CreateProgram(string shaderName, uint vertShaderId, uint fragShaderId)
        {
            GLInvoker.BeginGroup($"Create {shaderName} Shader Program");

            //Combining the shaders under one shader program.
            ShaderId = GLInvoker.CreateProgram();

            GLInvoker.LabelShaderProgram(ShaderId, $"{shaderName} Shader Program");

            GLInvoker.AttachShader(ShaderId, vertShaderId);
            GLInvoker.AttachShader(ShaderId, fragShaderId);

            //Link and check for for errors.
            GLInvoker.LinkProgram(ShaderId);
            GLInvoker.GetProgram(ShaderId, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Error linking shader {GLInvoker.GetProgramInfoLog(ShaderId)}");
            }

            GLInvoker.EndGroup();
        }

        private void CleanShadersIfReady(string name, uint vertShaderId, uint fragShaderId)
        {
            GLInvoker.BeginGroup($"Clean Up {name} Vertex Shader");

            GLInvoker.DetachShader(ShaderId, vertShaderId);
            GLInvoker.DeleteShader(vertShaderId);

            GLInvoker.EndGroup();

            GLInvoker.BeginGroup($"Clean Up {name} Fragment Shader");

            //Delete the no longer useful individual shaders
            GLInvoker.DetachShader(ShaderId, fragShaderId);
            GLInvoker.DeleteShader(fragShaderId);

            GLInvoker.EndGroup();
        }
    }
}
