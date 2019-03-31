using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using OpenTK.Graphics.ES20;
using WpfGles.Interop;

namespace WpfGles
{
    public interface IAngleImageSource : IDisposable
    {
        ImageSource Source { get; }
        IReadOnlyDpi Dpi { get; }
        void DummyRender();
        void PreRender();
        void PostRender();
        void Resize(int width, int height);
        void InvalidateD3DImage();
        void SetupDpi(Visual v);
    }

    public class AngleImageSource : IAngleImageSource
    {
        private static D3DAngleInterop _shared;
        private readonly int _height;
        private readonly int _width;
        private float _color;
        private IntPtr _d3d_surface;
        private bool _disposed;
        private IntPtr _egl_surface;
        private D3DImage _image;

        public AngleImageSource()
        {
            if (_shared == null)
            {
                _shared = new D3DAngleInterop();
                _color = 0.5f;
            }
            _shared.AddUser(this);

            _width = _height = 0;
        }

        public IGlesRenderer Renderer { get; set; }

        public void Dispose()
        {
            Dispose(false);
        }

        public ImageSource Source => _image;

        public void PreRender()
        {
            _shared.MakeCurrent(_egl_surface);
        }

        public void PostRender()
        {
            GL.Flush();
            InvalidateD3DImage();
        }

        public void Resize(int width, int height)
        {
            if (_image == null)
            {
                CreateD3DImage();
            }

            if (width == 0 || height == 0)
            {
                width = 2;
                height = 2;
            }

            if (width == _width && height == _height && HasSurface())
            {
                return;
            }

            _shared.EnsureContext();

            if (_egl_surface != IntPtr.Zero)
            {
                _shared.DestroyOffscreenSurface(ref _egl_surface);
            }

            _egl_surface = _shared.CreateOffscreenSurface(width, height);
            _shared.MakeCurrent(_egl_surface);

            _d3d_surface = _shared.GetD3DSharedHandleForSurface(_egl_surface, width, height);
            if (_d3d_surface != IntPtr.Zero)
            {
                SetSharedSurfaceToD3DImage();
            }

            Renderer?.Resize(width, height);
        }
        
        public IReadOnlyDpi Dpi => _shared.Dpi;

        public void SetupDpi(Visual v)
        {
            _shared.Dpi.SetVisual(v);
        }

        public void DummyRender()
        {
            PreRender();
            GL.ClearColor(_color, _image.PixelWidth/255.0f, _image.PixelHeight/255.0f, 1.0f);
            _color += 0.0125f;
            if (_color > 1.0f)
            {
                _color = 0.0f;
            }
            GL.Clear(ClearBufferMask.ColorBufferBit);
            PostRender();
        }

        private void SetSharedSurfaceToD3DImage()
        {
            _image.Lock();
            _image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _d3d_surface);
            _image.AddDirtyRect(new Int32Rect(0, 0, _image.PixelWidth, _image.PixelHeight));
            _image.Unlock();
        }

        ~AngleImageSource()
        {
            Dispose(true);
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
                _shared.RemoveUser(this);
            }
            // dispose unmamanged

            _disposed = true;
        }

        public void InvalidateD3DImage()
        {
            _image.Lock();
            _image.AddDirtyRect(new Int32Rect(0, 0, _image.PixelWidth, _image.PixelHeight));
            _image.Unlock();
        }

        private bool HasSurface()
        {
            return _d3d_surface != IntPtr.Zero;
        }

        private void CreateD3DImage()
        {
            if (_image != null)
            {
                return;
            }

            var dpi = _shared.Dpi;
            _image = new D3DImage(dpi.DpiX, dpi.DpiY);
        }
    }
}