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
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Greenshot.Capturing;
using Greenshot.Plugin.Core;
using Greenshot.Plugin.Modules;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.Direct3D10_1;
using System.IO;
using System.Windows.Media;

namespace Greenshot.Modules {
	/// <summary>
	/// This class is currently not used!!
	///
	/// Uses the DwmGetDxSharedSurface in user32 to get access to the DWM Shared surface.
	/// With SlimDX we can clone the information, and store it in a Bitmap.
	/// Disadvantages are that we have no Window "border" and we need SlimDX.
	/// Advantages are that we capture the complete transparancy, which also poses a small problem:
	/// we should draw the image on a background!
	/// 
	/// I still feel like the code is not fully optimized yet.
	/// There also seems to be a problem with the shared suface, I got garbage on it...
	/// </summary>
	[Export(typeof(ISource))]
	[SourceMetadata(Designation = "WindowFromDWMSource", LanguageKey = "")]
	public class WindowFromDWMSource : ISource {
		#region native import
		[DllImport("user32", SetLastError = true, EntryPoint = "DwmGetDxSharedSurface")]
		private static extern bool DwmGetDxSharedSurface(IntPtr hWnd, out IntPtr sharedHandle, out UInt64 p2, out UInt32 p3, out UInt32 p4, out UInt64 p5);
		#endregion

		[Import]
		private WindowsContainer _windowsContainer = null;

		/// <summary>
		/// Capture the screen
		/// </summary>
		/// <returns></returns>
		public bool Import(CaptureContext captureContext) {
			captureContext.Capture = Capture(_windowsContainer.ActiveWindow);
			captureContext.ClipArea = new RectangleGeometry(windowBounds);
			captureContext.CropRect = windowBounds;
			return true;
		}

		/// <summary>
		/// Capture the window via DWM & DX by using the supplied hWnd
		/// </summary>
		/// <param name="hWnd">IntPtr</param>
		/// <returns>BitmapSource</returns>
		public static IElement<BitmapSource> Capture(WindowInfo window) {
			IElement<BitmapSource> element = new BitmapSourceElement();

			IntPtr sharedHandle;
			UInt64 p2;
			UInt32 p3;
			UInt32 p4;
			UInt64 p5;
			if (!DwmGetDxSharedSurface(window.Handle, out sharedHandle, out p2, out p3, out p4, out p5)) {
				return null;
			}
			if (sharedHandle == IntPtr.Zero) {
				return null;
			}

			BitmapSource bitmap = null;
			using (var device = new SlimDX.Direct3D10_1.Device1(DeviceCreationFlags.Debug, FeatureLevel.Level_10_0)) {
				using (var texture = device.OpenSharedResource<Texture2D>(sharedHandle)) {
					Console.WriteLine(string.Format("Info {0}x{1} {2}", texture.Description.Width, texture.Description.Height, texture.Description.Format));
					var surface = texture.AsSurface();
					var screenTexture2DDesc = texture.Description;
					screenTexture2DDesc.BindFlags = BindFlags.None;
					screenTexture2DDesc.Usage = ResourceUsage.Staging;
					screenTexture2DDesc.CpuAccessFlags = CpuAccessFlags.Read;
					screenTexture2DDesc.OptionFlags = ResourceOptionFlags.None;
					byte[] data = null;
					int width = 0;
					int height = 0;
					using (var tmpTexture = new SlimDX.Direct3D10.Texture2D(device, screenTexture2DDesc)) {
						device.CopyResource(texture, tmpTexture);
						var clonedSurface = tmpTexture.AsSurface();

						width = clonedSurface.Description.Width;
						height = clonedSurface.Description.Height;
						var map = clonedSurface.Map(SlimDX.DXGI.MapFlags.Read);

						using (DataStream dataStream = map.Data) {
							int lines = (int)(dataStream.Length / map.Pitch);
							data = new byte[(width * 4) * height];
							int dataCounter = 0;
							int rest = map.Pitch - (width * 4);
							// width of the surface - 4 bytes per pixel.
							for (int y = 0; y < height; y++) {
								dataCounter += dataStream.ReadRange(data, dataCounter, width * 4);
								dataStream.Seek(rest, SeekOrigin.Current);
							}
						}
						clonedSurface.Unmap();
					}
					if (data != null) {
						bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Pbgra32, null, data, width * 4);
					}
				}
			}
			element.Content = bitmap;
			element.Bounds = window.Bounds;
			return element;
		}
	}

}
