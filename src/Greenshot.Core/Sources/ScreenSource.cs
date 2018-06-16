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

using Greenshot.Plugin.Modules;
using Greenshot.Plugin.UnmanagedHelpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapplo.Log;
using Dapplo.Windows.Common;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Gdi32;
using Dapplo.Windows.Gdi32.SafeHandles;
using Dapplo.Windows.Gdi32.Structs;
using Dapplo.Windows.User32;
using Greenshot.Core;
using Greenshot.Core.Extensions;
using Greenshot.Core.Interfaces;
using Greenshot.Plugin.Modules.Interfaces;
using Region = System.Drawing.Region;
using Screen = System.Windows.Forms.Screen;

namespace Greenshot.Modules {
	/// <summary>
	/// This is the screen source, and makes it possible to capture the screen
	/// </summary>
	[Source(Designation = "ScreenSource", LanguageKey = "")]
	public class ScreenSource : ISource {
        private static readonly LogSource Log = new LogSource();
		/// <summary>
		/// Capture the screen
		/// </summary>
		/// <returns></returns>
		public virtual Task<bool> ImportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			Rect screenBounds = GetScreenBounds();
			captureContext.Capture = CaptureRectangle(screenBounds);
			captureContext.ClipArea = new RectangleGeometry(screenBounds);
			captureContext.CropRect = screenBounds;
			return Task.FromResult<bool>(true);
		}

		/// <summary>
		/// Get the bounds of all screens combined.
		/// </summary>
		/// <returns>A Rect of the bounds of the entire display area.</returns>
		protected static Rect GetScreenBounds() {
			int left = 0, top = 0, bottom = 0, right = 0;
			foreach (Screen screen in Screen.AllScreens) {
				left = Math.Min(left, screen.Bounds.X);
				top = Math.Min(top, screen.Bounds.Y);
				int screenAbsRight = screen.Bounds.X + screen.Bounds.Width;
				int screenAbsBottom = screen.Bounds.Y + screen.Bounds.Height;
				right = Math.Max(right, screenAbsRight);
				bottom = Math.Max(bottom, screenAbsBottom);
			}
			return new Rect(left, top, (right + Math.Abs(left)), (bottom + Math.Abs(top)));
		}

	
		/// <summary>
		/// Helper method to create an exception that might explain what is wrong while capturing
		/// </summary>
		/// <param name="method">string with current method</param>
		/// <param name="captureBounds">Rectangle of what we want to capture</param>
		/// <returns></returns>
		private static Exception CreateCaptureException(string method, Rect captureBounds) {
			Exception exceptionToThrow = User32Api.CreateWin32Exception(method);
			if (!captureBounds.IsEmpty) {
				exceptionToThrow.Data.Add("Height", captureBounds.Height);
				exceptionToThrow.Data.Add("Width", captureBounds.Width);
			}
			return exceptionToThrow;
		}

