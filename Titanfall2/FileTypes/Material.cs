using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Titanfall2.FileTypes
{
    class Material
    {
        // pad 0-8
        public ulong GUID { get; private set; }
        public DataDescriptor NameDesc { get; private set; }
        public uint NameOffset { get; private set; }

        // TODO ???-0xd0

        public string Name { get; private set; }

        public Material(RPakFile rpak, FileEntryInternal file)
        {
            if (rpak.MinDataChunkID > file.Description.id)
            {
                Name = "OOB";
                return;
            }

            var description = file.Description;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[description.id] + description.offset, System.IO.SeekOrigin.Begin);

            // WTF?
            var pad0 = rpak.reader.ReadUInt64();
            var pad8 = rpak.reader.ReadUInt64();

            if (pad0 != 0)
                throw new Exception("pad0 wasn't 0!!!");
            if (pad8 != 0)
                throw new Exception("pad8 wasn't 0!!!");

            GUID = rpak.reader.ReadUInt64();
            DataDescriptor d;
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            NameDesc = d;

            if (rpak.MinDataChunkID > d.id)
            {
                Name = "OOB2";
                return;
            }

            //foreach (var st in rpak.StringTables)
            //    if (st.ID == d.id)
            //        Name = st.GetString(d.offset);
            var backup = rpak.reader.BaseStream.Position;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[d.id] + d.offset, System.IO.SeekOrigin.Begin);
            Name = rpak.reader.ReadNTString();
            rpak.reader.BaseStream.Position = backup;

            // TODO
        }
    }
}
