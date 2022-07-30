using System;

namespace DataStructures.Hash;

/// <summary>
/// This implementation is adapted from <see href="https://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html" />.
/// </summary>
/// <remarks>
/// <p>The original converted the two long values that represent the calculated hash
/// to a bit array. This version return the two longs as a tuple. </p>
/// <p> The original also did no reset all the internal values when calculating a hash. this one does, so 
/// the same function can be used to calculate hashes multiple times.</p>
/// </remarks>
public class Murmur3HashFunction
{
	// 128 bit output, 64 bit platform version

	public const ulong ReadSize = 16;
	private const ulong C1 = 0x87c37b91114253d5L;
	private const ulong C2 = 0x4cf5ad432745937fL;

	private ulong length;
	private readonly uint seed; // if want to start with a seed, create a constructor
	private ulong h1;
	private ulong h2;

	/// <summary>
	/// Constructs a new Murmur3HashFunction hash function with the given seed.
	/// </summary>
	public Murmur3HashFunction(uint seed = default)
	{
		this.seed = seed;
	}
	
	public (ulong, ulong) ComputeHash(byte[] bytes)
	{
		ProcessBytes(bytes);
		CalculateHash();
		return (h1, h2);
	}

	private void MixBody(ulong k1, ulong k2)
	{
		h1 ^= MixKey1(k1);

		h1 = h1.RotateLeft(27);
		h1 += h2;
		h1 = h1 * 5 + 0x52dce729;

		h2 ^= MixKey2(k2);

		h2 = h2.RotateLeft(31);
		h2 += h1;
		h2 = h2 * 5 + 0x38495ab5;
	}

	private static ulong MixKey1(ulong k1)
	{
		k1 *= C1;
		k1 = k1.RotateLeft(31);
		k1 *= C2;
		return k1;
	}

	private static ulong MixKey2(ulong k2)
	{
		k2 *= C2;
		k2 = k2.RotateLeft(33);
		k2 *= C1;
		return k2;
	}

	private static ulong MixFinal(ulong k)
	{
		// avalanche bits

		k ^= k >> 33;
		k *= 0xff51afd7ed558ccdL;
		k ^= k >> 33;
		k *= 0xc4ceb9fe1a85ec53L;
		k ^= k >> 33;
		return k;
	}

	private void ProcessBytes(byte[] bytes)
	{
		h2 = 0; //This line was missing from the original. Setting h2 here allows us to call CalculateHash more than once. 
		h1 = seed;
		length = 0L;

		int pos = 0;
		ulong remaining = (ulong)bytes.Length;

		// read 128 bits, 16 bytes, 2 longs in each cycle
		while (remaining >= ReadSize)
		{
			ulong k1 = bytes.GetUInt64(pos);
			pos += 8;

			ulong k2 = bytes.GetUInt64(pos);
			pos += 8;

			length += ReadSize;
			remaining -= ReadSize;

			MixBody(k1, k2);
		}

		// if the input MOD 16 != 0
		if (remaining > 0)
		{
			ProcessBytesRemaining(bytes, remaining, pos);
		}
		else
		{
			GLDebug.Assert(bytes.Length % 16 == 0);
		}
	}

	private void ProcessBytesRemaining(byte[] bytes, ulong remaining, int pos)
	{
		ulong k1 = 0;
		ulong k2 = 0;
		length += remaining;

		// little endian (x86) processing
		switch (remaining)
		{
			case 15:
				k2 ^= (ulong)bytes[pos + 14] << 48; // fall through
				goto case 14;
			case 14:
				k2 ^= (ulong)bytes[pos + 13] << 40; // fall through
				goto case 13;
			case 13:
				k2 ^= (ulong)bytes[pos + 12] << 32; // fall through
				goto case 12;
			case 12:
				k2 ^= (ulong)bytes[pos + 11] << 24; // fall through
				goto case 11;
			case 11:
				k2 ^= (ulong)bytes[pos + 10] << 16; // fall through
				goto case 10;
			case 10:
				k2 ^= (ulong)bytes[pos + 9] << 8; // fall through
				goto case 9;
			case 9:
				k2 ^= bytes[pos + 8]; // fall through
				goto case 8;
			case 8:
				k1 ^= bytes.GetUInt64(pos);
				break;
			case 7:
				k1 ^= (ulong)bytes[pos + 6] << 48; // fall through
				goto case 6;
			case 6:
				k1 ^= (ulong)bytes[pos + 5] << 40; // fall through
				goto case 5;
			case 5:
				k1 ^= (ulong)bytes[pos + 4] << 32; // fall through
				goto case 4;
			case 4:
				k1 ^= (ulong)bytes[pos + 3] << 24; // fall through
				goto case 3;
			case 3:
				k1 ^= (ulong)bytes[pos + 2] << 16; // fall through
				goto case 2;
			case 2:
				k1 ^= (ulong)bytes[pos + 1] << 8; // fall through
				goto case 1;
			case 1:
				k1 ^= bytes[pos]; // fall through
				break;
			default:
				throw new Exception("Something went wrong with remaining bytes calculation.");
		}

		h1 ^= MixKey1(k1);
		h2 ^= MixKey2(k2);
	}

	private void CalculateHash()
	{
		h1 ^= length;
		h2 ^= length;

		h1 += h2;
		h2 += h1;

		h1 = MixFinal(h1);
		h2 = MixFinal(h2);

		h1 += h2;
		h2 += h1;
	}
	
	private byte[] GetHash()
	{
		byte[] hash = new byte[ReadSize];

		Array.Copy(BitConverter.GetBytes(h1), 0, hash, 0, 8);
		Array.Copy(BitConverter.GetBytes(h2), 0, hash, 8, 8);

		return hash;
	}
}

public static class IntHelpers
{
	public static ulong RotateLeft(this ulong original, int bits)
		=> (original << bits) | (original >> (64 - bits));

	public static ulong RotateRight(this ulong original, int bits)
		=> (original >> bits) | (original << (64 - bits));

	public static unsafe ulong GetUInt64(this byte[] bb, int pos)
	{
		// we only read aligned longs, so a simple casting is enough
		fixed (byte* pByte = &bb[pos])
		{
			return *((ulong*)pByte);
		}
	}
}
