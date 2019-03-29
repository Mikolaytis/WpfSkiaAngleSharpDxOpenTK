using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media.Imaging;
using OpenTK.Graphics.ES30;

namespace WpfGles
{
    public class DemoRenderer : IGlesRenderer
    {
        private readonly Stopwatch _stopwatch;
        private int _height;
        private int _index_count;
        private int _indices;
        private bool _initialized;
        private int _program;
        private int _rotation_angle_location;
        private int _sampler_location;
        private int _tex_height;
        private int _tex_width;
        private int _texture;
        private byte[] _texture_pixels;
        private int _vertices;
        private int _width;
        private bool _disposed;

        public DemoRenderer()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public BitmapImage Texture
        {
            set
            {
                var bitmap = value;
                _tex_width = bitmap.PixelWidth;
                _tex_height = bitmap.PixelHeight;
                var stride = _tex_width*4;
                var size = _tex_height*stride;
                _texture_pixels = new byte[size];
                bitmap.CopyPixels(_texture_pixels, stride, 0);
            }
        }

        public void Render(bool force_redraw)
        {
            if (!_initialized)
            {
                Initialize();
            }
            if (_texture == 0)
            {
                GL.ClearColor(0.0f, 1.0f, 1.0f, 0.5f);
            }
            else
            {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            }
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Viewport(0, 0, _width, _height);

            GL.UseProgram(_program);

            if (_texture != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, _texture);
                GL.Uniform1(_sampler_location, 0);
                GL.Uniform1(_rotation_angle_location, _stopwatch.ElapsedMilliseconds*0.002f);
            }
            else
            {
                GL.Uniform1(_rotation_angle_location, _stopwatch.ElapsedMilliseconds*0.02f);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertices);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 20, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 20, 12);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indices);

            GL.DrawElements(PrimitiveType.TriangleStrip, _index_count, DrawElementsType.UnsignedByte, IntPtr.Zero);

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.UseProgram(0);

            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception();
            }
        }

        public void Resize(int width, int height)
        {
            _width = width;
            _height = height;
        }

        private void Initialize()
        {
            SetupVertices();
            SetupIndices();
            SetupShaders();
            SetupTexture();
            _initialized = true;
        }

        private void SetupVertices()
        {
            _vertices = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertices);

            var z0 = 0f;
            var min = -0.5f;
            var max = 0.5f;
            float[] vertex_data =
            {
                min, min, z0,
                0, 0,
                max, min, z0,
                1, 0,
                min, max, z0,
                0, 1,
                max, max, z0,
                1, 1
            };
            GL.BufferData(BufferTarget.ArrayBuffer,
                new IntPtr(vertex_data.Length*sizeof (float)),
                vertex_data,
                BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void SetupIndices()
        {
            _indices = GL.GenBuffer();
            byte[] index_data =
            {
                0, 1, 2, 3
            };
            _index_count = index_data.Length;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indices);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(index_data.Length*sizeof (byte)),
                index_data,
                BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private void SetupShaders()
        {
            var asm = Assembly.GetExecutingAssembly();
            var vs_src = LoadEmbedded.TextFile(asm, "WpfGlesDemo.demo.vert");

            var status = new int[1];

            const int GL_TRUE = 1;

            _program = GL.CreateProgram();
            var vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vs_src);
            GL.CompileShader(vs);

            GL.GetShader(vs, ShaderParameter.CompileStatus, status);
            if (status[0] != GL_TRUE)
            {
                var error = GL.GetShaderInfoLog(vs);
                throw new Exception(error);
            }

            var fs_src = LoadEmbedded.TextFile(asm, "WpfGlesDemo.demo.frag");
            var fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fs_src);
            GL.CompileShader(fs);

            GL.GetShader(fs, ShaderParameter.CompileStatus, status);
            if (status[0] != GL_TRUE)
            {
                var error = GL.GetShaderInfoLog(fs);
                throw new Exception(error);
            }

            GL.AttachShader(_program, vs);
            GL.AttachShader(_program, fs);

            GL.BindAttribLocation(_program, 0, "in_position");
            GL.BindAttribLocation(_program, 1, "in_tex_coord");

            GL.LinkProgram(_program);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            GL.UseProgram(_program);
            _sampler_location = GL.GetUniformLocation(_program, "tex0");
            _rotation_angle_location = GL.GetUniformLocation(_program, "rotation_angle");


            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, status);
            if (status[0] != GL_TRUE)
            {
                var error = GL.GetProgramInfoLog(_program);
                throw new Exception(error);
            }
        }

        private void SetupTexture()
        {
            if (_texture_pixels == null)
            {
                return;
            }
            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba,
                _tex_width, _tex_height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, _texture_pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                Convert.ToInt32(TextureWrapMode.Repeat));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                Convert.ToInt32(TextureWrapMode.Repeat));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                Convert.ToInt32(TextureMinFilter.Linear));
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                Convert.ToInt32(TextureMagFilter.Linear));
            GL.BindTexture(TextureTarget.Texture2D, 0);
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception();
            }
        }

        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool called_from_finalizer)
        {
            if (_disposed)
            {
                return;
            }
            if (!called_from_finalizer)
            {
                // dispose managed
            }

            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vertices);
            GL.DeleteBuffer(_indices);

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~DemoRenderer()
        {
           Dispose(true); 
        }
    }
}