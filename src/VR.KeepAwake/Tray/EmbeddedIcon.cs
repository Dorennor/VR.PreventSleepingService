using System.Buffers.Binary;

using static VR.KeepAwake.Tray.NativeMethods;

namespace VR.KeepAwake.Tray;

/// <summary>Loads icons from inside the exe, so that the published app stays a single file.</summary>
internal static class EmbeddedIcon
{
    private const int HeaderSize = 6;
    private const int EntrySize = 16;
    private const int IconFormatVersion = 0x00030000;

    /// <returns>An icon handle to pass to DestroyIcon, or 0 if it could not be loaded.</returns>
    public static nint Load(string resourceName, int size)
    {
        using var stream = typeof(EmbeddedIcon).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return 0;
        }

        var file = new byte[stream.Length];
        stream.ReadExactly(file);

        var image = FindBestImage(file, size);
        return CreateIconFromResourceEx(image, image.Length, fIcon: true, IconFormatVersion, size, size, 0);
    }

    /// <summary>Scaling down looks better than scaling up, so this prefers the smallest image that is large enough.</summary>
    private static ReadOnlySpan<byte> FindBestImage(ReadOnlySpan<byte> file, int size)
    {
        var count = BinaryPrimitives.ReadUInt16LittleEndian(file[4..]);
        var best = file[HeaderSize..];
        var bestWidth = 0;

        for (var i = 0; i < count; i++)
        {
            var entry = file.Slice(HeaderSize + (i * EntrySize), EntrySize);

            // A width of 0 means 256 in the ICO format.
            var width = entry[0] == 0 ? 256 : entry[0];
            var isBetter = bestWidth == 0
                || (bestWidth < size ? width > bestWidth : width >= size && width < bestWidth);

            if (isBetter)
            {
                bestWidth = width;
                best = entry;
            }
        }

        var length = BinaryPrimitives.ReadInt32LittleEndian(best[8..]);
        var offset = BinaryPrimitives.ReadInt32LittleEndian(best[12..]);
        return file.Slice(offset, length);
    }
}