using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Platform;
using OpenTK.Platform.Egl;
using SharpDX.Direct3D9;

namespace WpfGles.Interop
{
    public interface ID3DAngleInterop
    {
        void EnsureContext();
        void MakeCurrent(IntPtr surface);
        IntPtr CreateOffscreenSurface(int width, int height);
        void DestroyOffscreenSurface(ref IntPtr surface);
        IntPtr GetD3DSharedHandleForSurface(IntPtr egl_surface, int width, int height);
    }

    internal class D3DAngleInterop : ID3DAngleInterop, IDisposable
    {
        private readonly Control _control;
        private readonly GraphicsContext _context;
        private readonly IAngleWindowInfo _window_info;
        private readonly D3D9Interop _d3d9_interop;
        private readonly Dpi _dpi;
        private bool _disposed;
        private Texture _texture;
        private IList<AngleImageSource> _sources = new List<AngleImageSource>();

        public D3DAngleInterop()
            : this(
                new GraphicsMode(32, 24),
                3, 0,
                GraphicsContextFlags.Embedded
                | GraphicsContextFlags.AngleD3D11
                )
        {
        }

        public D3DAngleInterop(GraphicsMode mode,
            int major, int minor, GraphicsContextFlags flags)
        {
            _d3d9_interop = new D3D9Interop();
            _dpi = new Dpi();

            _control = new Control();
            var win = OpenTK.Platform.Utilities.CreateWindowsWindowInfo(_control.Handle);
            _window_info = OpenTK.Platform.Utilities.CreateAngleWindowInfo(win);
            _context = new GraphicsContext(mode, _window_info, major, minor, flags);
            _context.MakeCurrent(_window_info);
            _context.LoadAll();
        }

        public IDpi Dpi => _dpi;

        private IGraphicsContext Context => _context;

        public void AddUser(AngleImageSource source)
        {
            _sources.Add(source);
        }

        public void RemoveUser(AngleImageSource source)
        {
            _sources.Remove(source);
        }

        ~D3DAngleInterop()
        {
            Dispose(true);
        }

        public void Dispose()
        {
           Dispose(false); 
        }

        public void Dispose(bool called_from_finalizer)
        {
            if (_disposed)
            {
                return;
            }
            if (!called_from_finalizer)
            {
                
            }
            // 
            _disposed = true;
        }

        public void EnsureContext()
        {
            _context.MakeCurrent(_window_info);
        }

        public void MakeCurrent(IntPtr surface)
        {
            _window_info.MakeCurrent(surface);
        }

        public IntPtr GetD3DSharedHandleForSurface(IntPtr egl_surface, int width, int height)
        {
            var ptr = _window_info.QuerySurfacePointer(egl_surface);
            _texture = _d3d9_interop.CreateNewSharedTexture(ptr, width, height);
            return _texture.GetSurfaceLevel(0).NativePointer;
        }

        public IntPtr CreateOffscreenSurface(int width, int height)
        {
            return _window_info.CreateSurface(width, height);
        }

        public void DestroyOffscreenSurface(ref IntPtr surface)
        {
            _window_info.DestroySurface(ref surface);
        }
    }
}