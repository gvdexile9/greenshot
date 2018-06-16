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

using Greenshot.Windows;
using Greenshot.Plugin.Modules;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading;
using Greenshot.Plugin.Modules.Interfaces;

namespace Greenshot.Modules {
	[Destination(Designation = "PreviewDestinationPicker", DescriptionLanguageKey = "settings_destination_picker", IconFilename = "preview")]
	public class PreviewDestinationPicker : IDestination {

		[Import]
		private ExportFactory<DestinationPickerWindow> _destinationPickerWindowFactory = null;

		public Task<bool> ExportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			var picker = _destinationPickerWindowFactory.CreateExport().Value;
			picker.CaptureContext = captureContext;

			var result = picker.ShowDialog();
			if (result.HasValue && result.Value) {
				// Do something
			}
			return Task.FromResult<bool>(true);
		}
	}
}
