using System;
using SharpDX.Direct3D9;

namespace WpfGles.Interop
{
    public class D3D9Interop
    {
        private bool _disposed;
        private Texture _texture;
        private readonly Direct3DEx _d3d_ex;
        private readonly DeviceEx _device_ex;

        public D3D9Interop()
        {
            _d3d_ex = new Direct3DEx();
            _device_ex = MakeDevice();
        }

        public IntPtr Pointer => _texture.GetSurfaceLevel(0).NativePointer;

        /// <summary>
        ///     Creates a new Direct3D9 Ex device, required for efficient
        ///     hardware-accelerated in Windows Vista and later.
        /// </summary>
        /// <returns></returns>
        private DeviceEx MakeDevice()
        {
            var present_params = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                PresentationInterval = PresentInterval.Default,

                // The device back buffer is not used.
                BackBufferFormat = Format.Unknown,
                BackBufferWidth = 1,
                BackBufferHeight = 1,

                // Use dummy window handle.
                DeviceWindowHandle = IntPtr.Zero
            };

            return new DeviceEx(_d3d_ex, 0, 
                DeviceType.Hardware, 
                IntPtr.Zero, 
                CreateFlags.HardwareVertexProcessing
                | CreateFlags.Multithreaded
                | CreateFlags.FpuPreserve,
                present_params);
        }

        /// <summary>
        ///     Creates a new Direct3D9 texture that uses the same memory as the
        ///     passed in DirectX11-texture.
        /// </summary>
        public Texture CreateNewSharedTexture(IntPtr shared_handle, int width, int height)
        {
            ThrowIfDisposed();

            if (shared_handle == IntPtr.Zero)
            {
                throw new ArgumentException(
                    "Unable to access resource. The texture needs to be created as a shared resource.", "render_target");
            }

            const Format format = Format.A8R8G8B8;

            _texture = new Texture(_device_ex,
                width,
                height,
                1, Usage.RenderTarget, format,
                Pool.Default, ref shared_handle);
            return _texture;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        ~D3D9Interop()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose_managed)
        {
            if (_disposed)
                return;

            if (dispose_managed)
            {
                _texture?.Dispose();
                _device_ex?.Dispose();
                _d3d_ex?.Dispose();
            }
            _disposed = true;
        }
    }
}