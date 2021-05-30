using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Apex.FileTypes
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
        private static readonly string[] _Compression = {
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
            public bool optional;
        }
        private static readonly ushort[] _textureTypeSteps = { 1032, 1032, 1040, 1040, 1040, 1040, 1032, 1032, 1040, 1040, 1040, 1040, 1040, 1040, 272, 272, 272, 268, 268, 268, 264, 264, 264, 264, 264, 264, 264, 264, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 258, 258, 258, 258, 258, 258, 258, 258, 258, 257, 257, 257, 257, 257, 260, 260, 260, 258, 0, 0 };

        public ulong GUID { get; private set; } // 8
        public DataDescriptor NameDesc { get; private set; } // 8

        public ushort Width { get; private set; } // 2
        public ushort Height { get; private set; } // 2

        public ushort Unk14 { get; private set; } // 2

        public ushort TextureType { get; private set; } // 2

        public uint Unk18 { get; private set; } // 4
        public byte Unk1c { get; private set; } // 1
        public byte StarpakOptionalMipMaps { get; private set; } // 1
        public ushort Unk1e { get; private set; } // 2
        public byte Unk20 { get; private set; } // 1

        public byte MipMaps { get; private set; } // 1
        public byte StarPakMandatoryMipMapsCount { get; private set; } // 1

        // TODO: 0x23-0x38

        public string Name { get; private set; }

        public long StartSeekRPak { get; private set; }
        public ulong RPakSize { get; private set; } // Don't trust this really
        public ulong StartSeekStarpak { get; private set; }
        public ulong StartSeekStarpakOptional { get; private set; }
        public int StarpakNum { get; private set; }

        public int StarpakTotalCount => StarPakMandatoryMipMapsCount + StarpakOptionalMipMaps;
        public int RPakMipMapsCount => MipMaps - StarpakTotalCount;
        //public int StarpakMandatCount => StarPakMipMaps - UnkMipMaps;

        public TextureData[] TextureDatas { get; private set; }
        public string Algorithm { get; private set; }

        public Texture(RPakFile rpak, FileEntryInternal file)
        {
            var description = file.Description;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[description.id] + description.offset, System.IO.SeekOrigin.Begin);

            GUID = rpak.reader.ReadUInt64(); // 0
            DataDescriptor d;
            d.id = rpak.reader.ReadUInt32(); // 8
            d.offset = rpak.reader.ReadUInt32(); // 0xC
            NameDesc = d;

            var backup = rpak.reader.BaseStream.Position;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[d.id] + d.offset, System.IO.SeekOrigin.Begin);
            Name = rpak.reader.ReadNTString();
            rpak.reader.BaseStream.Position = backup;

            Width = rpak.reader.ReadUInt16(); // 0x10
            Height = rpak.reader.ReadUInt16(); // 0x12

            Unk14 = rpak.reader.ReadUInt16(); // 0x14

            TextureType = rpak.reader.ReadUInt16(); // 0x16

            //Unk18 = rpak.reader.ReadUInt64(); // 0x18
            Unk18 = rpak.reader.ReadUInt32(); // 0x18
            Unk1c = rpak.reader.ReadByte(); // 0x1c
            StarpakOptionalMipMaps = rpak.reader.ReadByte(); // 0x1d optional - cnt < 0x1d
            Unk1e = rpak.reader.ReadUInt16(); // 0x1e
            Unk20 = rpak.reader.ReadByte(); // 0x20

            MipMaps = rpak.reader.ReadByte(); // 0x21
            StarPakMandatoryMipMapsCount = rpak.reader.ReadByte(); // 0x22

            if (StarpakOptionalMipMaps != 0 && file.StarpakOffsetOptional == ulong.MaxValue)
                throw new Exception("UnkMipMaps != 0 && file.StarpakOffsetOptional == ulong.MaxValue");

            if (StarpakTotalCount > MipMaps)
                throw new Exception("StarpakTotalCount > MipMaps");

            var data = file.Data;
            //if (data.offset != 0)
            //    throw new Exception("Bruh moment, actual texture data offset wasn't 0!!!");
            StartSeekRPak = rpak.DataChunkSeeks[data.id] + data.offset;
            RPakSize = rpak.DataChunks[data.id].Size - data.offset;

            StarpakNum = (int)file.StarpakOffset & 0xFFF; // Yes, it's bigger in apex for some reason, or is it?..

            StartSeekStarpak = StarPakMandatoryMipMapsCount == 0 ? 0 : file.StarpakOffset;
            StartSeekStarpakOptional = StarpakOptionalMipMaps == 0 ? 0 : file.StarpakOffsetOptional;
            Algorithm = _Compression[TextureType];
            if (Algorithm == "UNKNOWN")
                Console.WriteLine($"!!! {TextureType} ISNT PROGRAMMED IN !!!");

            // --- RETARDED MIPMAP WALKER ---
            var textureDatas = new TextureData[MipMaps];
            var off = StartSeekRPak;
            
            var offStar = (long)StartSeekStarpak;
            offStar -= StarpakNum;
            var offStarOpt = (long)StartSeekStarpakOptional;
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

                textureDatas[i].streaming = i < StarpakTotalCount;
                textureDatas[i].optional = i < StarpakOptionalMipMaps;

                var v19 = lobyte_ * ((v16 + hibyte_ - 1) >> (hibyte_ >> 1)) * ((v17 + hibyte_ - 1) >> (hibyte_ >> 1));
                textureDatas[i].seek = textureDatas[i].optional ? offStarOpt : (textureDatas[i].streaming ? offStar : off);
                textureDatas[i].width = v16;
                textureDatas[i].height = v17;
                textureDatas[i].size = v19;

                if (textureDatas[i].optional)
                    offStarOpt += (v19 + 15) & 0xFFFFFFF0;
                if (textureDatas[i].streaming)
                    offStar += (v19 + 15) & 0xFFFFFFF0;
                else
                    off += (v19 + 15) & 0xFFFFFFF0;
            }

            TextureDatas = textureDatas;
        }
    }
}
