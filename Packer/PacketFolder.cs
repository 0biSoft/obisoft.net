using obisoft.net.encoders;
using System;
using System.Collections.Generic;
using System.IO;

namespace obisoft.net.packer
{
    public class PacketFolder : IDisposable
    {
        public static Stream MyStream { get;internal set; }
        internal string path;
        
        public string Name { get;internal set; }
        public List<PacketFile> Files { get; internal set; } = new List<PacketFile>();

        public PacketFile SearchFile(string filename, bool in_all = false) => SearchFileInFolder(this, filename, in_all);

        public static PacketFile SearchFileInFolder(PacketFolder fold,string filename,bool in_all=false)
        {
            foreach (var fi in fold.Files)
            {
                if (fi.Filename == filename)
                    return fi;
            }
            if(in_all)
            foreach (var fo in fold.Folders)
            {
                return SearchFileInFolder(fo, filename);
            }
            return null;
        }

        public List<PacketFolder> Folders { get; internal set; } = new List<PacketFolder>();

        
        private static PacketFile ReadFile(Stream stream,PacketFolder folder,OnProgress progress=null)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);
                if (true)
                {
                    string name = reader.ReadString();
                    long filesize = reader.ReadInt64();
                    var file = new PacketFile(name, filesize, stream.Position, folder);
                    file._onread = progress;
                    return file;
                }
            }catch{}
            return null;
        }
        private static PacketFolder ReadFolder(Stream stream,OnProgress progress=null)
        {
            try
            {
                PacketFolder folder = new PacketFolder();
                BinaryReader reader = new BinaryReader(stream);
                if (true)
                {
                    folder.Name = reader.ReadString();
                    int lenFi = reader.ReadInt32();
                    int lenFo = reader.ReadInt32();
                    for (int i = 0; i < lenFi; i++)
                    {
                        bool isfolder = reader.ReadBoolean();
                        var fi = ReadFile(stream, folder, progress);
                        folder.Files.Add(fi);
                        stream.Position += fi.Filesize;
                    }
                    for (int i = 0; i < lenFo; i++)
                    {
                        bool isfolder = reader.ReadBoolean();
                        folder.Folders.Add(ReadFolder(stream,progress));
                    }
                }

                return folder;
            }catch{}
            return null;
        }
        internal static PacketFolder From(Stream stream,string password="NONE",OnProgress progress=null)
        {
            try
            {
                bool valid = false;
                BinaryReader reader = new BinaryReader(stream);
                if (true)
                {
                    string psw = S6Encoder.decrypt(reader.ReadString());
                    if (psw == password)
                    {
                        bool isfolder = reader.ReadBoolean();
                        var folder = ReadFolder(stream,progress);
                        return folder;
                    }
                }

                if (!valid)
                    throw new Exception("Password Invalid!");
            }catch{}
            return null;
        }

        public static PacketFolder From(string pathfile, string password = "NONE")
        {
            PacketFolder folder = null;
            using (Stream stream = File.OpenRead(pathfile))
                folder = From(stream,password);
            if (folder != null)
            {
                folder.path = pathfile;
                PacketFolder.MyStream = File.OpenRead(pathfile);
            }
            return folder;
        }


        private static long WriteFile(string path, BinaryWriter writer)
        {
            long packedlen = 0;
            FileInfo fi = new FileInfo(path);
            writer.Write(false);
            writer.Write(fi.Name);
            writer.Write(fi.Length);
            using (Stream stream = File.OpenRead(path))
            {
                byte[] bytes = new byte[1024];
                int read = 0;
                while ((read = stream.Read(bytes,0,bytes.Length))!=0)
                {
                    Array.Resize(ref bytes,read);
                    //byte[] compress = Compresion.Compress(bytes);
                    //packedlen += bytes.Length - compress.Length;
                    //writer.Write(compress);
                    writer.Write(bytes);
                }
            }

            return packedlen;
        }
        private static void WriteFolder(string path, BinaryWriter writer)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            writer.Write(true);
            writer.Write(di.Name);
            int lenFi = di.GetFiles().Length;
            int lenFo = di.GetDirectories().Length;
            writer.Write(lenFi);
            writer.Write(lenFo);
            foreach (var fi in di.GetFiles())
            {
                WriteFile(fi.FullName,writer);
            }
            foreach (var fi in di.GetDirectories())
            {
                WriteFolder(fi.FullName,writer);
            }
        }
        private static void WriteFolder(string name,int len, BinaryWriter writer)
        {
            writer.Write(true);
            writer.Write(name);
            writer.Write(len);
        }
        public static void Pack(string path, string savename,string password="NONE")
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                using (Stream stream = File.Create(savename))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(S6Encoder.encrypt(password));
                        WriteFolder(path,writer);
                    }
                }
            }
            else if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                using (Stream stream = File.Create(savename))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(S6Encoder.encrypt(password));
                        WriteFolder(fi.Name,1,writer);
                        WriteFile(path,writer);
                    }
                }
            }
        }

        public static void ExtractFolder(PacketFolder folder, string path,OnProgress progress=null)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (Directory.Exists(path))
            {
                foreach (PacketFile fi in folder.Files)
                {
                    using (Stream stream = File.Create($"{path}/{fi.Filename}"))
                    {
                        long i = 0;
                        foreach (byte[] chunks in fi.IterBytes())
                        {
                            stream.Write(chunks,0,chunks.Length);
                            i += chunks.Length;
                            if (progress!=null)
                                progress(folder.Name, fi.Filename, i, fi.Filesize);
                        }
                    }
                }

                foreach (PacketFolder fo in folder.Folders)
                {
                    ExtractFolder(fo,$"{path}/{fo.Name}");
                }
            }
        }
        
        public static void Extract(string pack, string path,OnProgress progress=null)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (File.Exists(pack))
            {
                using (PacketFolder packF = PacketFolder.From(pack))
                {
                    ExtractFolder(packF,$"{path}/{packF.Name}",progress);
                }
            }
            else
                throw new Exception("Not File Exist!");
        }
        
        public void Dispose()
        {
            PacketFolder.MyStream?.Dispose();
        }
    }
}