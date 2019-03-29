using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using WpfGles;

namespace WpfGlesDemo
{
    public class SkiaRenderer : IGlesRenderer
    {
        public void Dispose()
        {

        }

        private SKImage _img;
        public void Render(bool force_redraw)
        {
            _surface.Canvas.DrawColor(SKColors.Blue);
            var img = _img ?? (_img = SKImage.FromEncodedData(File.ReadAllBytes("fail.png")));
            _surface.Canvas.RotateDegrees(1f, _renderTarget.Width / 2, _renderTarget.Height / 2);
            _surface.Canvas.DrawImage(img, new SKRect(
                _renderTarget.Width / 4, 
                _renderTarget.Height / 4, 
                _renderTarget.Width / 4 * 3, 
                _renderTarget.Height / 4 * 3));
            _surface.Canvas.Flush();
        }

        private GRContext _context;
        private SKSurface _surface;
        private GRBackendRenderTarget _renderTarget;
        public void Resize(int width, int height)
        {
            if (_context == null)
            {
                var glInterface = GRGlInterface.CreateNativeAngleInterface();
                _context = GRContext.Create(GRBackend.OpenGL, glInterface);
            }
            var glInfo = new GRGlFramebufferInfo(
                fboId: 0,
                format: SKColorType.Rgba8888.ToGlSizedFormat());
            _renderTarget = new GRBackendRenderTarget(
                width: width,
                height: height,
                sampleCount: 0,
                stencilBits: 0,
                glInfo: glInfo);
            _surface = SKSurface.Create(
                _context, _renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
        }
    }
}
