//Written for Warp Engine games.
//Cathedral 3-D https://www.feardemic-games.com/cathedral-3-d
//Chapel 3-D: The Ascent https://store.steampowered.com/app/1904680/
using System.IO;
using System.Collections.Generic;
using System;

namespace wUNpak
{
    class Program
    {
        public static BinaryReader br;
        static void Main(string[] args)
        {
            br = new BinaryReader(File.OpenRead(args[0]));
            if (new string(br.ReadChars(4)) != "wpk\0")
                throw new System.Exception("Not a Warp Engine wpak file.");
            br.ReadUInt32();
            uint subfileDataOffset = (uint)(br.ReadUInt32() + br.BaseStream.Position);
            uint numSubfiles = br.ReadUInt32();
            List<SubFileMetaData> subFileMetaData = new();
            for (int i = 0; i < numSubfiles; i++)
            {
                byte[] sizeBytes = br.ReadBytes(br.ReadUInt16() + 1);
                if (sizeBytes.Length == 3)
                {
                    Array.Resize(ref sizeBytes, sizeBytes.Length + 1);
                    sizeBytes[^1] = br.ReadByte();
                }
                while (sizeBytes.Length < 4)
                    Array.Resize(ref sizeBytes, sizeBytes.Length + 1);
                subFileMetaData.Add(new SubFileMetaData()
                {
                    size = BitConverter.ToInt32(sizeBytes, 0),
                    name = new(br.ReadChars(br.ReadInt16()))
                });
            }
            br.BaseStream.Position = subfileDataOffset;
            for (int i = 0; i < numSubfiles; i++)
            {
                SubFileMetaData sub = subFileMetaData[i];
                string newFolder = Path.GetDirectoryName(args[0]);

                sub.name = sub.name.Replace("/", "\\");
                if (sub.name.Contains(@"\"))
                {
                    newFolder += "\\" + Path.GetDirectoryName(sub.name);
                    sub.name = Path.GetFileName(sub.name);
                }

                MemoryStream outstream = new();
                
                Directory.CreateDirectory(newFolder);
                BinaryWriter bw = new(File.OpenWrite(newFolder + "\\" + sub.name));
                bw.Write(br.ReadBytes(sub.size));
                bw.Close();
            }
        }
        public struct SubFileMetaData
        {
            public int size;
            public string name;
        }
    }
}

