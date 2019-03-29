using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace WpfGles.Interop
{
    public class D3D9Interop
    {
        private bool _disposed;
        private Texture2D _texture;
        private readonly Device _device_ex;

        public D3D9Interop()
        {
            _device_ex = MakeDevice();
        }
        

        /// <summary>
        ///     Creates a new Direct3D9 Ex device, required for efficient
        ///     hardware-accelerated in Windows Vista and later.
        /// </summary>
        /// <returns></returns>
        private Device MakeDevice()
        {
            return new Device(DriverType.Hardware, DeviceCreationFlags.None, FeatureLevel.Level_11_1);
        }

        /// <summary>
        ///     Creates a new Direct3D9 texture that uses the same memory as the
        ///     passed in DirectX11-texture.
        /// </summary>
        public Texture2D CreateNewSharedTexture(IntPtr shared_handle, int width, int height)
        {
            ThrowIfDisposed();

            if (shared_handle == IntPtr.Zero)
            {
                throw new ArgumentException(
                    "Unable to access resource. The texture needs to be created as a shared resource.", "render_target");
            }
            
            _texture = new Texture2D(shared_handle);
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
            }
            _disposed = true;
        }
    }
}