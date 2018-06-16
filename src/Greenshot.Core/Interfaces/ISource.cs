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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Greenshot.Core.Interfaces {
	/// <summary>
	/// This is the attribute which can be used for the type-safe meta-data
	/// </summary>
	[MetadataAttribute]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SourceAttribute : ExportAttribute, ISourceMetadata {
		public SourceAttribute() : base (typeof(ISource)) {
		}
		public string Designation {
			get;
			set;
		}
		public string LanguageKey {
			get;
			set;
		}
	}

	/// <summary>
	/// This is the interface for the MEF meta-data
	/// </summary>
	public interface ISourceMetadata {
		string Designation {
			get;
		}
		string LanguageKey {
			get;
		}
	}

	/// <summary>
	/// This ISource describes the modules which can capture a "source".
	/// </summary>
	public interface ISource : IModule {
		Task<bool> ImportAsync(CaptureContext captureContext, CancellationToken cancellationToken = default(CancellationToken));
	}
}
