/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2015  Thomas Braun, Jens Klingen, Robin Krom
 * 
 * For more information see: http://getgreenshot.org/
 * The Greenshot project is hosted on Sourceforge: http://sourceforge.net/projects/greenshot/
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Greenshot.Addons.Core.Enums;
using Greenshot.Core.Encoders;

namespace Greenshot.Core.Extensions {
	public static class WpfExtensions {

		/// <summary>
		/// Copy the rect from source to target 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="source"></param>
		/// <param name="rect"></param>
		public static void CopyPixels(this WriteableBitmap target, BitmapSource source, Rect rect) {
			// Calculate stride of source
			int stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);

			// Create data array to hold source pixel data
			byte[] data = new byte[stride * source.PixelHeight];

			// Copy source image pixels to the data array
			source.CopyPixels(data, stride, 0);

			// Write the pixel data to the WriteableBitmap.
			target.WritePixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), data, stride, 0);
		}

        /// <summary>
        /// Make a BitmapSource (actually a RenderTargetBitmap) from an ImageSource, e.g. DrawingImage
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns>BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this ImageSource imageSource) {
			return imageSource.ToBitmapSource((int)(imageSource.Width + 0.5), (int)(imageSource.Height + 0.5));
		}

		/// <summary>
		/// Make a BitmapSource (actually a RenderTargetBitmap) from an ImageSource, e.g. DrawingImage
		/// </summary>
		/// <param name="imageSource"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>BitmapSource</returns>
		public static BitmapSource ToBitmapSource(this ImageSource imageSource, int width, int height) {
			var renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
		    var image = new Image
		    {
		        Source = imageSource
		    };
		    image.Arrange(new Rect(0, 0, width, height));
			image.UpdateLayout();
			renderTargetBitmap.Render(ModifyToDrawingVisual(image, width, height));
			return renderTargetBitmap;
		}

		/// <summary>
		/// Convert an ImageSource to a bitmap, also works for DrawingImage (like the one for SVG)
		/// </summary>
		/// <param name="imageSource">ImageSource</param>
		/// <returns>Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap(this ImageSource imageSource, PixelFormat pixelFormat = default(PixelFormat)) {
			return imageSource.ToBitmap((int)(imageSource.Width + 0.5), (int)(imageSource.Height + 0.5), pixelFormat);
		}

		/// <summary>
		/// Convert a BitmapSource to a System.Drawing.Icon
		/// </summary>
		/// <param name="bitmapSource">BitmapSource to render in the icon</param>
		/// <param name="quality">0-100</param>
		/// <returns>System.Drawing.Icon</returns>
		public static System.Drawing.Icon ToIcon(this BitmapSource bitmapSource, int quality) {
			IconBitmapEncoder encoder = new IconBitmapEncoder();

			if (quality >= 90) {
				BitmapSource bmp256 = IconBitmapEncoder.GetResized(bitmapSource, 256);
				encoder.Frames.Add(BitmapFrame.Create(bmp256));
			}

			if (quality >= 80) {
				BitmapSource bmp128 = IconBitmapEncoder.GetResized(bitmapSource, 128);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp128)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp128)));
			}

			if (quality >= 70) {
				BitmapSource bmp96 = IconBitmapEncoder.GetResized(bitmapSource, 96);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp96)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp96)));
			}

			if (quality >= 60) {
				BitmapSource bmp72 = IconBitmapEncoder.GetResized(bitmapSource, 72);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp72)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp72)));
			}

			if (quality >= 50) {
				BitmapSource bmp64 = IconBitmapEncoder.GetResized(bitmapSource, 64);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp64)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp64)));
			}

			if (quality >= 40) {
				BitmapSource bmp48 = IconBitmapEncoder.GetResized(bitmapSource, 48);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp48)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp48)));
			}

			if (quality >= 30) {
				BitmapSource bmp32 = IconBitmapEncoder.GetResized(bitmapSource, 32);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp32)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp32)));
			}

			if (quality >= 20) {
				BitmapSource bmp24 = IconBitmapEncoder.GetResized(bitmapSource, 24);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp24)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp24)));
			}

			if (quality >= 0) {
				BitmapSource bmp16 = IconBitmapEncoder.GetResized(bitmapSource, 16);
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get24plus8BitImage(bmp16)));
				encoder.Frames.Add(BitmapFrame.Create(IconBitmapEncoder.Get8BitImage(bmp16)));
			}
			using (var stream = new MemoryStream()) {
				encoder.Save(stream);
				stream.Seek(0, SeekOrigin.Begin);
				return new System.Drawing.Icon(stream);
			}
		}

		/// <summary>
		/// Convert an ImageSource to a bitmap, also works for DrawingImage (like the one for SVG)
		/// </summary>
		/// <param name="imageSource">ImageSource</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap(this ImageSource imageSource, int width, int height, PixelFormat pixelFormat = default(PixelFormat)) {
			if (imageSource == null) {
				return null;
			}

		    var image = new Image
		    {
		        Source = imageSource
		    };
		    image.Arrange(new Rect(0, 0, width, height));
			image.UpdateLayout();
			return image.ToBitmap(width, height, pixelFormat);
		}

		/// <summary>
		/// Render a FrameworkElement to a bitmap, using it's current size
		/// </summary>
		/// <param name="frameworkElement"></param>
		/// <returns>System.Drawing.Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap(this FrameworkElement frameworkElement, PixelFormat pixelFormat = default(PixelFormat)) {
			return frameworkElement.ToBitmap((int)(frameworkElement.Width + 0.5), (int)(frameworkElement.Height + 0.5), pixelFormat);
		}

		/// <summary>
		/// Render a FrameworkElement to a RenderTargetBitmap, with the supplied width & height
		/// </summary>
		/// <param name="frameworkElement"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>RenderTargetBitmap</returns>
		public static RenderTargetBitmap ToRenderTargetBitmap(this FrameworkElement frameworkElement, int width = 0, int height = 0, PixelFormat pixelFormat = default(PixelFormat)) {
			if (pixelFormat == default(PixelFormat)) {
				pixelFormat = PixelFormats.Pbgra32;
			}

			if (width == 0) {
				width = (int)frameworkElement.Width;
			}
			if (height == 0) {
				height = (int)frameworkElement.Height;
			}
			var renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			//frameworkElement.Measure(new Size(width, height));
			//frameworkElement.Arrange(new Rect(0,0,width, height));
			//frameworkElement.UpdateLayout();
			renderTargetBitmap.Render(ModifyToDrawingVisual(frameworkElement, width, height));
			return renderTargetBitmap;
		}

		/// <summary>
		/// Render a FrameworkElement to a bitmap, with the supplied width & height
		/// </summary>
		/// <param name="frameworkElement"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>System.Drawing.Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap(this FrameworkElement frameworkElement, int width, int height, PixelFormat pixelFormat = default(PixelFormat)) {
			var renderTargetBitmap = frameworkElement.ToRenderTargetBitmap(width, height, PixelFormats.Pbgra32);

			using (var stream = new MemoryStream()) {
				renderTargetBitmap.ToStream(stream);

				using (var newBitmap = new System.Drawing.Bitmap(stream)) {
					return (System.Drawing.Bitmap)newBitmap.Clone();
				}
			}
		}

		/// <summary>
		/// Write the RenderTargetBitmap to the Stream in the supplied (optional) PixelFormat
		/// </summary>
		/// <param name="renderTargetBitmap">RenderTargetBitmap</param>
		/// <param name="stream">Stream</param>
		/// <param name="pixelFormat">PixelFormat</param>
		public static void ToStream(this RenderTargetBitmap renderTargetBitmap, Stream stream, OutputFormats outputFormat = OutputFormats.png, PixelFormat pixelFormat = default(PixelFormat)) {
			BitmapEncoder encoder;
			switch (outputFormat) {
				case OutputFormats.png:
					encoder = new PngBitmapEncoder();
					break;
				case OutputFormats.gif:
					encoder = new GifBitmapEncoder();
					break;
				case OutputFormats.tiff:
					encoder = new TiffBitmapEncoder();
					break;
				case OutputFormats.jpg:
					encoder = new JpegBitmapEncoder();
					break;
				case OutputFormats.bmp:
					encoder = new BmpBitmapEncoder();
					break;
				default:
					throw new NotImplementedException(string.Format("Currently we do not support: {0}", outputFormat));
			}

			if (pixelFormat == default(PixelFormat)) {
				encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
			} else {
				FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(renderTargetBitmap, pixelFormat, null, 0);
				encoder.Frames.Add(BitmapFrame.Create(convertedBitmap));
			}
			encoder.Save(stream);
		}


		/// <summary>
		/// Helper method for rendering a FrameworkElement
		/// Modify Position aranges and measures the FrameworkElement so it can be drawn.
		/// </summary>
		/// <param name="fe"></param>
		private static void ModifyPosition(FrameworkElement fe) {
			// get the size of the visual with margin
			Size fs = new Size(fe.ActualWidth + fe.Margin.Left + fe.Margin.Right, fe.ActualHeight + fe.Margin.Top + fe.Margin.Bottom);

			// measure the visual with new size
			fe.Measure(fs);

			// arrange the visual to align parent with (0,0)
			fe.Arrange(new Rect(-fe.Margin.Left, -fe.Margin.Top, fs.Width, fs.Height));
		}

		/// <summary>
		/// "Reverts" the ModifyPosition call
		/// </summary>
		/// <param name="fe"></param>
		private static void ModifyPositionBack(FrameworkElement fe) {
			// remeasure a size smaller than need, wpf will
			// rearrange it to the original position
			fe.Measure(new Size());
		}

		private static DrawingVisual ModifyToDrawingVisual(Visual visual, int width, int height) {
			// new a drawing visual and get its context
			DrawingVisual dv = new DrawingVisual();
			DrawingContext dc = dv.RenderOpen();

			// generate a visual brush by input, and paint
			VisualBrush vb = new VisualBrush(visual);
			dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
			dc.Close();

			return dv;
		}
	}
}
