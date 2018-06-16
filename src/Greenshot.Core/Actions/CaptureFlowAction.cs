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
using System.Threading;
using System.Threading.Tasks;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core.Actions {

	/// <summary>
	/// This IAction implements the basic capture flow:
	/// Source.import, Processor.process, Destination.export
	/// </summary>
	public class CaptureFlowAction : IAction {
        /// <summary>
        /// Source
        /// </summary>
		public ISource Source {
			get;
			set;
		}

        /// <summary>
        ///  Processor
        /// </summary>
		public IProcessor Processor {
			get;
			set;
		}

        /// <summary>
        /// Destination
        /// </summary>
		public IDestination Destination {
			get;
			set;
		}

        /// <summary>
        /// Content
        /// </summary>
		public CaptureContext Context {
			get;
			set;
		}

	    /// <inheritdoc />
	    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default) {
			if (Context == null) {
				throw new ArgumentException("Missing parameter", nameof(Context));
			}
	        if (Source == null)
	        {
	            throw new ArgumentException("Missing parameter", nameof(Source));
	        }
	        if (Destination == null)
	        {
	            throw new ArgumentException("Missing parameter", nameof(Destination));
	        }
            if (!await Source.ImportAsync(Context, cancellationToken)) {
				return false;
			}
			if (Processor != null) {
				await Processor.ProcessAsync(Context, cancellationToken);
			}
			return await Destination.ExportAsync(Context, cancellationToken);
		}
	}
}
