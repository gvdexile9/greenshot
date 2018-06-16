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
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Greenshot.Addons.Core;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core.Processors {
	[Processor(Designation = "TitlefixProcessor", LanguageKey = "")]
	public class TitlefixProcessor : IProcessor {
		private static readonly LogSource Log = new LogSource();
		
		private ICoreConfiguration _config = null;

		[ImportingConstructor]
		public TitlefixProcessor(ICoreConfiguration config) {
			this._config = config;
			var corruptKeys = new List<string>();
			foreach (string key in _config.ActiveTitleFixes) {
				if (!_config.TitleFixMatcher.ContainsKey(key)) {
					Log.Warn().WriteLine("Key {0} not found, configuration is broken! Disabling this key!");
					corruptKeys.Add(key);
				}
			}

			// Fix configuration if needed
			if (corruptKeys.Count > 0) {
				foreach (string corruptKey in corruptKeys) {
					// Removing any reference to the key
					_config.ActiveTitleFixes.Remove(corruptKey);
					_config.TitleFixMatcher.Remove(corruptKey);
					_config.TitleFixReplacer.Remove(corruptKey);
				}
			}
		}

		public Task<bool> ProcessAsync(CaptureContext captureContext, CancellationToken token = default(CancellationToken)) {
			bool changed = false;
			string title = captureContext.Title;
			if (!string.IsNullOrEmpty(title)) {
				title = title.Trim();
				foreach (string titleIdentifier in _config.ActiveTitleFixes) {
					string regexpString = _config.TitleFixMatcher[titleIdentifier];
					string replaceString = _config.TitleFixReplacer[titleIdentifier];
					if (replaceString == null) {
						replaceString = "";
					}
					if (!string.IsNullOrEmpty(regexpString)) {
						Regex regex = new Regex(regexpString);
						title = regex.Replace(title, replaceString);
						changed = true;
					}
				}
			}
			if (changed) {
				captureContext.Title = title;
			}
			return Task.FromResult<bool>(true);
		}
	}
}
