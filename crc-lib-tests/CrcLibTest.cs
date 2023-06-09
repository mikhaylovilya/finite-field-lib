using crc_lib;
using finite_fields;

namespace crc_lib_tests
{
	[TestClass]
	public class CrcLibTest
	{
		private byte[] PadRight(byte[] src, int count)
		{
			byte[] result = new byte[count];
			for (int i = 0; i < src.Length; i++)
				result[i] = src[i];

			for (int i = src.Length; i < count; i++)
				result[i] = 0;

			return result;
		}
		[TestMethod]
		public void Crc_CalculateTest1()
		{
			var gf256 = new FiniteField(2, new int[] { 1, 0, 1, 1, 1, 0, 0, 0, 1 });
			var mainpolyn = new byte[] { 1, 110, 67, 0 };
			var msg = new byte[] { 20, 90, 77, 0, 89, 0, 34, 2, 2 };

			var checkSumInstance = new CheckSum(mainpolyn); //little-endian
			var checkSum = checkSumInstance.CalculateCheckSum(msg);
			var polynOfCheckSum = new Polyn<FiniteFieldElement>(2, checkSum
				.Select(x => gf256.Get(new byte[] { x })).ToArray());

			var intermediatePolyn = new byte[] { 1, 110, 67, 0, 1 };
			var p1 = new Polyn<FiniteFieldElement>(2, intermediatePolyn
				.Select(x => gf256.Get(new byte[] { x }))
				.ToArray());

			var p2 = new Polyn<FiniteFieldElement>(2, msg
				.Select(x => gf256.Get(new byte[] { x }))
				.ToArray());

			var p = p2 % p1;
			var bytesOfp = PadRight(p
				.GetValue()
				.Select(x => x.GetByte().First())
				.ToArray(), 4);

			bool cnf = p.Equals(polynOfCheckSum) && checkSum.SequenceEqual(bytesOfp);
			Assert.IsTrue(cnf);
		}
		[TestMethod]
		public void Crc_CheckTest1()
		{
			var mainpolyn = new byte[] { 17, 97, 0, 76 };
			var msg = new byte[] { 0, 91, 71, 0, 89, 0, 34, 2, 2 };

			var checkSumInstance = new CheckSum(mainpolyn);
			var checkSum = checkSumInstance.CalculateCheckSum(msg); // 32 63 172 111

			var cr = checkSumInstance.Check(checkSum, new byte[] { 32, 63, 172, 111});
			Assert.IsTrue(cr);
		}

		[TestMethod]
		public void Demo()
		{
			// provide four bytes as an input to the constructor
            var checkSumInstance = new CheckSum(new byte[] { 1, 89, 12, 45});

            //create message as array of bytes
            // notice: byte order is little-endian
            var msg = new byte[] { 245, 1, 90, 0, 12, 46, 23, 85, 85, 0, 1};
			//calculate checksum
			var initialCheckSum = checkSumInstance.CalculateCheckSum(msg); // {236, 83, 61, 83}

			//check
			var randomCheckSum = new byte[] { 90, 1, 78, 13 };
			var copiedCheckSum = new byte[initialCheckSum.Length];
			initialCheckSum.CopyTo(copiedCheckSum, 0);

			bool check1 = checkSumInstance.Check(initialCheckSum, randomCheckSum); // false
            bool check2 = checkSumInstance.Check(initialCheckSum, copiedCheckSum); // true
        }
	}
}