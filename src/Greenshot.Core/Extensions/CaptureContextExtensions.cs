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

using System.IO;
using System.Windows;
using System.Windows.Media;
using Greenshot.Core.Extensions;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core {
	public static class CaptureContextExtensions {

		/// <summary>
		/// Create a UI Element via the named template
		/// </summary>
		/// <param name="context"></param>
		/// <param name="templateName"></param>
		/// <returns>FrameworkElement for the CaptureContext</returns>
		public static FrameworkElement ApplyTemplate(this CaptureContext context, string templateName = "DefaultTemplate") {
			if (templateName == null) {
				templateName = "DefaultTemplate";
			}
			var template = ModuleContainer.MefContainer.GetExport<ITemplate>(templateName);
			return template.Value.Create(context, true);
		}

		/// <summary>
		/// Write this context as a Bitmap to a stream, using the settings in the context.
		/// </summary>
		/// <param name="stream">Stream to write the bitmap to</param>
		/// <param name="outputFormat">Bitmap-Format to write to, if this is not supplied use PNG</param>
		/// <param name="templateName">Name of the template, null -> DefaultTemplate</param>
		public static void ToStream(this CaptureContext context, Stream stream, string templateName = null, OutputFormat outputFormat = OutputFormat.png, PixelFormat pixelFormat = default(PixelFormat)) {
			var frameworkElement = context.ApplyTemplate(templateName);
			frameworkElement.ToRenderTargetBitmap().ToStream(stream, outputFormat, pixelFormat);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="outputFormat">Bitmap-Format to write to, if this is not supplied use PNG</param>
		/// <param name="templateName">Name of the template to use for rendering, null -> DefaultTemplate</param>
		/// <returns></returns>
		public static MemoryStream AsMemoryStream(this CaptureContext context, string templateName = null, OutputFormat outputFormat = OutputFormat.png, PixelFormat pixelFormat = default(PixelFormat)) {
			var stream = new MemoryStream();
			context.ToStream(stream, templateName, outputFormat, pixelFormat);
			return stream;
		}
	}
}
