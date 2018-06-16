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

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Greenshot.Core.Interfaces {
	/// <summary>
	/// This is the attribute which can be used for the type-safe meta-data
	/// </summary>

	public class DestinationAttribute : ExportAttribute, IDestinationMetadata {
		public DestinationAttribute() : base(typeof(IDestination)) {
		}
		public string DescriptionLanguageKey {
			get;
			set;
		}
		public string Designation {
			get;
			set;
		}
		/// <summary>
		/// Filename of the icon (SVG) to use
		/// </summary>
		public string IconFilename {
			get;
			set;
		}
		/// <summary>
		/// Filename of the logo (SVG) to use
		/// </summary>
		public string LogoFilename {
			get;
			set;
		}
	}

	/// <summary>
	/// This is the interface for the MEF meta-data
	/// </summary>
	public interface IDestinationMetadata {
		[DefaultValue(null)]
		string DescriptionLanguageKey {
			get;
		}
		[DefaultValue(null)]
		string IconFilename {
			get;
		}
		[DefaultValue(null)]
		string LogoFilename {
			get;
		}
		string Designation {
			get;
		}
	}

	/// <summary>
	/// This IDestination describes the modules which can export a capture
	/// </summary>
	public interface IDestination : IModule {
        /// <summary>
        /// This is called when the destination needs to export a CaptureContext
        /// </summary>
        /// <param name="captureContext">CaptureContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        Task<bool> ExportAsync(CaptureContext captureContext, CancellationToken cancellationToken = default);
	}
}
