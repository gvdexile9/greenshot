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

using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Icons;
using Greenshot.Addons.Core;
using Greenshot.Addons.Interfaces;
using Greenshot.Core.Interfaces;
using IProcessor = Greenshot.Core.Interfaces.IProcessor;

namespace Greenshot.Core.Processors {
	/// <summary>
	/// Use this processor for the full-screen capture, this will crop the needed part of the screen
	/// </summary>
	[Processor(Designation = "ScreenCaptureProcessor", LanguageKey = "")]
	public class ScreenCaptureProcessor : IProcessor {
		private static readonly Logger LOG = LogManager.GetCurrentClassLogger();

		[Import]
		private ICoreConfiguration _conf = null;

		public Task<bool> ProcessAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			switch (_conf.ScreenCaptureMode) {
				case ScreenCaptureMode.Auto:
					System.Drawing.Point mouseLocation = GetCursorLocation();
					foreach (var screen in System.Windows.Forms.Screen.AllScreens) {
						if (screen.Bounds.Contains(mouseLocation)) {
							captureContext.CropRect = new Rect(screen.Bounds.Left, screen.Bounds.Top, screen.Bounds.Width, screen.Bounds.Height);
							break;
						}
					}
					break;
				case ScreenCaptureMode.Fixed:
					if (_conf.ScreenToCapture > 0 && _conf.ScreenToCapture <= System.Windows.Forms.Screen.AllScreens.Length) {
						var bounds = System.Windows.Forms.Screen.AllScreens[_conf.ScreenToCapture].Bounds;
						captureContext.CropRect = new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
					}
					break;
				case ScreenCaptureMode.FullScreen:
					// Do nothing, we take the fullscreen capture automatically
					break;
			}
			_conf.LastCapturedRegion = captureContext.CropRect;
			return Task.FromResult<bool>(true);
		}

		/// <summary>
		///     Retrieves the cursor location safely, accounting for DPI settings in Vista/Windows 7.
		/// </summary>
		/// <returns>
		///     Point with cursor location, relative to the origin of the monitor setup (i.e. negative coordinates are
		///     possible in multiscreen setups)
		/// </returns>
		private static System.Drawing.Point GetCursorLocation() {
			NativePoint cursorLocation;
			if (CursorHelper.GetPhysicalCursorPos(out cursorLocation)) {
				return new System.Drawing.Point(cursorLocation.X, cursorLocation.Y);
			}
			return System.Drawing.Point.Empty;
		}
	}
}
