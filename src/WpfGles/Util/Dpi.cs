using System.Windows;
using System.Windows.Media;
using OpenTK.Platform.Egl;
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
        private double _dpi_x = 96.0;
        private double _dpi_y = 96.0;
        private double _scale_x = 1.0;
        private double _scale_y = 1.0;

        public void SetVisual(Visual v)
        {
            var source = PresentationSource.FromVisual(v);

            if (source == null || source.CompositionTarget == null)
            {
                throw new AngleInteropException("Cannot determine dpi.");
            }

            var m = source.CompositionTarget.TransformToDevice;

            _scale_x = m.M11;
            _scale_y = m.M22;
            _dpi_x = 96.0 * _scale_x;
            _dpi_y = 96.0 * _scale_y;
        }

        public double ScaleX
        {
            get { return _scale_x; }
        }
        public double ScaleY
        {
            get { return _scale_y; }
        }
        public double DpiX
        {
            get { return _dpi_x; }
        }
        public double DpiY
        {
            get { return _dpi_y; }
        }
    }
}
