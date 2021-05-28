using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Titanfall2
{
    class SectionDescriptor
    {

        // TODO: figure this shit out...
        public enum ESectionType
        {
            Descriptions = 0,
            Unk1 = 1,
            File = 3,

            Unk32 = 32,
            Unk33 = 33,

            StringTable = 0x81,
        }

        public ESectionType SectionType { get; private set; } // huh?
        public uint AlignByte { get; private set; }
        public ulong SizeUnaligned { get; private set; }
        public SectionDescriptor(BinaryReader reader)
        {
            SectionType = (ESectionType)reader.ReadUInt32();
            AlignByte = reader.ReadUInt32();
            SizeUnaligned = reader.ReadUInt64();
        }

        public static SectionDescriptor[] Parse(BinaryReader reader, ushort size)
        {
            var ret = new SectionDescriptor[size];
            for(var i=0; i<size; i++)
                ret[i] = new SectionDescriptor(reader);

            return ret;
        }
    }
}
