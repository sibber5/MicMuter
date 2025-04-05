using System;
using System.Runtime.InteropServices;

namespace MicMuter.NativeMisc.Windows.Types;

#pragma warning disable IDE0032 // Use auto property

/// <summary>The <a href="https://learn.microsoft.com/windows/win32/api/windef/ns-windef-point">POINT</a> structure defines the <i>x</i>- and <i>y</i>-coordinates of a point.</summary>
/// <remarks>
/// <para>The <a href="https://learn.microsoft.com/windows/win32/api/windef/ns-windef-point">POINT</a> structure is identical to the <a href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-pointl">POINTL</a> structure.</para>
/// <para><see href="https://docs.microsoft.com/windows/desktop/api/windef/ns-windef-point">Read more on docs.microsoft.com</see>.</para>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct Point(int x, int y) : IEquatable<Point>
{
    /// <summary>Specifies the <i>x</i>-coordinate of the point.</summary>
    public int X
    {
        get => _x;
#if NETCOREAPP3_0_OR_GREATER
        init => _x = value;
#endif
    }
    private readonly int _x = x;

    /// <summary>Specifies the <i>y</i>-coordinate of the point.</summary>
    public int Y
    {
        get => _y;
#if NETCOREAPP3_0_OR_GREATER
        init => _y = value;
#endif
    }
    private readonly int _y = y;

    public static explicit operator System.Drawing.Point(Point p) => new(p.X, p.Y);

    public static explicit operator Point(System.Drawing.Point p) => new(p.X, p.Y);

    public static bool operator !=(Point left, Point right) => !(left == right);

    public static bool operator ==(Point left, Point right) => left.Equals(right);

    public override int GetHashCode() => HashCode.Combine(_x, _y);

    public override bool Equals(object? obj) => obj is Point point && Equals(point);

    public bool Equals(Point other) => _x == other._x && _y == other._y;

    public void Deconstruct(out int X, out int Y)
    {
        X = this.X;
        Y = this.Y;
    }

    public override string ToString() => $"Point {{ {nameof(X)} = {X}, {nameof(Y)} = {Y} }}";
}
