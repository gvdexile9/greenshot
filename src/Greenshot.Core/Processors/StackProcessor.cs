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

namespace Greenshot.Core.Processors {
	/// <summary>
	/// The StackProcessor can have multiple processors, which are called in the set order
	/// </summary>
	public class StackProcessor : IProcessor {
        /// <summary>
        /// The list of IProcessors
        /// </summary>
		public IList<IProcessor> Processors { get; } = new List<IProcessor>();

        /// <summary>
        /// Actually process the CaptureContext with multiple IProcessors
        /// </summary>
        /// <param name="captureContext">CaptureContext</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task with bool</returns>
        public async Task<bool> ProcessAsync(CaptureContext captureContext, CancellationToken cancellationToken = default(CancellationToken)) {
			bool result = true;
			foreach (var processor in Processors) {
				result = result && await processor.ProcessAsync(captureContext, cancellationToken);
			}
			return result;
		}
	}
}
