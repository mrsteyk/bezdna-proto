using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Apex.FileTypes
{
    class Material
    {
        // pad 0-8
        public ulong GUID { get; private set; }
        public DataDescriptor NameDesc { get; private set; }

        // TODO ???-0xf0

        public string Name { get; private set; }
        public string MaterialName { get; private set; }

        public ulong[] TextureReferences { get; private set; }

        // Is this even valid?
        public static readonly string[] TextureRefName =
        {
            "_col",
            "_nml",
            "_gls",
            "_spc",
            "_ao", // ???
            "_cav", // Figure out wtf is cav and wtf is ao
        };

        public Material(RPakFile rpak, FileEntryInternal file)
        {
            var description = file.Description;
            var descOff = rpak.DataChunkSeeks[description.id] + description.offset;
            rpak.reader.BaseStream.Seek(descOff, System.IO.SeekOrigin.Begin);

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

            var backup = rpak.reader.BaseStream.Position;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[d.id] + d.offset, System.IO.SeekOrigin.Begin);
            Name = rpak.reader.ReadNTString();
            rpak.reader.BaseStream.Position = backup;

            // TODO: figure out wtf is everything else...
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            backup = rpak.reader.BaseStream.Position;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[d.id] + d.offset, System.IO.SeekOrigin.Begin);
            MaterialName = rpak.reader.ReadNTString();
            rpak.reader.BaseStream.Position = backup;

            rpak.reader.BaseStream.Position = descOff + 0x60; // 0x98 in TF|2???
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            var wtfOff = rpak.DataChunkSeeks[d.id] + d.offset;
            rpak.reader.BaseStream.Position = wtfOff;

            var textureRefs = new List<ulong>();
            var texture_guid = rpak.reader.ReadUInt64();
            do
            {
                textureRefs.Add(texture_guid);
                texture_guid = rpak.reader.ReadUInt64();
            } while (texture_guid != 0);

            TextureReferences = textureRefs.ToArray();

            // 0x58 - shds
        }
    }
}
