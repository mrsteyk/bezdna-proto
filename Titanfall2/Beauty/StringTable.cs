using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Titanfall2.Beauty
{
    class StringTable
    {
        public int ID { get; private set; }
        public string[] Strings { get; private set; }
        public uint[] Offsets { get; private set; }

        // TODO: make this use ReadOnlySpan<byte>
        public StringTable(byte[] data, int id)
        {
            ID = id;

            var reader = new BinaryReader(new MemoryStream(data));
            var strs = new List<string>();
            var offs = new List<uint>();

            while (reader.BaseStream.Length > reader.BaseStream.Position)
            {
                offs.Add((uint)reader.BaseStream.Position);
                strs.Add(reader.ReadNTString());
            }

            Offsets = offs.ToArray();
            Strings = strs.ToArray();
        }

        public string GetString(uint offset)
        {
            for (var i = 0; i < Strings.Length; i++) 
            {
                if (Offsets[i] == offset)
                    return Strings[i];
            }

            return "";
        }
    }
}
