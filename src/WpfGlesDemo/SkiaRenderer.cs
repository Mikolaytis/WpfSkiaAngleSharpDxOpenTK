using System.IO;
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
            //GL.Clear(ClearBufferMask.StencilBufferBit  | ClearBufferMask.DepthBufferBit);
            //GL.Viewport(0,0, _renderTarget.Width, _renderTarget.Height);

            _surface.Canvas.Clear(SKColors.Blue);
            var img = _img ?? (_img = SKImage.FromEncodedData(File.ReadAllBytes("fail.png")));
            _surface.Canvas.RotateDegrees(1f, _renderTarget.Width / 2, _renderTarget.Height / 2);
            _surface.Canvas.DrawImage(img, new SKRect(
                _renderTarget.Width / 4,
                _renderTarget.Height / 4,
                _renderTarget.Width / 4 * 3,
                _renderTarget.Height / 4 * 3));
            _surface.Canvas.DrawCircle(500, 500, 500, new SKPaint()
            {
                Color = SKColors.Black,
                TextSize = 100,
                BlendMode = SKBlendMode.Clear,
                StrokeWidth = 10
            });
            _surface.Canvas.Save();
            _surface.Canvas.DrawText("HELLO", 100, 100, new SKPaint()
            {
                Color = SKColors.White,
                TextSize = 100
            });
            _surface.Canvas.Restore();
            _surface.Canvas.Flush();
            _surface.Canvas.DrawRect(0, 0, 300, 300, new SKPaint()
            {
                Color = SKColors.Black,
                TextSize = 100
            });
            _surface.Canvas.DrawLine(0,0,1000,1000, new SKPaint()
            {
                Color = SKColors.Black,
                StrokeWidth = 10,
                IsStroke = true
            });
            _surface.Canvas.DrawUrlAnnotation(new SKRect(100, 100, 100, 100), "what");
            //_surface.Canvas.DrawColor(SKColors.Red);
            //_surface.Canvas.Flush();
            
        }

        private GRContext _context;
        private SKSurface _surface;
        private GRBackendRenderTarget _renderTarget;
        public void Resize(int width, int height)
        {
            if (_context == null)
            {
                //var glInterface = GRGlInterface.AssembleAngleInterface(D3DAngleInterop._window_info,);
                var glInterface = GRGlInterface.CreateNativeAngleInterface();
                _context = GRContext.Create(GRBackend.OpenGL, glInterface);
            }
            var glInfo = new GRGlFramebufferInfo(
                fboId: 0,
                format: SKColorType.Rgba8888.ToGlSizedFormat());
            _renderTarget = new GRBackendRenderTarget(
                width: width,
                height: height,
                sampleCount: 8,
                stencilBits: 8,
                glInfo: glInfo);
            _surface = SKSurface.Create(
                _context, _renderTarget, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888);
        }
    }
}
