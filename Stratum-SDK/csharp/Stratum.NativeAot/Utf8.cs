using System.Runtime.InteropServices;
using System.Text;

namespace Stratum.NativeAot;

internal static unsafe class Utf8
{
    internal static readonly UTF8Encoding Encoding = new(false, true);

    internal static int Size(string value, int limit)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Contains('\0')) throw new ArgumentException("Embedded NUL is not supported.");
        int count = Encoding.GetByteCount(value);
        if (count > limit) throw new ArgumentException($"UTF-8 string exceeds {limit} bytes.");
        return count + 1;
    }

    internal static void Write(string value, Span<byte> bytes)
    {
        int count = Encoding.GetBytes(value, bytes);
        bytes[count] = 0;
    }

    internal static string Read(byte* value, int limit)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        int count = 0;
        while (count <= limit && value[count] != 0) count++;
        if (count > limit) throw new ArgumentException("Native UTF-8 string exceeds its limit.");
        return Encoding.GetString(new ReadOnlySpan<byte>(value, count));
    }
}

internal unsafe ref struct NativeText
{
    internal byte* Pointer { get; private set; }

    internal NativeText(string text, int limit)
    {
        int size = Utf8.Size(text, limit);
        Pointer = (byte*)Marshal.AllocHGlobal(size);
        try { Utf8.Write(text, new Span<byte>(Pointer, size)); }
        catch { Marshal.FreeHGlobal((nint)Pointer); Pointer = null; throw; }
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal((nint)Pointer);
        Pointer = null;
    }
}
