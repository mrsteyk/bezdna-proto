using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Titanfall2
{
    class RPakHeader
    {
        public uint Magic { get; private set; }
        public ushort Version { get; private set; }
        public ushort Flags { get; private set; }
        public bool Compressed
        {
            get
            {
                return ((Flags >> 8) & 0xFF) == 1;
            }
        }
        public bool ShouldLLA // prolly UI rpaks in Apex?
        {
            get
            {
                return (Flags & 0x11) != 0;
            }
        }

        public ulong RPakType { get; private set; }
        public ulong Unk10 { get; private set; }

        public ulong SizeDisk { get; private set; }
        public ulong Unk20 { get; private set; }

        public ulong SizeDecompressed { get; private set; }
        public ulong Unk30 { get; private set; }

        public ushort SkipShit { get; private set; }
        public ushort SectionsNum { get; private set; }
        public ushort DataChunksNum { get; private set; }
        public ushort PartRPak { get; private set; } // aka skip_16, not 0 for %s(%02u).rpak

        public uint Unk40 { get; private set; }
        public uint NumFiles { get; private set; }
        public uint Unk48 { get; private set; }
        public uint Unk4c { get; private set; }

        public uint Unk50 { get; private set; }
        public uint Unk54 { get; private set; }

        public RPakHeader(FileStream file)
        {
            if (file.Length < Utils.HEADER_SIZE7)
                throw new Exception("File is too short!");

            file.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(file);
            
            Magic = reader.ReadUInt32();
            if (Magic != 0x6b615052)
                throw new Exception("Invalid magic!");
            Version = reader.ReadUInt16();
            if (Version != 7)
                throw new Exception("Invalid version!");
            Flags = reader.ReadUInt16();

            RPakType = reader.ReadUInt64();
            Unk10 = reader.ReadUInt64();

            SizeDisk = reader.ReadUInt64();
            Unk20 = reader.ReadUInt64();

            SizeDecompressed = reader.ReadUInt64();
            Unk30 = reader.ReadUInt64();

            if (SizeDecompressed > uint.MaxValue)
                throw new Exception("Bruhhhh");

            SkipShit = reader.ReadUInt16();
            SectionsNum = reader.ReadUInt16();
            DataChunksNum = reader.ReadUInt16();
            PartRPak = reader.ReadUInt16();

            Unk40 = reader.ReadUInt32();
            NumFiles = reader.ReadUInt32();
            Unk48 = reader.ReadUInt32();
            Unk4c = reader.ReadUInt32();

            Unk50 = reader.ReadUInt32();
            Unk54 = reader.ReadUInt32();

            if (reader.BaseStream.Position != Utils.HEADER_SIZE7)
                throw new Exception("Bruh");
        }
    }
}
