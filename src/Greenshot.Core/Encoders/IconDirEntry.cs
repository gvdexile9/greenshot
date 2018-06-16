using System;

namespace Greenshot.Core.Encoders
{
    public struct IconDirEntry {

		//Width, in pixels, of the image
		public readonly byte bWidth;
		//Height, in pixels, of the image
		public readonly byte bHeight;
		//Number of colors in image (0 if >=8bpp)
		public readonly byte bColorCount;
		//Reserved ( must be 0)
		public readonly byte bReserved;
		//Color Planes
		public readonly ushort wPlanes;
		//Bits per pixel
		public readonly ushort wBitCount;
		//How many bytes in this resource?
		public readonly uint dwBytesInRes;
		//Where in the file is this image?

		public readonly uint dwImageOffset;
		public IconDirEntry(ushort width, ushort height, ushort bitsPerPixel, uint resSize, uint imageOffset) {
			if (width == 256) {
				bWidth = Convert.ToByte(0);
			} else {
				bWidth = Convert.ToByte(width);
			}
			if (height == 256) {
				bHeight = Convert.ToByte(0);
			} else {
				bHeight = Convert.ToByte(height);
			}
			if (bitsPerPixel == 4) {
				bColorCount = Convert.ToByte(16);
			} else {
				bColorCount = Convert.ToByte(0);
			}
			bReserved = Convert.ToByte(0);
			wPlanes = Convert.ToUInt16(1);
			wBitCount = bitsPerPixel;
			dwBytesInRes = resSize;
			dwImageOffset = imageOffset;
		}
	}
}