		/// <summary>
		/// This method will use User32 code to capture the specified captureBounds from the screen
		/// </summary>
		/// <param name="captureBounds">Rectangle with the bounds to capture</param>
		/// <returns>Bitmap which is captured from the screen at the location specified by the captureBounds</returns>
		protected static IElement<BitmapSource> CaptureRectangle(Rect captureBounds) {
			if (captureBounds.Height <= 0 || captureBounds.Width <= 0) {
				Log.Warn().WriteLine("Nothing to capture, ignoring!");
				return null;
			}
			IElement<BitmapSource> element = new BitmapSourceElement();
			Log.Debug().WriteLine("CaptureRectangle Called!");
			// .NET GDI+ Solution, according to some post this has a GDI+ leak...
			// See http://connect.microsoft.com/VisualStudio/feedback/details/344752/gdi-object-leak-when-calling-graphics-copyfromscreen
			// Bitmap capturedBitmap = new Bitmap(captureBounds.Width, captureBounds.Height);
			// using (Graphics graphics = Graphics.FromImage(capturedBitmap)) {
			//	graphics.CopyFromScreen(captureBounds.Location, Point.Empty, captureBounds.Size, CopyPixelOperation.CaptureBlt);
			// }
			// capture.Image = capturedBitmap;
			// capture.Location = captureBounds.Location;

			using (SafeWindowDCHandle desktopDCHandle = SafeWindowDCHandle.FromDesktop()) {
				if (desktopDCHandle.IsInvalid) {
					// Get Exception before the error is lost
					Exception exceptionToThrow = CreateCaptureException("desktopDCHandle", captureBounds);
					// throw exception
					throw exceptionToThrow;
				}

				// create a device context we can copy to
				using (SafeCompatibleDCHandle safeCompatibleDCHandle = GDI32.CreateCompatibleDC(desktopDCHandle)) {
					// Check if the device context is there, if not throw an error with as much info as possible!
					if (safeCompatibleDCHandle.IsInvalid) {
						// Get Exception before the error is lost
						Exception exceptionToThrow = CreateCaptureException("CreateCompatibleDC", captureBounds);
						// throw exception
						throw exceptionToThrow;
					}
					// Create BitmapInfoHeader for CreateDIBSection
					var bmi = new BitmapInfoHeader(captureBounds.Width, captureBounds.Height, 24);

					// Make sure the last error is set to 0
					Win32.SetLastError(0);

					// create a bitmap we can copy it to, using GetDeviceCaps to get the width/height
					IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
					using (SafeDibSectionHandle safeDibSectionHandle = Gdi32Api.CreateDIBSection(desktopDCHandle, ref bmi, BITMAPINFOHEADER.DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0)) {
						if (safeDibSectionHandle.IsInvalid) {
							// Get Exception before the error is lost
							Exception exceptionToThrow = CreateCaptureException("CreateDIBSection", captureBounds);
							exceptionToThrow.Data.Add("hdcDest", safeCompatibleDCHandle.DangerousGetHandle().ToInt32());
							exceptionToThrow.Data.Add("hdcSrc", desktopDCHandle.DangerousGetHandle().ToInt32());

							// Throw so people can report the problem
							throw exceptionToThrow;
						}
						// select the bitmap object and store the old handle
						using (safeCompatibleDCHandle.SelectObject(safeDibSectionHandle)) {
                            // bitblt over (make copy)
                            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
						    Gdi32Api.BitBlt(safeCompatibleDCHandle, 0, 0, (int)captureBounds.Width, (int)captureBounds.Height, desktopDCHandle, (int)captureBounds.X, (int)captureBounds.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
						}

						// get a .NET image object for it
						// A suggestion for the "A generic error occurred in GDI+." E_FAIL/0×80004005 error is to re-try...
						bool success = false;
						ExternalException exception = null;
						for (int i = 0; i < 3; i++) {
							try {
								// Create BitmapSource from the DibSection
								element.Content = Imaging.CreateBitmapSourceFromHBitmap(safeDibSectionHandle.DangerousGetHandle(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

								// Now cut away invisible parts

								// Collect all screens inside this capture
								var screensInsideCapture = new List<Screen>();
								foreach (Screen screen in Screen.AllScreens) {
									if (screen.Bounds.IntersectsWith(captureBounds.ToRectangle())) {
										screensInsideCapture.Add(screen);
									}
								}
								// Check all all screens are of an equal size
								bool offscreenContent;
								using (Region captureRegion = new Region(captureBounds.ToRectangle())) {
									// Exclude every visible part
									foreach (Screen screen in screensInsideCapture) {
										captureRegion.Exclude(screen.Bounds);
									}
									// If the region is not empty, we have "offscreenContent"
									using (Graphics screenGraphics = Graphics.FromHwnd(User32Api.GetDesktopWindow())) {
										offscreenContent = !captureRegion.IsEmpty(screenGraphics);
									}
								}
								// Check if we need to have a transparent background, needed for offscreen content
								if (offscreenContent) {
									var modifiedImage = new WriteableBitmap(element.Content.PixelWidth, element.Content.PixelHeight, element.Content.DpiX, element.Content.DpiY, PixelFormats.Bgr32, element.Content.Palette);
									foreach (Screen screen in Screen.AllScreens) {
										modifiedImage.CopyPixels(element.Content, screen.Bounds);
									}
								}
								// We got through the capture without exception
								success = true;
								break;
							} catch (ExternalException ee) {
								Log.Warn().WriteLine(ee, "Problem getting ImageSource at try {0} : ", i);
								exception = ee;
							}
						}
						if (!success) {
							Log.Error().WriteLine("Still couldn't create ImageSource!");
							if (exception != null) {
								throw exception;
							}
						}
					}
				}
			}
			element.Bounds = new NativeRect(0, 0, (int)element.Content.Width, (int)element.Content.Height);
			return element;
		}
	}
}
