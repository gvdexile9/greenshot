#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core.Templates {
	[Export("CroppedTemplate", typeof(ITemplate))]
	[Export("DefaultTemplate", typeof(ITemplate))]
	public class CroppedTemplate : ITemplate {
		public FrameworkElement Create(CaptureContext captureContext, bool displayMouse = true) {
			int width = (int)(captureContext.CropRect.Width + 0.5);
			int height = (int)(captureContext.CropRect.Height + 0.5);
		    var rectangle = new Rectangle
		    {
		        Width = width,
		        Height = height,
		        Stretch = System.Windows.Media.Stretch.Fill
		    };
		    var visualBrush = new VisualBrush
		    {
		        Viewbox = captureContext.CropRect,
		        ViewboxUnits = BrushMappingMode.Absolute,
		        Viewport = new Rect(0, 0, 1, 1),
		        ViewportUnits = BrushMappingMode.RelativeToBoundingBox
		    };

		    var canvas = new Canvas
		    {
		        Width = captureContext.Capture.Bounds.Width,
		        Height = captureContext.Capture.Bounds.Height,
		        Clip = captureContext.ClipArea
		    };

		    var image = new Image
		    {
		        Source = captureContext.Capture.Content,
		        Width = captureContext.Capture.Bounds.Width,
		        Height = captureContext.Capture.Bounds.Height
		    };
		    Canvas.SetTop(image, 0);
			Canvas.SetLeft(image, 0);
			canvas.Children.Add(image);
			if (displayMouse) {
			    var cursor = new Image
			    {
			        Source = captureContext.MouseCursor.Content,
			        Width = captureContext.MouseCursor.Bounds.Width,
			        Height = captureContext.MouseCursor.Bounds.Height
			    };
			    Canvas.SetTop(cursor, captureContext.MouseCursor.Bounds.Top);
				Canvas.SetLeft(cursor, captureContext.MouseCursor.Bounds.Left);
				canvas.Children.Add(cursor);
			}

			visualBrush.Visual = canvas;
			rectangle.Fill = visualBrush;

			rectangle.Measure(new Size(width, height));
			rectangle.Arrange(new Rect(0, 0, width, height));
			rectangle.UpdateLayout();

			return rectangle;
		}
	}
}
