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

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Icons;
using Greenshot.Addons.Core;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core.Sources {
	/// <summary>
	/// This is the screen source, and makes it possible to capture the screen
	/// </summary>
	[Source(Designation = "MouseSource", LanguageKey = "")]
	public class MouseSource : ISource {
	    private readonly ICoreConfiguration _coreConfiguration;

	    /// <inheritdoc />
	    public MouseSource(ICoreConfiguration coreConfiguration)
	    {
	        _coreConfiguration = coreConfiguration;
	    }

		/// <summary>
		/// Capture the mouse
		/// </summary>
		/// <returns></returns>
		public Task<bool> ImportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			captureContext.MouseCursor = _coreConfiguration.CaptureMousepointer?CaptureCursor():null;
			return Task.FromResult(true);
		}
		
		/// <summary>
		/// This method will capture the current Cursor by using User32 Code
		/// </summary>
		/// <returns>A IElement with the Mouse Cursor as Image in it.</returns>
		private static IElement<BitmapSource> CaptureCursor()
		{
		    IElement<BitmapSource> element = null;
		    if (CursorHelper.TryGetCurrentCursor(out var bitmapSource, out var location))
		    {
		        element = new BitmapSourceElement
		        {
		            Content = bitmapSource,
		            Bounds = new NativeRect(location,
		                new NativeSize((int) bitmapSource.Width, (int) bitmapSource.Height))
		        };
		    }

			return element;
		}
	}
}
