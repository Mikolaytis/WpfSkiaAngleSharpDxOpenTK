using System.Windows;
using System.Windows.Media;
using WpfGles.Interop;

namespace WpfGles
{
    public interface IReadOnlyDpi
    {
        double ScaleX { get; }
        double ScaleY { get; }
        double DpiX { get; }
        double DpiY { get; }
    }

    public interface IDpi : IReadOnlyDpi
    {
        void SetVisual(Visual v);
    }

    public class Dpi : IDpi
    {
        public void SetVisual(Visual v)
        {
            var source = PresentationSource.FromVisual(v);

            if (source?.CompositionTarget == null)
            {
                throw new AngleInteropException("Cannot determine dpi.");
            }

            var m = source.CompositionTarget.TransformToDevice;

            ScaleX = m.M11;
            ScaleY = m.M22;
            DpiX = 96.0 * ScaleX;
            DpiY = 96.0 * ScaleY;
        }

        public double ScaleX { get; private set; } = 1.0;

        public double ScaleY { get; private set; } = 1.0;

        public double DpiX { get; private set; } = 96.0;

        public double DpiY { get; private set; } = 96.0;
    }
}
