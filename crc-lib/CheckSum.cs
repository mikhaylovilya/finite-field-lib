﻿using finite_fields;
using System.Security.Cryptography;

namespace crc_lib
{
	public class CheckSum
	{
		private const int _mpl = 5; // main polynomial length
		private const int _ffc = 2; //finite field characteristic
		private readonly int[] _factorpolyn = { 1, 0, 1, 1, 1, 0, 0, 0, 1 };
		private readonly FiniteField gf256;
		private readonly Polyn<FiniteFieldElement> _mainpolyn;
		public CheckSum(byte[] bytes) //little-endian
		{
			if (bytes.Length != _mpl - 1) throw new ArgumentException("Length of byte array should equal to 4");

			gf256 = new FiniteField(_ffc, _factorpolyn);
			var ElementPolyn = new FiniteFieldElement[_mpl];
			for (int i = 0; i < bytes.Length; i++)
			{
				ElementPolyn[i] = gf256.Get(new byte[] { bytes[i] } ); // overload for byte and not byte[]
			}
			ElementPolyn[_mpl - 1] = gf256.GetMultiplicativeIdent(); // get 1
			_mainpolyn = new Polyn<FiniteFieldElement>(_ffc, ElementPolyn);
		}

		public byte[] CalculateCheckSum(byte[] message) //little-endian
		{
			var ffElementsOfBytes = message
				.Select(x => gf256.Get(new byte[] { x } ))
				.ToArray();

			var PolynOfElements = new Polyn<FiniteFieldElement>(_ffc, ffElementsOfBytes); 
			var remainder = PolynOfElements % _mainpolyn;

			var remainderValue = remainder
				.GetValue()
				.Select(x => x.GetByte().First())
				.ToArray();

			byte[] result = PadRight(remainderValue, _mpl - 1);
			return result;
		}
		private byte[] PadRight(byte[] src, int count) // count is 4
		{
			byte[] result = new byte[count];
			for (int i = 0; i < src.Length; i++) 
				result[i] = src[i];

			for (int i = src.Length; i < count; i++)
				result[i] = 0;

			return result;
		}
		public bool Check(byte[] initialCheckSum, byte[] currCheckSum) 
		{ 
			return initialCheckSum.SequenceEqual(currCheckSum);
		}

	}
}