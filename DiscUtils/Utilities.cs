//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

namespace DiscUtils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    internal delegate TResult Func<in T, out TResult>(T arg);
    
    internal static class Utilities
    {
        /// <summary>
        /// Round up a value to a multiple of a unit size.
        /// </summary>
        /// <param name="value">The value to round up</param>
        /// <param name="unit">The unit (the returned value will be a multiple of this number)</param>
        /// <returns>The rounded-up value</returns>
        public static long RoundUp(long value, long unit)
        {
            return ((value + (unit - 1)) / unit) * unit;
        }

        /// <summary>
        /// Round up a value to a multiple of a unit size.
        /// </summary>
        /// <param name="value">The value to round up</param>
        /// <param name="unit">The unit (the returned value will be a multiple of this number)</param>
        /// <returns>The rounded-up value</returns>
        public static int RoundUp(int value, int unit)
        {
            return ((value + (unit - 1)) / unit) * unit;
        }

        /// <summary>
        /// Calculates the CEIL function.
        /// </summary>
        /// <param name="numerator">The value to divide</param>
        /// <param name="denominator">The value to divide by</param>
        /// <returns>The value of CEIL(numerator/denominator)</returns>
        public static uint Ceil(uint numerator, uint denominator)
        {
            return (numerator + (denominator - 1)) / denominator;
        }

        /// <summary>
        /// Calculates the CEIL function.
        /// </summary>
        /// <param name="numerator">The value to divide</param>
        /// <param name="denominator">The value to divide by</param>
        /// <returns>The value of CEIL(numerator/denominator)</returns>
        public static long Ceil(long numerator, long denominator)
        {
            return (numerator + (denominator - 1)) / denominator;
        }

        /// <summary>
        /// Converts between two arrays.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the source array</typeparam>
        /// <typeparam name="U">The type of the elements of the destination array</typeparam>
        /// <param name="source">The source array</param>
        /// <param name="func">The function to map from source type to destination type</param>
        /// <returns>The resultant array</returns>
        public static U[] Map<T, U>(ICollection<T> source, Func<T, U> func)
        {
            U[] result = new U[source.Count];
            int i = 0;

            foreach (T sVal in source)
            {
                result[i++] = func(sVal);
            }

            return result;
        }

        #region Bit Twiddling

        public static ushort BitSwap(ushort value)
        {
            return (ushort)(((value & 0x00FF) << 8) | ((value & 0xFF00) >> 8));
        }

        public static uint BitSwap(uint value)
        {
            return ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0x00FF0000) >> 8) | ((value & 0xFF000000) >> 24);
        }

        public static void WriteBytesLittleEndian(ushort val, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(val & 0xFF);
            buffer[offset + 1] = (byte)((val >> 8) & 0xFF);
        }

        public static void WriteBytesLittleEndian(uint val, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(val & 0xFF);
            buffer[offset + 1] = (byte)((val >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((val >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((val >> 24) & 0xFF);
        }

        public static void WriteBytesBigEndian(ushort val, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(val >> 8);
            buffer[offset + 1] = (byte)(val & 0xFF);
        }

        public static void WriteBytesBigEndian(uint val, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)((val >> 24) & 0xFF);
            buffer[offset + 1] = (byte)((val >> 16) & 0xFF);
            buffer[offset + 2] = (byte)((val >> 8) & 0xFF);
            buffer[offset + 3] = (byte)(val & 0xFF);
        }

        public static ushort ToUInt16LittleEndian(byte[] buffer, int offset)
        {
            return (ushort)(((buffer[offset + 1] << 8) & 0xFF00) | ((buffer[offset + 0] << 0) & 0x00FF));
        }

        public static uint ToUInt32LittleEndian(byte[] buffer, int offset)
        {
            return (uint)(((buffer[offset + 3] << 24) & 0xFF000000U) | ((buffer[offset + 2] << 16) & 0x00FF0000U)
                | ((buffer[offset + 1] << 8) & 0x0000FF00U) | ((buffer[offset + 0] << 0) & 0x000000FFU));
        }

        /// <summary>
        /// Primitive conversion from Unicode to ASCII that preserves special characters.
        /// </summary>
        /// <param name="value">The string to convert</param>
        /// <param name="dest">The buffer to fill</param>
        /// <param name="offset">The start of the string in the buffer</param>
        /// <param name="count">The number of characters to convert</param>
        /// <remarks>The built-in ASCIIEncoding converts characters of codepoint > 127 to ?,
        /// this preserves those code points by removing the top 16 bits of each character.</remarks>
        public static void StringToBytes(string value, byte[] dest, int offset, int count)
        {
            char[] chars = value.ToCharArray();

            int i = 0;
            while (i < chars.Length)
            {
                dest[i + offset] = (byte)chars[i];
                ++i;
            }

            while (i < count)
            {
                dest[i + offset] = 0;
                ++i;
            }
        }

        /// <summary>
        /// Primitive conversion from ASCII to Unicode that preserves special characters.
        /// </summary>
        /// <param name="data">The data to convert</param>
        /// <param name="offset">The first byte to convert</param>
        /// <param name="count">The number of bytes to convert</param>
        /// <returns>The string</returns>
        /// <remarks>The built-in ASCIIEncoding converts characters of codepoint > 127 to ?,
        /// this preserves those code points.</remarks>
        public static string BytesToString(byte[] data, int offset, int count)
        {
            char[] result = new char[count];

            for (int i = 0; i < count; ++i)
            {
                result[i] = (char)data[i + offset];
            }

            return new string(result);
        }

        #endregion

        #region Path Manipulation
        /// <summary>
        /// Extracts the directory part of a path.
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The directory part</returns>
        public static string GetDirectoryFromPath(string path)
        {
            string trimmed = path.TrimEnd('\\');

            int index = trimmed.LastIndexOf('\\');
            if (index < 0)
            {
                return string.Empty; // No directory, just a file name
            }

            return trimmed.Substring(0, index);
        }

        /// <summary>
        /// Extracts the file part of a path.
        /// </summary>
        /// <param name="path">The path to process</param>
        /// <returns>The file part of the path</returns>
        public static string GetFileFromPath(string path)
        {
            string trimmed = path.Trim('\\');

            int index = trimmed.LastIndexOf('\\');
            if (index < 0)
            {
                return trimmed; // No directory, just a file name
            }

            return trimmed.Substring(index + 1);
        }

        /// <summary>
        /// Combines two paths.
        /// </summary>
        /// <param name="a">The first part of the path</param>
        /// <param name="b">The second part of the path</param>
        /// <returns>The combined path</returns>
        public static string CombinePaths(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || (b.Length > 0 && b[0] == '\\'))
            {
                return b;
            }
            if (string.IsNullOrEmpty(b))
            {
                return a;
            }
            return a.TrimEnd('\\') + '\\' + b.TrimStart('\\');
        }

        /// <summary>
        /// Resolves a relative path into an absolute one.
        /// </summary>
        /// <param name="basePath">The base path to resolve from</param>
        /// <param name="relativePath">The relative path</param>
        /// <returns>The absolute path, so far as it can be resolved.  If the
        /// <paramref name="relativePath"/> contains more '..' characters than the
        /// base path contains levels of directory, the resultant string will be relative.
        /// For example: (TEMP\Foo.txt, ..\..\Bar.txt) gives (..\Bar.txt).</returns>
        private static string ResolveRelativePath(string basePath, string relativePath)
        {
            List<string> pathElements = new List<string>(basePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));
            if (!basePath.EndsWith(@"\", StringComparison.Ordinal) && pathElements.Count > 0)
            {
                pathElements.RemoveAt(pathElements.Count - 1);
            }

            pathElements.AddRange(relativePath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries));

            int pos = 1;
            while (pos < pathElements.Count)
            {
                if (pathElements[pos] == ".")
                {
                    pathElements.RemoveAt(pos);
                }
                else if (pathElements[pos] == ".." && pos > 0 && pathElements[pos - 1][0] != '.')
                {
                    pathElements.RemoveAt(pos);
                    pathElements.RemoveAt(pos - 1);
                    pos--;
                }
                else
                {
                    pos++;
                }
            }

            string merged = string.Join(@"\", pathElements.ToArray());
            if (relativePath.EndsWith(@"\", StringComparison.Ordinal))
            {
                merged += @"\";
            }

            if (basePath.StartsWith(@"\\", StringComparison.Ordinal))
            {
                merged = @"\\" + merged;
            }
            else if (basePath.StartsWith(@"\", StringComparison.Ordinal))
            {
                merged = @"\" + merged;
            }

            return merged;
        }

        public static string ResolvePath(string basePath, string path)
        {
            if (!path.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveRelativePath(basePath, path);
            }
            return path;
        }

        #endregion

        #region Stream Manipulation
        /// <summary>
        /// Read bytes until buffer filled or EOF.
        /// </summary>
        /// <param name="stream">The stream to read</param>
        /// <param name="buffer">The buffer to populate</param>
        /// <param name="offset">Offset in the buffer to start</param>
        /// <param name="length">The number of bytes to read</param>
        /// <returns>The number of bytes actually read.</returns>
        public static int ReadFully(Stream stream, byte[] buffer, int offset, int length)
        {
            int totalRead = 0;
            int numRead = stream.Read(buffer, offset, length);
            while (numRead > 0)
            {
                totalRead += numRead;
                if (totalRead == length)
                {
                    break;
                }

                numRead = stream.Read(buffer, offset + totalRead, length - totalRead);
            }

            return totalRead;
        }

        /// <summary>
        /// Read bytes until buffer filled or throw IOException.
        /// </summary>
        /// <param name="stream">The stream to read</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>The data read from the stream</returns>
        public static byte[] ReadFully(Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            if (ReadFully(stream, buffer, 0, count) == count)
            {
                return buffer;
            }
            throw new IOException("Unable to complete read of " + count + " bytes");
        }

        #endregion

        #region Filesystem Support

        /// <summary>
        /// Converts a 'standard' wildcard file/path specification into a regular expression.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert</param>
        /// <returns>The resultant regular expression</returns>
        /// <remarks>
        /// The wildcard * (star) matches zero or more characters (including '.'), and ?
        /// (question mark) matches precisely one character (except '.').
        /// </remarks>
        public static Regex ConvertWildcardsToRegEx(string pattern)
        {
            if (!pattern.Contains("."))
            {
                pattern += ".";
            }

            string query = "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", "[^.]") + "$";
            return new Regex(query, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public static FileAttributes FileAttributesFromUnixFileType(UnixFileType fileType)
        {
            switch (fileType)
            {
                case UnixFileType.Fifo:
                    return FileAttributes.Device | FileAttributes.System;
                case UnixFileType.Character:
                    return FileAttributes.Device | FileAttributes.System;
                case UnixFileType.Directory:
                    return FileAttributes.Directory;
                case UnixFileType.Block:
                    return FileAttributes.Device | FileAttributes.System;
                case UnixFileType.Regular:
                    return FileAttributes.Normal;
                case UnixFileType.Link:
                    return FileAttributes.ReparsePoint;
                case UnixFileType.Socket:
                    return FileAttributes.Device | FileAttributes.System;
                default:
                    return 0;
            }
        }
        #endregion
    }
}
