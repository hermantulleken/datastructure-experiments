using System;
using System.Numerics;

namespace DataStructures;

public readonly struct Int2 : IEquatable<Int2>
{
	public readonly int X;
	public readonly int Y;

	public static readonly Int2 Zero = new(0, 0);
	public static readonly Int2 One = new(1, 1);
	public static readonly Int2 Up = new(0, 1);
	public static readonly Int2 Right = new(1, 0);
	public static readonly Int2 Down = new(0, -1);
	public static readonly Int2 Left = new(-1, 0);
	public static Int2 NegOne = new(-1, -1);

	public Int2 LeftNeighbor => this - Right;
	public Int2 RightNeighbor => this + Right;
		
	public Int2 DownNeighbor => this - Up;
	public Int2 UpNeighbor => this + Up;
		
	public Int2(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static Int2 operator+(Int2 point1, Int2 point2) => new(point1.X + point2.X, point1.Y + point2.Y);
	public static Int2 operator-(Int2 point1, Int2 point2) => new(point1.X - point2.X, point1.Y - point2.Y);
	public static Int2 operator-(Int2 point1) => new(-point1.X, -point1.Y);
	public static Int2 operator*(int factor, Int2 point) => new(factor * point.X, factor * point.Y);
	public static Int2 operator*(Int2 point, int factor) => new(factor * point.X, factor * point.Y);
		
	public static Vector2 operator*(float factor, Int2 point) => new(factor * point.X, factor * point.Y);
	public static Vector2 operator*(Int2 point, float factor) => new(factor * point.X, factor * point.Y);
	public static Int2 operator/(Int2 point, int divisor) => new(point.X / divisor, point.Y / divisor);
		
	public bool Equals(Int2 other) => Equals(other, this);

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		var objectToCompareWith = (Int2) obj;

		return objectToCompareWith.X == X && objectToCompareWith.Y == Y;

	}
		
	public static bool operator ==(Int2 point1, Int2 point2) => point1.Equals(point2);

	public static bool operator !=(Int2 point1, Int2 point2) => !point1.Equals(point2);


	public static implicit operator Vector2(Int2 point) => new(point.X, point.Y);
		

	public override string ToString() => $"({X}, {Y})";

	public override int GetHashCode() => HashCode.Combine(X.GetHashCode(), Y.GetHashCode());

	public static implicit operator Int2((int x, int y) point) => new (point.x, point.y);
	public static implicit operator (int x, int y)(Int2 point) => new (point.X, point.Y);

	public Int2 Rotate90() => new Int2(-Y, X);
	public Int2 Rotate180() => new Int2(-X, -Y);
	public Int2 Rotate270() => new Int2(Y, -X);
	public Int2 ReflectX() => new Int2(-X, Y);
	public Int2 ReflectXRotate90() => new Int2(-Y, -X);
	public Int2 ReflectXRotate180() => new Int2(X, -Y);
	public Int2 ReflectXRotate270() => new Int2(Y, X);
}