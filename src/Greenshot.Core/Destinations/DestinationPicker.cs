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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Greenshot.Addons.Core;
using Greenshot.Core;
using Greenshot.Core.Encoders;
using Greenshot.Core.Interfaces;

namespace Greenshot.Modules {
	[Destination(Designation = "DestinationPicker", DescriptionLanguageKey = "settings_destination_picker", IconFilename="menu")]
	public class DestinationPicker : IDestination {
		[ImportMany]
		private IEnumerable<Lazy<IDestination, IDestinationMetadata>> _destinations = null;

		public Task<bool> ExportAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			ContextMenu menu = new ContextMenu();
			menu.Placement = PlacementMode.Mouse;

			var sortedDestination = from destination in _destinations
									where !destination.Metadata.Designation.Contains("Picker")
									orderby destination.Metadata.Designation
									select destination;

			foreach (var destination in sortedDestination) {
				string translation;
				MenuItem destinationItem = new MenuItem();
				if (Language.TryGetString(destination.Metadata.DescriptionLanguageKey, out translation)) {
					destinationItem.Header = translation;
				} else {
					destinationItem.Header = destination.Metadata.DescriptionLanguageKey;
				}
				var iconDisplayer = new IconDisplayer();
				iconDisplayer.IconName = destination.Metadata.IconFilename;
				iconDisplayer.Width = 24;
				iconDisplayer.Height = 24;
				destinationItem.Icon = iconDisplayer;
				var eventDestination = destination.Value;
				destinationItem.Click += async (sender, eventArgs) => {
					await eventDestination.ExportAsync(captureContext, token);
				};
				menu.Items.Add(destinationItem);
			}
			menu.Items.Add(new Separator());
			MenuItem exitItem = new MenuItem();
			exitItem.Header = Language.GetString("editor_close");
			var exitIconDisplayer = new IconDisplayer();
			exitIconDisplayer.IconName = "exit";
			exitIconDisplayer.Width = 30;
			exitIconDisplayer.Height = 30;
			exitItem.Icon = exitIconDisplayer;
			menu.Items.Add(exitItem);
            
			menu.IsOpen = true;
			menu.Placement = PlacementMode.MousePoint;
			return Task.FromResult<bool>(true);
		}

	}
}
