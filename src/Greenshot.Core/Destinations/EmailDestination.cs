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

using Greenshot.Helpers;
using Greenshot.Plugin.Modules;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Greenshot.Plugin.Modules.Interfaces;

namespace Greenshot.Modules {
	/// <summary>
	/// The EmailDestination, this is exported during startup if a MAPI client is found.
	/// </summary>
	public class EmailDestination : IDestination {

		public Task<bool> ExportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			bool createdFile = false;
			string imageFile = Path.Combine(@"C:\LocalData", captureContext.Filename + ".png");
			try {
				using (var fileStream = new FileStream(imageFile, FileMode.OpenOrCreate)) {
					createdFile = true;
					captureContext.ToStream(fileStream);
					MapiMailMessage.SendImage(imageFile, captureContext);
				}
			} finally {
				// Cleanup imageFile if we created it here, so less tmp-files are generated and left
				if (createdFile && File.Exists(imageFile)) {
					File.Delete(imageFile);
				}
			}
			return Task.FromResult<bool>(true);
		}
	}
}