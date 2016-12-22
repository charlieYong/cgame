using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

using SevenZip.Compression.LZMA;


namespace ZR_MasterTools
{
    public static class FileCompression
    {
        static int _headerBytesLen = 4;
        public static string _file_compression_tempare = "_zronline_temp_compression";

        public static void Compress(string src, string dst) {
            if (!File.Exists(src)) {
                return;
            }
            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            FileStream inStream = new FileStream(src, FileMode.Open);
            FileStream outStream = new FileStream(_file_compression_tempare, FileMode.Create);
            try {
                // 写入4位的随机标识头，用于防止被其他人直接解压
                outStream.Write(BitConverter.GetBytes(UnityEngine.Random.Range(0, int.MaxValue)), 0, _headerBytesLen);
                outStream.Write(BitConverter.GetBytes(inStream.Length), 0, 8);
                encoder.WriteCoderProperties(outStream);
                encoder.Code(inStream, outStream, inStream.Length, -1L, null);
            }
            finally {
                inStream.Close();
                outStream.Close();
            }
            if (File.Exists(dst)) {
                File.Delete(dst);
            }
            File.Move(_file_compression_tempare, dst);
        }

        public static void Decompress(string src, string dst) {
            if (!File.Exists(src)) {
                return;
            }
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            FileStream inStream = new FileStream(src, FileMode.Open);
            FileStream outStream = new FileStream(_file_compression_tempare, FileMode.Create);
            try {
                // Header
                byte[] headerBytes = new byte[_headerBytesLen];
                inStream.Read(headerBytes, 0, headerBytes.Length);
                // File Size
                byte[] lenBytes = new byte[8];
                inStream.Read(lenBytes, 0, lenBytes.Length);
                // Property
                byte[] propertyBytes = new byte[5];
                inStream.Read(propertyBytes, 0, propertyBytes.Length);

                decoder.SetDecoderProperties(propertyBytes);
                decoder.Code(inStream, outStream, inStream.Length, BitConverter.ToInt64(lenBytes, 0), null);
            }
            finally {
                inStream.Close();
                outStream.Close();
            }
            if (File.Exists(dst)) {
                File.Delete(dst);
            }
            File.Move(_file_compression_tempare, dst);
        }

        public static byte[] Decompress(byte[] data) {
            if (null == data || data.Length <= 0) {
                return null;
            }
            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            MemoryStream inStream = new MemoryStream(data);
            MemoryStream outStream = new MemoryStream();
            // Header
            byte[] headerBytes = new byte[_headerBytesLen];
            inStream.Read(headerBytes, 0, headerBytes.Length);
            // File Size
            byte[] lenBytes = new byte[8];
            inStream.Read(lenBytes, 0, lenBytes.Length);
            // Property
            byte[] propertyBytes = new byte[5];
            inStream.Read(propertyBytes, 0, propertyBytes.Length);

            decoder.SetDecoderProperties(propertyBytes);
            decoder.Code(inStream, outStream, inStream.Length, BitConverter.ToInt64(lenBytes, 0), null);
            return outStream.GetBuffer();
        }
    }
}