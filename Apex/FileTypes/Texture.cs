using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Apex.FileTypes
{
    class Texture
    {
        public struct TextureData
        {
            public long seek;
            public int width;
            public int height;
            public int size;

            public bool streaming;
            public bool optional;
        }
        //private static readonly ushort[] _textureTypeSteps = { 1032, 1032, 1040, 1040, 1040, 1040, 1032, 1032, 1040, 1040, 1040, 1040, 1040, 1040, 272, 272, 272, 268, 268, 268, 264, 264, 264, 264, 264, 264, 264, 264, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 260, 258, 258, 258, 258, 258, 258, 258, 258, 258, 257, 257, 257, 257, 257, 260, 260, 260, 258, 0, 0 };
        private static readonly byte[] _textureTypeShit = { 8, 4, 4, 8, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 8, 4, 4, 8, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 16, 4, 4, 16, 1, 1, 16, 1, 1, 16, 1, 1, 12, 1, 1, 12, 1, 1, 12, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 8, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 4, 1, 1, 4, 1, 1, 4, 1, 1, 2, 1, 1, 16, 4, 4, 16, 5, 4 };

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

            Name = ""; // generally a safe assumption, never seen name desc not 0ui64...

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

            //if ((Unk1e & 0xFF) > 1)
            //    throw new Exception("byte Unk1e > 1");

            var data = file.Data;
            //if (data.offset != 0)
            //    throw new Exception("Bruh moment, actual texture data offset wasn't 0!!!");
            StartSeekRPak = rpak.DataChunkSeeks[data.id] + data.offset;
            RPakSize = rpak.DataChunks[data.id].Size - data.offset;

            StarpakNum = (int)file.StarpakOffset & 0xFFF; // Yes, it's bigger in apex for some reason, or is it?..

            StartSeekStarpak = StarPakMandatoryMipMapsCount == 0 ? 0 : file.StarpakOffset;
            StartSeekStarpakOptional = StarpakOptionalMipMaps == 0 ? 0 : file.StarpakOffsetOptional;
            Algorithm = Titanfall2.FileTypes.Texture.Compression[TextureType];
            if (Algorithm == "UNKNOWN")
                Console.WriteLine($"!!! {TextureType} ISNT PROGRAMMED IN !!!");

            // Retard?
            MipMaps += (byte)StarpakTotalCount;
            //MipMaps += 1;
            long size = 0;

            // --- RETARDED MIPMAP WALKER ---
            var textureDatas = new TextureData[MipMaps];
            var off = StartSeekRPak;
            
            var offStar = (long)StartSeekStarpak;
            offStar -= StarpakNum;
            var offStarOpt = (long)StartSeekStarpakOptional;

            var unk1e = Unk1e & 0xFF;
            if (unk1e == 0)
                unk1e = 1;
            //if (unk1e != 0)
            //    throw new Exception("What?");

            //var v10 = MipMaps;
            var v20 = TextureType * 3;
            for (int i = MipMaps - 1; i >= 0; i--) 
            {
                //v10 -= 1;
                var v10 = i;

                textureDatas[i].streaming = i < StarpakTotalCount;
                textureDatas[i].optional = i < StarpakOptionalMipMaps;

                var v15 = _textureTypeShit[v20];
                var v14 = _textureTypeShit[v20 + 1];
                var v16 = _textureTypeShit[v20 + 2];

                var v17 = 1;
                if ((Width >> v10) > 1)
                    v17 = Width >> v10;
                var v22 = 1;
                if ((Height >> v10) > 1)
                    v22 = Height >> v10;

                var v21 = (v14 + v17 - 1) / v14;
                var v23 = v21 * ((v16 + v22 - 1) / v16);
                var v25 = v15 * v23;

                textureDatas[i].seek = textureDatas[i].optional ? offStarOpt : (textureDatas[i].streaming ? offStar : off);
                textureDatas[i].width = v17;
                textureDatas[i].height = v22;
                textureDatas[i].size = v25;

                if (textureDatas[i].optional)
                    offStarOpt += (v25 + 15) & 0xFFFFFFF0;
                else if (textureDatas[i].streaming)
                    offStar += (v25 + 15) & 0xFFFFFFF0;
                else
                    off += (v25 + 15) & 0xFFFFFFF0;

                //for(var j = 0; j<unk1e; j++)
                if(i < StarpakOptionalMipMaps)
                    size += unk1e * v25;
                else
                    size += unk1e * ((v25 + 15) & 0xFFFFFFF0);
            }

            if (size != Unk18 && MipMaps != 1) // Textures with mipmaps of 1 usually don't match cuz 8 @ the end or something stupid like that...
                throw new Exception("size != Unk18");

            TextureDatas = textureDatas;
        }
    }
}
