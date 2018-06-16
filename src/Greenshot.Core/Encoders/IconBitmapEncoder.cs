using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Greenshot.Core.Encoders
{
    public class IconBitmapEncoder : DispatcherObject {
        private readonly IconBitmapFramesCollection _frames = new IconBitmapFramesCollection();

        /// <summary>
        /// 
        /// </summary>
        public IList<BitmapFrame> Frames => _frames;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream) {
			_frames.SortAscending();
			BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF32);

			ushort framesCount = Convert.ToUInt16(_frames.Count);
			const ushort fileHeaderLength = 6;
			const ushort frameHeaderLength = 16;

			IconDir fileHeader = new IconDir(framesCount);
			writer.Write(fileHeader.idReserved);
			writer.Write(fileHeader.idType);
			writer.Write(fileHeader.idCount);

			byte[][] data = new byte[framesCount][];

			foreach (var frame in _frames) {
				int frameIndex = _frames.IndexOf(frame);
				if (frame.PixelWidth == 256) {
					data[frameIndex] = GetPngData(frame);
				} else {
					data[frameIndex] = GetBmpData(frame);
				}
			}

			uint frameDataOffset = fileHeaderLength;
			frameDataOffset += (uint)(frameHeaderLength * framesCount);

			foreach (var frame in _frames) {
				int frameIndex = _frames.IndexOf(frame);
				if (frameIndex > 0) {
					frameDataOffset += Convert.ToUInt32(data[frameIndex - 1].Length);
				}
				IconDirEntry frameHeader = new IconDirEntry((ushort)frame.PixelWidth, (ushort)frame.PixelHeight, Convert.ToUInt16(frame.Format.BitsPerPixel), Convert.ToUInt32(data[frameIndex].Length), frameDataOffset);
				writer.Write(frameHeader.bWidth);
				writer.Write(frameHeader.bHeight);
				writer.Write(frameHeader.bColorCount);
				writer.Write(frameHeader.bReserved);
				writer.Write(frameHeader.wPlanes);
				writer.Write(frameHeader.wBitCount);
				writer.Write(frameHeader.dwBytesInRes);
				writer.Write(frameHeader.dwImageOffset);
			}

			foreach (byte[] frameData in data) {
				writer.Write(frameData);
			}

		}

		private byte[] GetPngData(BitmapFrame frame) {
			using (var dataStream = new MemoryStream())
		    {
		        var encoder = new PngBitmapEncoder();
		        encoder.Frames.Add(frame);
		        encoder.Save(dataStream);
		        return dataStream.ToArray();
            }
			
		}

		private byte[] GetBmpData(BitmapFrame frame) {
			var dataStream = new MemoryStream();
			BmpBitmapEncoder encoder = new BmpBitmapEncoder();
			encoder.Frames.Add(frame);
			encoder.Save(dataStream);
			encoder = null;
			dataStream.Position = 14;
			BinaryReader dataStreamReader = new BinaryReader(dataStream, Encoding.UTF32);
			MemoryStream outDataStream = new MemoryStream();
			BinaryWriter outDataStreamWriter = new BinaryWriter(outDataStream, Encoding.UTF32);
			outDataStreamWriter.Write(dataStreamReader.ReadUInt32());
			outDataStreamWriter.Write(dataStreamReader.ReadInt32());
			int height = dataStreamReader.ReadInt32();
			if (height > 0) {
				height = height * 2;
			} else if (height < 0) {
				height = -(height * 2);
			} else {
				height = 0;
			}
			outDataStreamWriter.Write(height);
			for (int i = 26; i <= dataStream.Length - 1; i++) {
				outDataStream.WriteByte((byte)(dataStream.ReadByte()));
			}
			byte[] data = outDataStream.ToArray();
			outDataStreamWriter.Close();
			outDataStream.Close();
			dataStreamReader.Close();
			dataStream.Close();
			return data;
		}


		public static BitmapSource Get4BitImage(BitmapSource source)
		{
		    if (source == null)
		    {
		        throw new ArgumentNullException(nameof(source));
		    }

		    return new FormatConvertedBitmap(source, PixelFormats.Indexed4, BitmapPalettes.Halftone8, 0);
		}

		public static BitmapSource Get8BitImage(BitmapSource source) {
		    if (source == null)
		    {
		        throw new ArgumentNullException(nameof(source));
		    }

		    return new FormatConvertedBitmap(source, PixelFormats.Indexed8, BitmapPalettes.Halftone256, 0);
		}

		public static BitmapSource Get24plus8BitImage(BitmapSource source) {
		    if (source == null)
		    {
		        throw new ArgumentNullException(nameof(source));
		    }

		   return new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
		}

		public static BitmapSource GetResized(BitmapSource source, int size) {
			BitmapSource backup = source.Clone();
			try {
				TransformedBitmap scaled = new TransformedBitmap();
				scaled.BeginInit();
				scaled.Source = source;
				double scX = size / (double)source.PixelWidth;
				double scy = size / (double)source.PixelHeight;
				ScaleTransform tr = new ScaleTransform(scX, scy, source.Width / 2, source.Height / 2);
				scaled.Transform = tr;
				scaled.EndInit();
				source = scaled;
			} catch (Exception) {
				source = backup;
			}
			return source;
		}
	}
}
