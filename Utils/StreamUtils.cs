using System.IO;

namespace GIF_Viewer.Utils
{
    /// <summary>
    /// Contains static helper methods for streams
    /// </summary>
    public static class StreamUtils
    {
        /// <summary>
        /// Copies a sequence of bytes from this stream into another stream
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="target">The target stream</param>
        /// <param name="length">The number of bytes to copy to the target stream</param>
        /// <returns>The number of bytes copied in this operation</returns>
        public static int WriteToStream(this Stream source, Stream target, int length)
        {
            if (length <= 0)
                return 0;

            byte[] bytes = new byte[length];
            int copied = source.Read(bytes, 0, length);
            target.Write(bytes, 0, copied);

            return copied;
        }
    }
}