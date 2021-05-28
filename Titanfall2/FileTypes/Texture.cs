using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Titanfall2.FileTypes
{
    class Texture
    {
        /*public enum Compression
        {
            DXT1 = 0,
            DXT1 = 1,
            BC4U = 6,
            BC5U = 8,
        }*/
        // Ебал я этих даунов из РСПН, сука, кто так делает блять ЪуЪ
        private readonly string[] _Compression = {
            "DXT1", // 0
            "DXT1", // 1
            "UNKNOWN", // 2
            "UNKNOWN", // 3
            "UNKNOWN", // 4
            "UNKNOWN", // 5
            "BC4U", // 6
            "UNKNOWN", // 7
            "BC5U", // 8
            "UNKNOWN", // 9
            "BC6H", // 10
            "UNKNOWN", // 11
            "UNKNOWN", // 12
            "BC7U", // 13
            "UNKNOWN", // 14
            "UNKNOWN", // 15
            "UNKNOWN", // 16
            "UNKNOWN", // 17
            "UNKNOWN", // 18
            "UNKNOWN", // 19
            "UNKNOWN", // 20
            "UNKNOWN", // 21
            "UNKNOWN", // 22
            "UNKNOWN", // 23
            "UNKNOWN", // 24
            "UNKNOWN", // 25
            "UNKNOWN", // 26
            "UNKNOWN", // 27
            "UNKNOWN", // 28
            "UNKNOWN", // 29
            "UNKNOWN", // 30
            "UNKNOWN", // 31
            "UNKNOWN", // 32
            "UNKNOWN", // 33
            "UNKNOWN", // 34
            "UNKNOWN", // 35
            "UNKNOWN", // 36
            "UNKNOWN", // 37
            "UNKNOWN", // 38
            "UNKNOWN", // 39
            "UNKNOWN", // 40
            "UNKNOWN", // 41
            "UNKNOWN", // 42
            "UNKNOWN", // 43
            "UNKNOWN", // 44
            "UNKNOWN", // 45
            "UNKNOWN", // 46
            "UNKNOWN", // 47
            "UNKNOWN", // 48
            "UNKNOWN", // 49
            "UNKNOWN", // 50
            "UNKNOWN", // 51
            "UNKNOWN", // 52
            "UNKNOWN", // 53
            "UNKNOWN", // 54
            "UNKNOWN", // 55
            "UNKNOWN", // 56
            "UNKNOWN", // 57
            "UNKNOWN", // 58
            "UNKNOWN", // 59
            "UNKNOWN", // 60
            "UNKNOWN", // 61
            "UNKNOWN", // 62
            "UNKNOWN", // 63
        };

        public struct TextureData
        {
            public long seek;
            public int width;
            public int height;
            public int size;

            public bool streaming;
        }
        private readonly ushort[] _textureTypeSteps = { 1032, 1032, 1040, 1040, 1040, 1040, 1032, 1032, 1040, 1040, 1040, 1040, 1040, 1040, 272, 272, 272, 268, 268, 268, 264, 264, 264, 264, 264, 264, 264, 264, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 258, 258, 258, 258, 258, 258, 258, 258, 258, 257, 257, 257, 257, 257, 260, 260, 260, 258, 0, 0 };

        public ulong GUID { get; private set; }
        public DataDescriptor NameDesc { get; private set; }

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public ushort Unk14 { get; private set; }

        public ushort TextureType { get; private set; }

        public ulong Unk18 { get; private set; }
        public byte Unk20 { get; private set; }

        public byte MipMaps { get; private set; }
        public byte StarPakMipMaps { get; private set; }

        // TODO: 0x23-0x38

        public string Name { get; private set; }

        public long StartSeekRPak { get; private set; }
        public ulong RPakSize { get; private set; } // Don't trust this really
        public ulong StartSeekStarpak { get; private set; }
        public int StarpakNum { get; private set; }

        public int RPakMipMapsCount => MipMaps - StarPakMipMaps;

        public TextureData[] TextureDatas { get; private set; }
        public string Algorithm { get; private set; }

        public Texture(RPakFile rpak, FileEntryInternal file)
        {
            if(rpak.MinDataChunkID > file.Description.id)
            {
                Name = "OOB";
                return;
            }

            var description = file.Description;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[description.id] + description.offset, System.IO.SeekOrigin.Begin);

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

            Width = rpak.reader.ReadUInt16();
            Height = rpak.reader.ReadUInt16();

            Unk14 = rpak.reader.ReadUInt16();

            TextureType = rpak.reader.ReadUInt16();

            Unk18 = rpak.reader.ReadUInt64();
            Unk20 = rpak.reader.ReadByte();

            MipMaps = rpak.reader.ReadByte();
            StarPakMipMaps = rpak.reader.ReadByte();

            var data = file.Data;
            //if (data.offset != 0)
            //    throw new Exception("Bruh moment, actual texture data offset wasn't 0!!!");
            StartSeekRPak = rpak.DataChunkSeeks[data.id] + data.offset;
            RPakSize = rpak.DataChunks[data.id].Size - data.offset;

            StarpakNum = (int)file.StarpakOffset & 0xF;
            //StarpakOffset -= StarpakNum; // Make 'em aligned...

            StartSeekStarpak = StarPakMipMaps == 0 ? 0 : file.StarpakOffset;
            Algorithm = _Compression[TextureType];
            if (Algorithm == "UNKNOWN")
                Console.WriteLine($"!!! {TextureType} ISNT PROGRAMMED IN !!!");
                //throw new NotImplementedException("TextureType is unknown!");

            // --- RETARDED MIPMAP WALKER ---
            var textureDatas = new TextureData[MipMaps];
            var off = StartSeekRPak;
            var offStar = (long)StartSeekStarpak;
            offStar -= StarpakNum;
            var lobyte_ = _textureTypeSteps[TextureType] & 0xFF;
            var hibyte_ = _textureTypeSteps[TextureType] >> 8;

            // ACCURATE ONLY FOR Unk14 = 0
            var v10 = MipMaps;
            for (int i = MipMaps - 1; i >= 0; i--) 
            {
                v10 -= 1;

                var v16 = 1;
                if ((Width >> v10) > 1)
                    v16 = Width >> v10;
                var v17 = 1;
                if ((Height >> v10) > 1)
                    v17 = Height >> v10;

                textureDatas[i].streaming = i < StarPakMipMaps;

                var v19 = lobyte_ * ((v16 + hibyte_ - 1) >> (hibyte_ >> 1)) * ((v17 + hibyte_ - 1) >> (hibyte_ >> 1));
                textureDatas[i].seek = textureDatas[i].streaming ? offStar : off;
                textureDatas[i].width = v16;
                textureDatas[i].height = v17;
                textureDatas[i].size = v19;

                if (textureDatas[i].streaming)
                    offStar += (v19 + 15) & 0xFFFFFFF0;
                else
                    off += (v19 + 15) & 0xFFFFFFF0;
            }

            TextureDatas = textureDatas;
        }
    }
}
