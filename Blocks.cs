﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BCn
{
	/// <summary>
	/// An unsigned single-channel block (BC3 alpha, BC4 R, BC5 R and G).
	/// </summary>
	public struct UnsignedRBlock
	{
		public ulong PackedValue;

		public byte R0
		{
			get { return (byte)PackedValue; }
			set { PackedValue = (PackedValue & ~0xFFUL) | value; }
		}
		
		public byte R1
		{
			get { return (byte)(PackedValue >> 8); }
			set { PackedValue = (PackedValue & ~0xFF00UL) | ((ulong)value << 8); }
		}

		public byte R2 { get { return GetR( 2 ); } }
		public byte R3 { get { return GetR( 3 ); } }
		public byte R4 { get { return GetR( 4 ); } }
		public byte R5 { get { return GetR( 5 ); } }
		public byte R6 { get { return GetR( 6 ); } }
		public byte R7 { get { return GetR( 7 ); } }

		public byte GetR( int index )
		{
			if( index < 0 || index >= 8 )
				throw new ArgumentOutOfRangeException( "index" );

			byte r0 = R0;
			if( index == 0 )
				return r0;

			byte r1 = R1;
			if( index == 1 )
				return r1;

			index--;

			if( r0 <= r1 )
			{
				if( index == 5 )
					return 0x00;

				if( index == 6 )
					return 0xFF;

				return unchecked( (byte)((r0 * (5 - index) + r1 * index) / 5) );
			}

			return unchecked( (byte)((r0 * (7 - index) + r1 * index) / 7) );
		}

		/// <summary>
		/// Computes all of the R values.
		/// </summary>
		public void GetRValues( byte[] palette, int index = 0 )
		{
			if( palette == null )
				throw new ArgumentNullException( "palette" );

			if( index < 0 || palette.Length - index < 8 )
				throw new ArgumentOutOfRangeException( "index" );

			byte r0 = R0;
			byte r1 = R1;

			palette[index + 0] = r0;
			palette[index + 1] = r1;

			if( r0 <= r1 )
			{
				palette[index + 2] = unchecked( (byte)((r0 * 4 + r1 * 1) / 5) );
				palette[index + 3] = unchecked( (byte)((r0 * 3 + r1 * 2) / 5) );
				palette[index + 4] = unchecked( (byte)((r0 * 2 + r1 * 3) / 5) );
				palette[index + 5] = unchecked( (byte)((r0 * 1 + r1 * 4) / 5) );

				palette[index + 6] = 0x00;
				palette[index + 7] = 0xFF;
			}
			else
			{
				palette[index + 2] = unchecked( (byte)((r0 * 6 + r1 * 1) / 7) );
				palette[index + 3] = unchecked( (byte)((r0 * 5 + r1 * 2) / 7) );
				palette[index + 4] = unchecked( (byte)((r0 * 4 + r1 * 3) / 7) );
				palette[index + 5] = unchecked( (byte)((r0 * 3 + r1 * 4) / 7) );
				palette[index + 6] = unchecked( (byte)((r0 * 2 + r1 * 5) / 7) );
				palette[index + 7] = unchecked( (byte)((r0 * 1 + r1 * 6) / 7) );
			}
		}

		/// <summary>
		/// Computes all of the R values.
		/// </summary>
		public void GetRValues( float[] palette, int index = 0 )
		{
			if( palette == null )
				throw new ArgumentNullException( "palette" );

			if( index < 0 || palette.Length - index < 8 )
				throw new ArgumentOutOfRangeException( "index" );

			byte sr0 = R0;
			byte sr1 = R1;

			float r0 = sr0 / 255F;
			float r1 = sr1 / 255F;

			palette[index + 0] = r0;
			palette[index + 1] = r1;

			if( r0 <= r1 )
			{
				palette[index + 2] = (r0 * 4F + r1 * 1F) / 5F;
				palette[index + 3] = (r0 * 3F + r1 * 2F) / 5F;
				palette[index + 4] = (r0 * 2F + r1 * 3F) / 5F;
				palette[index + 5] = (r0 * 1F + r1 * 4F) / 5F;

				palette[index + 6] = 0F;
				palette[index + 7] = 1F;
			}
			else
			{
				palette[index + 2] = (r0 * 6F + r1 * 1F) / 7F;
				palette[index + 3] = (r0 * 5F + r1 * 2F) / 7F;
				palette[index + 4] = (r0 * 4F + r1 * 3F) / 7F;
				palette[index + 5] = (r0 * 3F + r1 * 4F) / 7F;
				palette[index + 6] = (r0 * 2F + r1 * 5F) / 7F;
				palette[index + 7] = (r0 * 1F + r1 * 6F) / 7F;
			}
		}

		/// <summary>
		/// Gets or sets individual block index entries.
		/// </summary>
		public int this[int index]
		{
			get
			{
				if( index < 0 || index >= 16 )
					throw new ArgumentOutOfRangeException( "index" );

				return (int)((PackedValue >> (16 + 3 * index)) & 0x7);
			}

			set
			{
				if( index < 0 || index >= 16 )
					throw new ArgumentOutOfRangeException( "index" );
				if( value < 0 || value > 0x7 )
					throw new ArgumentOutOfRangeException( "value" );

				int shift = 16 + 3 * index;
				PackedValue = (PackedValue & ~(0x7UL << shift)) | ((ulong)value << shift);
			}
		}

		/// <summary>
		/// Gets or sets the block index for a given coordinate within the block.
		/// </summary>
		public int this[int x, int y]
		{
			get
			{
				if( x < 0 || x >= 4 )
					throw new ArgumentOutOfRangeException( "x" );
				if( y < 0 || y >= 4 )
					throw new ArgumentOutOfRangeException( "y" );

				return (int)((PackedValue >> (16 + 3 * (y * 4 + x))) & 0x7);
			}

			set
			{
				if( x < 0 || x >= 4 )
					throw new ArgumentOutOfRangeException( "x" );
				if( y < 0 || y >= 4 )
					throw new ArgumentOutOfRangeException( "y" );

				int shift = 16 + 3 * (y * 4 + x);
				PackedValue = (PackedValue & ~(0x7UL << shift)) | ((ulong)value << shift);
			}
		}
	}

	/// <summary>
	/// A signed single-channel block (BC3 alpha, BC4 R, BC5 R and G).
	/// </summary>
	public struct SignedRBlock
	{
		public ulong PackedValue;

		public sbyte R0
		{
			get
			{
				var ret = (sbyte)PackedValue;
				return ret != -128 ? ret : (sbyte)-127;
			}
			set
			{
				if( value == -128 )
					value = -127;

				PackedValue = (PackedValue & ~0xFFUL) | (byte)value;
			}
		}

		public sbyte R1
		{
			get
			{
				var ret = (sbyte)(PackedValue >> 8);
				return ret != -128 ? ret : (sbyte)-127;
			}
			set
			{
				if( value == -128 )
					value = -127;

				PackedValue = (PackedValue & ~0xFF00UL) | ((ulong)(byte)value << 8);
			}
		}

		public sbyte R2 { get { return GetR( 2 ); } }
		public sbyte R3 { get { return GetR( 3 ); } }
		public sbyte R4 { get { return GetR( 4 ); } }
		public sbyte R5 { get { return GetR( 5 ); } }
		public sbyte R6 { get { return GetR( 6 ); } }
		public sbyte R7 { get { return GetR( 7 ); } }

		public sbyte GetR( int index )
		{
			if( index < 0 || index >= 8 )
				throw new ArgumentOutOfRangeException( "index" );

			sbyte r0 = R0;
			if( index == 0 )
				return r0;

			sbyte r1 = R1;
			if( index == 1 )
				return r1;

			index--;

			if( r0 <= r1 )
			{
				if( index == 5 )
					return -127;

				if( index == 6 )
					return 127;

				return unchecked( (sbyte)((r0 * (5 - index) + r1 * index) / 5) );
			}

			return unchecked( (sbyte)((r0 * (7 - index) + r1 * index) / 7) );
		}

		/// <summary>
		/// Computes all of the R values.
		/// </summary>
		public void GetRValues( sbyte[] palette, int index = 0 )
		{
			if( palette == null )
				throw new ArgumentNullException( "palette" );

			if( index < 0 || palette.Length - index < 8 )
				throw new ArgumentOutOfRangeException( "index" );

			sbyte r0 = R0;
			sbyte r1 = R1;

			palette[index + 0] = r0;
			palette[index + 1] = r1;

			if( r0 <= r1 )
			{
				palette[index + 2] = unchecked( (sbyte)((r0 * 4 + r1 * 1) / 5) );
				palette[index + 3] = unchecked( (sbyte)((r0 * 3 + r1 * 2) / 5) );
				palette[index + 4] = unchecked( (sbyte)((r0 * 2 + r1 * 3) / 5) );
				palette[index + 5] = unchecked( (sbyte)((r0 * 1 + r1 * 4) / 5) );

				palette[index + 6] = -127;
				palette[index + 7] = 127;
			}
			else
			{
				palette[index + 2] = unchecked( (sbyte)((r0 * 6 + r1 * 1) / 7) );
				palette[index + 3] = unchecked( (sbyte)((r0 * 5 + r1 * 2) / 7) );
				palette[index + 4] = unchecked( (sbyte)((r0 * 4 + r1 * 3) / 7) );
				palette[index + 5] = unchecked( (sbyte)((r0 * 3 + r1 * 4) / 7) );
				palette[index + 6] = unchecked( (sbyte)((r0 * 2 + r1 * 5) / 7) );
				palette[index + 7] = unchecked( (sbyte)((r0 * 1 + r1 * 6) / 7) );
			}
		}

		/// <summary>
		/// Computes all of the R values.
		/// </summary>
		public void GetRValues( float[] palette, int index = 0 )
		{
			if( palette == null )
				throw new ArgumentNullException( "palette" );

			if( index < 0 || palette.Length - index < 8 )
				throw new ArgumentOutOfRangeException( "index" );

			sbyte sr0 = R0;
			sbyte sr1 = R1;

			float r0 = sr0 / 127F;
			float r1 = sr1 / 127F;

			palette[index + 0] = r0;
			palette[index + 1] = r1;

			if( r0 <= r1 )
			{
				palette[index + 2] = (r0 * 4F + r1 * 1F) / 5F;
				palette[index + 3] = (r0 * 3F + r1 * 2F) / 5F;
				palette[index + 4] = (r0 * 2F + r1 * 3F) / 5F;
				palette[index + 5] = (r0 * 1F + r1 * 4F) / 5F;

				palette[index + 6] = -1F;
				palette[index + 7] = 1F;
			}
			else
			{
				palette[index + 2] = (r0 * 6F + r1 * 1F) / 7F;
				palette[index + 3] = (r0 * 5F + r1 * 2F) / 7F;
				palette[index + 4] = (r0 * 4F + r1 * 3F) / 7F;
				palette[index + 5] = (r0 * 3F + r1 * 4F) / 7F;
				palette[index + 6] = (r0 * 2F + r1 * 5F) / 7F;
				palette[index + 7] = (r0 * 1F + r1 * 6F) / 7F;
			}
		}

		/// <summary>
		/// Gets or sets individual block index entries.
		/// </summary>
		public int this[int index]
		{
			get
			{
				if( index < 0 || index >= 16 )
					throw new ArgumentOutOfRangeException( "index" );

				return (int)((PackedValue >> (16 + 3 * index)) & 0x7);
			}

			set
			{
				if( index < 0 || index >= 16 )
					throw new ArgumentOutOfRangeException( "index" );
				if( value < 0 || value > 0x7 )
					throw new ArgumentOutOfRangeException( "value" );

				int shift = 16 + 3 * index;
				PackedValue = (PackedValue & ~(0x7UL << shift)) | ((ulong)value << shift);
			}
		}

		/// <summary>
		/// Gets or sets the block index for a given coordinate within the block.
		/// </summary>
		public int this[int x, int y]
		{
			get
			{
				if( x < 0 || x >= 4 )
					throw new ArgumentOutOfRangeException( "x" );
				if( y < 0 || y >= 4 )
					throw new ArgumentOutOfRangeException( "y" );

				return (int)((PackedValue >> (16 + 3 * (y * 4 + x))) & 0x7);
			}

			set
			{
				if( x < 0 || x >= 4 )
					throw new ArgumentOutOfRangeException( "x" );
				if( y < 0 || y >= 4 )
					throw new ArgumentOutOfRangeException( "y" );

				int shift = 16 + 3 * (y * 4 + x);
				PackedValue = (PackedValue & ~(0x7UL << shift)) | ((ulong)value << shift);
			}
		}
	}
}
