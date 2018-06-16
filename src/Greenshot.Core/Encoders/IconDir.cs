using System;

namespace Greenshot.Core.Encoders
{
    public struct IconDir {

		//Reserved (must be 0)
		public readonly ushort idReserved;
		//Resource Type (1 for icons)
		public readonly ushort idType;
		//How many images?

		public readonly ushort idCount;
		public IconDir(ushort count) {
			idReserved = Convert.ToUInt16(0);
			idType = Convert.ToUInt16(1);
			idCount = count;
		}

	}
}
