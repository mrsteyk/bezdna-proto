using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Apex
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
        public ulong Unk28 { get; private set; }

        public ulong SizeDecompressed { get; private set; }
        public ulong Unk38 { get; private set; }
        public ulong Unk40 { get; private set; }

        public ushort SkipShit { get; private set; } // 0x48
        public ushort Unk4A { get; private set; } // 0x4A
        public ushort SectionsNum { get; private set; } // 0x4C
        public ushort DataChunksNum { get; private set; } //0x4E

        public ushort PartRPak { get; private set; } // 0x50

        public ushort Unk52 { get; private set; } // 0x52
        public uint Unk54 { get; private set; }
        public uint NumFiles { get; private set; }
        public uint Unk5C { get; private set; }

        public uint Unk60 { get; private set; }
        public uint Unk64 { get; private set; }
        public uint Unk68 { get; private set; }
        public uint Unk6C { get; private set; }

        public uint Unk70 { get; private set; }
        public uint Unk74 { get; private set; }
        public ulong Unk78 { get; private set; }

        public RPakHeader(FileStream file)
        {
            if (file.Length < Utils.HEADER_SIZE7)
                throw new Exception("File is too short!");

            file.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(file);

            Magic = reader.ReadUInt32(); // 0-4
            if (Magic != 0x6b615052)
                throw new Exception("Invalid magic!");
            Version = reader.ReadUInt16(); // 4-6
            if (Version != 8)
                throw new Exception("Invalid version!");
            Flags = reader.ReadUInt16(); // 6-8

            //if (ShouldLLA)
            //    throw new Exception("ShouldLLA");

            RPakType = reader.ReadUInt64();
            Unk10 = reader.ReadUInt64();
            SizeDisk = reader.ReadUInt64();

            Unk20 = reader.ReadUInt64();
            Unk28 = reader.ReadUInt64();

            SizeDecompressed = reader.ReadUInt64();
            Unk38 = reader.ReadUInt64();
            Unk40 = reader.ReadUInt64();

            SkipShit = reader.ReadUInt16(); // 48
            Unk4A = reader.ReadUInt16(); // 4A
            SectionsNum = reader.ReadUInt16(); // 4C
            DataChunksNum = reader.ReadUInt16(); // 4E

            //reader.BaseStream.Position = 0x50; // wtf???

            PartRPak = reader.ReadUInt16();
            Unk52 = reader.ReadUInt16();

            Unk54 = reader.ReadUInt32();
            NumFiles = reader.ReadUInt32();
            Unk5C = reader.ReadUInt32(); // new unk48?

            Unk60 = reader.ReadUInt32();
            Unk64 = reader.ReadUInt32();
            Unk68 = reader.ReadUInt32();
            Unk6C = reader.ReadUInt32();
            
            Unk70 = reader.ReadUInt32();
            Unk74 = reader.ReadUInt32();

            Unk78 = reader.ReadUInt64();

            if (file.Position != Utils.HEADER_SIZE8)
                throw new Exception("Я шиз!");
        }
    }
}
