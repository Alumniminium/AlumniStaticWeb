using System;
using System.IO;
using System.IO.Compression;
using AlumniStaticWeb.Helpers;

namespace AlumniStaticWeb.IO
{
    public class WebFile
    {
        private byte[] _content;
        private string _name;
        private string _mimeType;

        public readonly string Path;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _name = System.IO.Path.GetFileName(Path);
                return _name;
            }
        }


        public string MimeType
        {
            get
            {
                if (string.IsNullOrEmpty(_mimeType))
                {
                    if (MimeTypes.Mappings.TryGetValue(System.IO.Path.GetExtension(Path), out var mimetype))
                        _mimeType = mimetype;
                    else
                        _mimeType = "text/plain";
                }

                return _mimeType;
            }
        }

        public byte[] Content
        {
            get
            {
                if (_content == null)
                {
                    if (File.Exists(Environment.CurrentDirectory + "/" + Path))
                    {
                        _content = File.ReadAllBytes(Path);//Compress(File.ReadAllBytes(Path));
                    }
                    else
                        _content = new byte[0];
                }
                return _content;
            }
        }
        public static byte[] Compress(byte[] raw)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionLevel.Optimal, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        public WebFile(string path)
        {
            Path = path;
        }
    }
}