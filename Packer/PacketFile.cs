using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace obisoft.net.packer
{
    public class PacketFile
    {

        private long _position;
        internal OnProgress _onread;

        public PacketFolder Folder { get; private set; }
        public string Filename { get; private set; }
        public long Filesize { get; private set; }
         
        public long GetPositionBytes()
        {
            return _position;
        }

        public PacketFile(string filename, long filesize, long position, PacketFolder folder)
        {
            this.Folder = folder;
            this.Filename = filename;
            this.Filesize = filesize;
            this._position = position;
        }

        public IEnumerable<byte[]> IterBytes(int len=1024)
        {
            PacketFolder.MyStream.Position = _position;
            BinaryReader reader = new BinaryReader(PacketFolder.MyStream);
            if (true)
            {
                long read = 0;
                while (read < Filesize)
                {
                    int readlen = len;
                    if (readlen > Filesize - read)
                        readlen = (int) (Filesize - read);
                    byte[] bytes = reader.ReadBytes(readlen);
                    read += bytes.Length;
                    if (_onread != null)
                        _onread(Folder.Name, Filename, read, Filesize);
                    yield return bytes;
                }
            }
        }
    }
}