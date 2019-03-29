using System;

namespace WpfGles
{
    public interface IGlesRenderer : IDisposable
    {
        void Render(bool force_redraw);
        void Resize(int width, int height);
    }
}