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
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Desktop;
using Dapplo.Windows.DesktopWindowsManager;
using Dapplo.Windows.Gdi32;
using Dapplo.Windows.User32.Enums;
using Dapplo.Windows.User32.Structs;
using Greenshot.Addons.Core;
using Greenshot.Addons.Core.Enums;
using Greenshot.Core.Interfaces;
using Greenshot.Modules;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Region = System.Drawing.Region;
using Screen = System.Windows.Forms.Screen;
using Size = System.Windows.Size;

namespace Greenshot.Core.Sources {
	/// <summary>
	/// This is the screen source, and makes it possible to capture the screen
	/// </summary>
	[Export(typeof(ISource))]
	[SourceMetadata(Designation = "ScreenSource", LanguageKey = "")]
	public class DWMWindowScreenSource : ScreenSource {
		private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Capture the screen
		/// </summary>
		/// <returns></returns>
		public new bool Import(CaptureContext captureContext) {
			return true;
		}

		/// <summary>
		/// Capture DWM Window
		/// </summary>
		/// <param name="capture">Capture to fill</param>
		/// <param name="windowCaptureMode">Wanted WindowCaptureMode</param>
		/// <param name="autoMode">True if auto modus is used</param>
		/// <returns>ICapture with the capture</returns>
		public BitmapSource CaptureDwmWindow(IInteropWindow window, WindowCaptureModes windowCaptureMode, bool autoMode) {
			var thumbnailHandle = IntPtr.Zero;
			System.Windows.Forms.Form tempForm = null;
			var tempFormShown = false;
			try {
				tempForm = new System.Windows.Forms.Form {
					ShowInTaskbar = false,
					FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
					TopMost = true
				};

				// Register the Thumbnail
				Dwm.DwmRegisterThumbnail(tempForm.Handle, window.Handle, out thumbnailHandle);

				// Get the original size
			    Dwm.DwmQueryThumbnailSourceSize(thumbnailHandle, out var sourceSize);

				if (sourceSize.Width <= 0 || sourceSize.Height <= 0) {
					return null;
				}

				// Calculate the location of the temp form
				Point formLocation;
				var borderSize = new Size();
				var doesCaptureFit = false;
			    var bounds = window.GetInfo().Bounds;
				if (!window.IsMaximized()) {
					// Assume using it's own location
					formLocation = bounds.Location;
					using (var workingArea = new Region(Screen.PrimaryScreen.Bounds)) {
						// Find the screen where the window is and check if it fits
						foreach (var screen in Screen.AllScreens) {
							if (!Equals(screen, Screen.PrimaryScreen)) {
								workingArea.Union(screen.Bounds);
							}
						}

						// If the formLocation is not inside the visible area
						if (!workingArea.AreRectangleCornersVisisble(bounds)) {
							// If none found we find the biggest screen
							foreach (Screen screen in Screen.AllScreens) {
								Rect newWindowRectangle = new Rect(screen.WorkingArea.Location, window.Bounds.Size);
								if (!workingArea.AreRectangleCornersVisisble(newWindowRectangle)) {
									continue;
								}
								formLocation = screen.Bounds.Location;
								doesCaptureFit = true;
								break;
							}
						} else {
							doesCaptureFit = true;
						}
					}
				} else {
					//GetClientRect(out windowRectangle);
					User32.GetBorderSize(window.Handle, out borderSize);
					formLocation = new Point(window.Bounds.X - borderSize.Width, window.Bounds.Y - borderSize.Height);
				}

				tempForm.Location = formLocation;
				tempForm.Size = sourceSize.ToSize();

				// Prepare rectangle to capture from the screen.
				var captureRectangle = new Rect(formLocation.X, formLocation.Y, sourceSize.width, sourceSize.height);
				if (window.Maximised) {
					// Correct capture size for maximized window by offsetting the X,Y with the border size
					captureRectangle.X += borderSize.Width;
					captureRectangle.Y += borderSize.Height;
					// and subtrackting the border from the size (2 times, as we move right/down for the capture without resizing)
					captureRectangle.Width -= 2 * borderSize.Width;
					captureRectangle.Height -= 2 * borderSize.Height;
				} else if (autoMode) {
					// check if the capture fits
					if (!doesCaptureFit) {
						// if GDI is allowed.. (a screenshot won't be better than we comes if we continue)
						if (!window.IsMetroApp && WindowCapture.IsGdiAllowed(window.Process)) {
							// we return null which causes the capturing code to try another method.
							return null;
						}
					}
				}

				// Prepare the displaying of the Thumbnail
				var props = new DWM_THUMBNAIL_PROPERTIES();
				props.Opacity = 255;
				props.Visible = true;
				props.Destination = new RECT(0, 0, sourceSize.width, sourceSize.height);
				DWM.DwmUpdateThumbnailProperties(thumbnailHandle, ref props);
				tempForm.Show();
				tempFormShown = true;

				// Intersect with screen
				captureRectangle.Intersect(capture.ScreenBounds);

				// Destination bitmap for the capture
				Bitmap capturedBitmap = null;
				var frozen = false;
				try {
					// Check if we make a transparent capture
					if (windowCaptureMode == WindowCaptureMode.AeroTransparent) {
						frozen = FreezeWindow();
						// Use white, later black to capture transparent
						tempForm.BackColor = Color.White;
						// Make sure everything is visible
						tempForm.Refresh();
						Application.DoEvents();

						try {
							using (var whiteBitmap = WindowCapture.CaptureRectangle(captureRectangle)) {
								// Apply a white color
								tempForm.BackColor = Color.Black;
								// Make sure everything is visible
								tempForm.Refresh();
								if (!window.IsMetroApp) {
									// Make sure the application window is active, so the colors & buttons are right
									ToForeground();
								}
								// Make sure all changes are processed and visisble
								Application.DoEvents();
								using (var blackBitmap = WindowCapture.CaptureRectangle(captureRectangle)) {
									capturedBitmap = ApplyTransparency(blackBitmap, whiteBitmap);
								}
							}
						} catch (Exception ex) {
							LOG.Debug("Exception: ", ex);
							// Some problem occured, cleanup and make a normal capture
							if (capturedBitmap != null) {
								capturedBitmap.Dispose();
								capturedBitmap = null;
							}
						}
					}
					// If no capture up till now, create a normal capture.
					if (capturedBitmap == null) {
						// Remove transparency, this will break the capturing
						if (!autoMode) {
							tempForm.BackColor = Color.FromArgb(255, Conf.DWMBackgroundColor.R, Conf.DWMBackgroundColor.G, Conf.DWMBackgroundColor.B);
						} else {
							var colorizationColor = DWM.ColorizationColor;
							// Modify by losing the transparency and increasing the intensity (as if the background color is white)
							colorizationColor = Color.FromArgb(255, (colorizationColor.R + 255) >> 1, (colorizationColor.G + 255) >> 1, (colorizationColor.B + 255) >> 1);
							tempForm.BackColor = colorizationColor;
						}
						// Make sure everything is visible
						tempForm.Refresh();
						if (!window.IsMetroApp) {
							// Make sure the application window is active, so the colors & buttons are right
							window.ToForeground();
						}
						// Make sure all changes are processed and visisble
						Application.DoEvents();
						// Capture from the screen
						capturedBitmap = WindowCapture.CaptureRectangle(captureRectangle);
					}
					if (capturedBitmap != null) {
						// Not needed for Windows 8
						if (!(Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2)) {
							// Only if the Inivalue is set, not maximized and it's not a tool window.
							if (Conf.WindowCaptureRemoveCorners && !window.Maximised && (window.ExtendedWindowStyle & ExtendedWindowStyleFlags.WS_EX_TOOLWINDOW) == 0) {
								// Remove corners
								if (!Image.IsAlphaPixelFormat(capturedBitmap.PixelFormat)) {
									LOG.Debug("Changing pixelformat to Alpha for the RemoveCorners");
									var tmpBitmap = ImageHelper.Clone(capturedBitmap, PixelFormat.Format32bppArgb);
									capturedBitmap.Dispose();
									capturedBitmap = tmpBitmap;
								}
								RemoveCorners(capturedBitmap);
							}
						}
					}
				} finally {
					// Make sure to ALWAYS unfreeze!!
					if (frozen) {
						UnfreezeWindow();
					}
				}

				capture.Image = capturedBitmap;
				// Make sure the capture location is the location of the window, not the copy
				capture.Location = Location;
			} finally {
				if (thumbnailHandle != IntPtr.Zero) {
					// Unregister (cleanup), as we are finished we don't need the form or the thumbnail anymore
					DWM.DwmUnregisterThumbnail(thumbnailHandle);
				}
				if (tempForm != null) {
					if (tempFormShown) {
						tempForm.Close();
					}
					tempForm.Dispose();
				}
			}

			return capture;
		}

	}
}
