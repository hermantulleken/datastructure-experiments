namespace DataStructures.Tiling;

public interface IWidth
{
#pragma warning disable CA2252
	static abstract int Width { get; }
	static abstract ulong LastRowMask { get; }
	static abstract int EmptyBitCount { get; }
	static abstract ulong EmptyBitMask { get; }
#pragma warning restore CA2252
}

public sealed class Width65 : IWidth
{
	public static int Width => 65;
	public static ulong LastRowMask => 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << 1;
	
	public static int EmptyBitCount => 63;
}

public sealed class Width4 : IWidth
{
	private const int _Width = 4;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width10 : IWidth
{
	private const int _Width = 10;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width11 : IWidth
{
	private const int _Width = 11;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}


public sealed class Width12 : IWidth
{
	private const int _Width = 12;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width16 : IWidth
{
	private const int _Width = 16;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width20 : IWidth
{
	private const int _Width = 20;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width24 : IWidth
{
	private const int _Width = 24;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width28 : IWidth
{
	private const int _Width = 28;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width30 : IWidth
{
	private const int _Width = 30;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width32 : IWidth
{
	private const int _Width = 32;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public sealed class Width35 : IWidth
{
	private const int _Width = 35;
	public static int Width => _Width;
	public static ulong LastRowMask { get; } = (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount  { get; } = 64 - _Width;
}

public sealed class Width40 : IWidth
{
	private const int _Width = 40;
	public static int Width => _Width;
	public static ulong LastRowMask => (1ul << _Width) - 1;
	public static ulong EmptyBitMask { get; } = ulong.MaxValue << _Width;
	
	public static int EmptyBitCount => 64 - _Width;
}

public class Width64 : IWidth
{
	public static int Width => 64;
	public static ulong LastRowMask => ulong.MaxValue;
	public static int EmptyBitCount => 0;
	public static ulong EmptyBitMask => 0;
}

public class Width128 : IWidth
{
	public static int Width => 128;
	public static ulong LastRowMask => ulong.MaxValue;
	public static int EmptyBitCount => 0;
	public static ulong EmptyBitMask => 0;
}
