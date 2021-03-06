﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGles
{
    public class GlesImage : Image
    {
        private static BitmapImage _dummy;
        private bool _rendering_enabled;
        private bool _reset_back_buffer;

        public GlesImage()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            _rendering_enabled = false;
            LoadDummyImage();
        }

        public IGlesRenderer GlesRenderer { get; set; }

        private IAngleImageSource ImageSource { get; set; }

        private void LoadDummyImage()
        {
            if (_dummy != null)
            {
                Source = _dummy;
                return;
            }
            
            var img = File.ReadAllBytes("fail.png");

            _dummy = new BitmapImage();

            using (var stream = new MemoryStream(img))
            {
                _dummy.BeginInit();
                _dummy.CacheOption = BitmapCacheOption.OnLoad;
                _dummy.StreamSource = stream;
                _dummy.EndInit();
                stream.Close();
            }

            Source = _dummy;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs event_args)
        {
            _reset_back_buffer = true;
            StartRendering();
        }

        private void OnUnloaded(object sender, RoutedEventArgs event_args)
        {
            StopRendering();
            Source = null;
        }

        private void StartRendering()
        {
            if (_rendering_enabled)
            {
                return;
            }

            if (ImageSource == null)
            {
                ImageSource = new AngleImageSource();
                ImageSource.SetupDpi(this);
            }

            CompositionTarget.Rendering += OnRendering;
            _rendering_enabled = true;

        }

        private void StopRendering()
        {
            if (!_rendering_enabled)
            {
                return;
            }

            CompositionTarget.Rendering -= OnRendering;
            _rendering_enabled = false;
        }

        private void OnRendering(object sender, EventArgs event_args)
        {
            if (!_rendering_enabled)
            {
                return;
            }

            bool force_redraw = false;

            if (_reset_back_buffer)
            {
                var w = ActualWidth;
                var h = ActualHeight;
                if (w > 0 && h > 0)
                {
                    _reset_back_buffer = false;
                    // This is required so we don't get blurry render images
                    // when running on systems with non-default dpi.
                    var ww = (int) Math.Round(w * ImageSource.Dpi.ScaleX);
                    var hh = (int) Math.Round(h * ImageSource.Dpi.ScaleY);
                    ImageSource.Resize(ww, hh);
                    Source = ImageSource.Source;
                    force_redraw = true;
                    GlesRenderer?.Resize(ww, hh);
                }
                else
                {
                    Source = _dummy;
                }
            }

            if (GlesRenderer != null)
            {
                ImageSource.PreRender();
                GlesRenderer.Render(force_redraw);
                ImageSource.PostRender();
            }
            else
            {
                ImageSource.DummyRender();
            }
        }
        
        protected override void OnRenderSizeChanged(SizeChangedInfo size_info)
        {
            _reset_back_buffer = true;

            var ns = size_info.NewSize;
            var width = (int) ns.Width;
            var height = (int) ns.Height;
            if (height > 0 && width > 0)
            {
                StartRendering();
            }
            else
            {
                StopRendering();
            }

            base.OnRenderSizeChanged(size_info);
        }
    }
}
