using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Titanfall2
{
    class DataChunk
    {
        /*public enum EDataType
        {
            Descriptions = 0, // All descriptions, describe what that file is exactly...
            StringTable = 1, // All names
            File = 2, // File data itself
            Unk3 = 3, // Looks like [GUID,qword,NTString]
        }*/

        public ulong SectionID { get; private set; }
        public uint Align { get; private set; }
        public ulong Size { get; private set; }

        public DataChunk(BinaryReader reader)
        {
            SectionID = reader.ReadUInt32();
            Align = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }

        public static DataChunk[] Parse(BinaryReader reader, ushort size)
        {
            var ret = new DataChunk[size];
            for (var i = 0; i < size; i++)
                ret[i] = new DataChunk(reader);

            return ret;
        }
    }
}
