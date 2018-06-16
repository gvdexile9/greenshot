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

using System.Windows;

namespace Greenshot.Core.Interfaces {
	/// <summary>
	/// Basic interface which every visable module should implement
	/// </summary>
	public interface IVisualModule : IModule {
		/// <summary>
		/// Simple "designation" like "File", "Editor" etc, used reference this module
		/// </summary>
		string Designation {
			get;
		}

		/// <summary>
		/// Description which will be shown in the settings form, Context Menu, destination picker etc
		/// </summary>
		string Description {
			get;
		}

		/// <summary>
		/// Priority, used for sorting
		/// </summary>
		int Priority {
			get;
		}

		/// <summary>
		/// Returns if the module is active
		/// </summary>
		bool IsActive {
			get;
		}
		/// <summary>
		/// When displaying the module somewhere with an Icon, this is used.
		/// </summary>
		DependencyObject Visual {
			get;
		}
	}
}
