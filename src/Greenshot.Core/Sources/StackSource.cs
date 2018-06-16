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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core.Sources {
	/// <summary>
	/// This is the stack source, and makes it possible to stack multiple sources
	/// </summary>
	public class StackSource : ISource {
        /// <summary>
        /// The list of ISource
        /// </summary>
        public IList<ISource> Sources { get; } = new List<ISource>();

        /// <summary>
        /// This is creating an import, into the specified CaptureContext
        /// </summary>
        /// <param name="captureContext">CaptureContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task bool</returns>
        public async Task<bool> ImportAsync(CaptureContext captureContext, CancellationToken cancellationToken = default) {
			bool returnValue = true;
			foreach (var source in Sources) {
				returnValue &= await source.ImportAsync(captureContext, cancellationToken);
			}
			return returnValue;
		}
	}
}
