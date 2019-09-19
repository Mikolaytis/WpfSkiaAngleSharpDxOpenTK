using System.Windows;

namespace WpfGlesDemo
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            First.GlesRenderer = new SkiaRenderer();
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            First.Opacity = e.NewValue;
        }
    }
}