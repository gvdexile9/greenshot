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
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dapplo.Windows.Common.Structs;
using Greenshot.Addons.Core;
using Greenshot.Core.Interfaces;

namespace Greenshot.Core
{
	/// <summary>
	/// This class contains everything on a capture, from the moment of the capture itself to the export
	/// </summary>
	public class CaptureContext {

		private readonly Dictionary<string, string> _metaData = new Dictionary<string, string>();
		private string _filenamePattern;
		private string _filename;
		private string _title;
		private DateTimeOffset _captureTaken = new DateTimeOffset();

		/// <summary>
		/// the actual screen capture
		/// </summary>
		public IElement<BitmapSource> Capture {
			get;
			set;
		}

		/// <summary>
		/// the cursor
		/// </summary>
		public IElement<BitmapSource> MouseCursor {
			get;
			set;
		}

		/// <summary>
		/// This is used to crop the capture to a certain location/size
		/// </summary>
		public NativeRect CropRect {
			get;
			set;
		}

		/// <summary>
		/// This can be used for a region-capture which is not rectangle based
		/// </summary>
		public Geometry ClipArea {
			get;
			set;
		}

		/// <summary>
		/// When using this value, the default (from CoreConfiguration) FilenamePattern is given unless a value was set
		/// </summary>
		public string FilenamePattern {
			get {
				if (_filenamePattern != null) {
					return _filenamePattern;
				}
				return Conf.OutputFileFilenamePattern;
			}
			set {
				_filenamePattern = value;
			}
		}

		/// <summary>
		/// When using this value, the default (from FilenamePattern) Filename is given unless a value was set
		/// The filename is without extension!!
		/// </summary>
		public string Filename {
			get {
				if (_filename != null) {
					return _filename;
				}
				return FilenameHelper.GetFilenameWithoutExtensionFromPattern(FilenamePattern, this);
			}
			set {
				_filename = value;
			}
		}

		/// <summary>
		/// Title of the capture, this can e.g. be the title of the selected window.
		/// </summary>
		public string Title {
			get {
				return _title;
			}
			set {
				_title = value;
			}
		}

		/// <summary>
		/// Template to use, unless overridden by the destination
		/// </summary>
		public string Template {
			get;
			set;
		}

		/// <summary>
		/// The time that the capture was taken
		/// </summary>
		public DateTimeOffset DateTime {
			get {
				return _captureTaken;
			}
			set {
				_captureTaken = value;
			}
		}

		/// <summary>
		/// Special meta-data for this capture
		/// </summary>
		public Dictionary<string, string> MetaData {
			get {
				return _metaData;
			}
		}

		/// <summary>
		/// Add to the meta-data for this capture
		/// </summary>
		public void AddMetaData(string key, string value) {
			if (_metaData.ContainsKey(key)) {
				_metaData[key] = value;
			} else {
				_metaData.Add(key, value);
			}
		}
	}
}
