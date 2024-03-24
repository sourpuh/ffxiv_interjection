using FFXIVClientStructs.FFXIV.Client.Graphics;
using System.Numerics;

namespace Interjection;
public static class Extensions
{
    public static ByteColor ToByteColor(this Vector4 v)
    {
        v *= 255.0f;
        ByteColor c = new()
        {
            R = (byte)v.X,
            G = (byte)v.Y,
            B = (byte)v.Z,
            A = (byte)v.W
        };
        return c;
    }

    public static Vector4 ToVector4(this ByteColor c)
    {
        return new Vector4(c.R, c.G, c.B, c.A) / 255.0f;
    }
}
