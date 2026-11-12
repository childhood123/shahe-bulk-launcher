using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ShaheLauncher.Core.Configuration
{
    internal static class IniTextFile
    {
        internal static Tuple<string, string> Read(string path)
        {
            if (!File.Exists(path)) return Tuple.Create(string.Empty, "utf-8");

            byte[] data = File.ReadAllBytes(path);
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                return Decode(data, "utf-8-sig", 3);
            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
                return Decode(data, "utf-16", 2);
            if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
                return Decode(data, "utf-16BE", 2);

            // Without a BOM, trying UTF-16 can misinterpret otherwise valid GBK bytes.
            foreach (string name in new[] { "utf-8", "gbk", "mbcs" })
            {
                try { return Decode(data, name, 0); }
                catch (DecoderFallbackException) { }
            }

            Encoding fallback = Encoding.GetEncoding(65001,
                EncoderFallback.ExceptionFallback, new DecoderReplacementFallback(string.Empty));
            return Tuple.Create(fallback.GetString(data), "utf-8");
        }

        internal static void Write(string path, string text, string encodingName)
        {
            Encoding encoding = GetEncoding(encodingName);
            // Encode before opening the destination so an unrepresentable value cannot truncate it.
            byte[] data = encoding.GetPreamble().Concat(encoding.GetBytes(text ?? string.Empty)).ToArray();
            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            File.WriteAllBytes(path, data);
        }

        private static Tuple<string, string> Decode(byte[] data, string name, int offset)
        {
            return Tuple.Create(GetEncoding(name).GetString(data, offset, data.Length - offset), name);
        }

        private static Encoding GetEncoding(string name)
        {
            if (string.Equals(name, "utf-8-sig", StringComparison.OrdinalIgnoreCase))
                return new UTF8Encoding(true, true);
            if (string.Equals(name, "utf-8", StringComparison.OrdinalIgnoreCase))
                return new UTF8Encoding(false, true);
            if (string.Equals(name, "utf-16", StringComparison.OrdinalIgnoreCase))
                return new UnicodeEncoding(false, true, true);
            if (string.Equals(name, "utf-16BE", StringComparison.OrdinalIgnoreCase))
                return new UnicodeEncoding(true, true, true);
            if (string.Equals(name, "gbk", StringComparison.OrdinalIgnoreCase))
                return Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            if (string.Equals(name, "mbcs", StringComparison.OrdinalIgnoreCase))
                return Encoding.GetEncoding(Encoding.Default.CodePage,
                    EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            return Encoding.GetEncoding(name, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        }
    }
}
