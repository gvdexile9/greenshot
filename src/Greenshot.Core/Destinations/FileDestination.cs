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

using Greenshot.Plugin.Core;
using Greenshot.Plugin.Modules;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Greenshot.Plugin.Modules.Interfaces;

namespace Greenshot.Modules {
	/// <summary>
	/// This is the File destination
	/// </summary>
	[Destination(Designation = "File", DescriptionLanguageKey = "settings_destination_file", IconFilename = "file")]
	public class FileDestination : IDestination {
		private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();
		[Import]
		private CoreConfiguration coreConfiguration = null;

		public Task<bool> ExportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			string filename = FilenameHelper.GetFilenameFromPattern(coreConfiguration.OutputFileFilenamePattern, coreConfiguration.OutputFileFormat, captureContext);
            string filepath = FilenameHelper.FillVariables(coreConfiguration.OutputFilePath, false);
			string combinedPath = Path.Combine(filepath, filename);
			// Convert from CaptureContext to image
			using (var stream = new FileStream(combinedPath, FileMode.OpenOrCreate, FileAccess.Write)) {
				captureContext.ToStream(stream);
				LOG.Info("Wrote file to {0}", combinedPath);
			}
			return Task.FromResult<bool>(true);
		}
	}
}
