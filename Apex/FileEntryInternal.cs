using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Apex
{
    class FileEntryInternal
    {
        public ulong GUID { get; private set; }
        public ulong NamePad { get; private set; } // it's 8 bytes, so game can override it with a pointer...

        public DataDescriptor Description { get; private set; } // MANDATORY file description
        public DataDescriptor Data { get; private set; } // OPTIONAL file data

        public ulong StarpakOffset { get; private set; } // -1 if not in StarPak
        public ulong StarpakOffsetOptional { get; private set; }

        public ushort Unk30 { get; private set; }
        public ushort Unk32 { get; private set; }

        public uint Unk34 { get; private set; }
        public uint StartIdx { get; private set; }
        public uint Unk3c { get; private set; }
        public uint Count { get; private set; }

        // was 8 padding in my C++ RE project
        public uint DescriptionSize { get; private set; }
        public uint DescriptionAlign { get; private set; } // ??? Desc for first txtr is 0|0x38|0x8, first matl is 0x40|0xd0|0xC

        public string ShortName { get; private set; } // C# moog1k

        public FileEntryInternal(BinaryReader reader)
        {
            GUID = reader.ReadUInt64();
            NamePad = reader.ReadUInt64();

            DataDescriptor d;
            d.id = reader.ReadUInt32();
            d.offset = reader.ReadUInt32();
            Description = d;

            d.id = reader.ReadUInt32();
            d.offset = reader.ReadUInt32();
            Data = d;

            StarpakOffset = reader.ReadUInt64();
            StarpakOffsetOptional = reader.ReadUInt64();

            Unk30 = reader.ReadUInt16();
            Unk32 = reader.ReadUInt16();

            Unk34 = reader.ReadUInt32();
            StartIdx = reader.ReadUInt32();
            Unk3c = reader.ReadUInt32();
            Count = reader.ReadUInt32();

            DescriptionSize = reader.ReadUInt32();
            DescriptionAlign = reader.ReadUInt32();

            ShortName = Encoding.ASCII.GetString(reader.ReadBytes(4));
        }

        public static FileEntryInternal[] Parse(BinaryReader reader, uint size)
        {
            var ret = new FileEntryInternal[size];
            for (var i = 0; i < size; i++)
                ret[i] = new FileEntryInternal(reader);

            return ret;
        }
    }
}
