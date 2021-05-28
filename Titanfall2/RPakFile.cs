using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Titanfall2
{
    class RPakFile
    {
        public struct U48
        {
            public uint d0;
            public uint d1;
        }

        //private StreamReader file;
        public RPakHeader Header { get; private set; }

        public BinaryReader reader { get; private set; }

        public string[] StarPaks { get; private set; }
        //public int SectionOffset { get; private set; }
        public SectionDescriptor[] SectionDescriptors { get; private set; }
        public DataChunk[] DataChunks { get; private set; }
        public FileEntryInternal[] FilesInternal { get; private set; }

        public U48[] Unk48;

        public long MinDataChunkID { get; private set; }

        public RPakFile(FileStream file)
        {
            Header = new RPakHeader(file);

            /*if(header.PartRPak != 0)
            {
                throw new NotImplementedException("Fucking partial RPaks");
            }*/

            if(Header.Compressed)
            {
                reader = new BinaryReader(new MemoryStream(Utils.Decompress(file, Header.SizeDecompressed, Utils.HEADER_SIZE7)));
            } else
            {
                reader = new BinaryReader(file);
            }

            reader.BaseStream.Seek(Utils.HEADER_SIZE7, SeekOrigin.Begin);

            // Now let's parse the underlying structs?

            // STARPAKS
            StarPaks = new string[2];
            var wtf = Utils.HEADER_SIZE7;
            var wtfInt = reader.ReadUInt32();
            //SectionOffset = 0;
            var wtfSkipped = wtf;
            if (Header.PartRPak != 0)
            {
                //SectionOffset = reader.ReadInt32(); // ???
                wtfSkipped += 8;
            }
            //StarPaks[0] = reader.ReadNTString(); // NOT A STARPAK!!!
            var starpak1 = wtfSkipped + (16 * Header.PartRPak);
            reader.BaseStream.Seek(starpak1, SeekOrigin.Begin);
            StarPaks[0] = reader.ReadNTString();
            var starpak2 = starpak1 + (2 * Header.PartRPak);
            reader.BaseStream.Seek(starpak2, SeekOrigin.Begin);
            StarPaks[1] = reader.ReadNTString();

            var sectionDescStart = starpak2 + Header.SkipShit;
            reader.BaseStream.Seek(sectionDescStart, SeekOrigin.Begin);
            SectionDescriptors = SectionDescriptor.Parse(reader, Header.SectionsNum);

            var sectionDescSkipped = sectionDescStart + (16 * Header.SectionsNum);
            if (sectionDescSkipped != reader.BaseStream.Position)
                throw new Exception("sectionDescSkipped missmatch!");
            DataChunks = DataChunk.Parse(reader, Header.DataChunksNum);

            var dataChunksSkipped = sectionDescSkipped + (12 * Header.DataChunksNum);
            if (dataChunksSkipped != reader.BaseStream.Position)
                throw new Exception("dataChunksSkipped missmatch!");
            // TODO: unk40
            // parse unk40 here...

            var unk40Skipped = dataChunksSkipped + (8 * Header.Unk40);
            reader.BaseStream.Seek(unk40Skipped, SeekOrigin.Begin);
            FilesInternal = FileEntryInternal.Parse(reader, Header.NumFiles);

            var fileEntriesSkipped = unk40Skipped + (72 * Header.NumFiles);
            if (fileEntriesSkipped != reader.BaseStream.Position)
                throw new Exception("fileEntriesSkipped missmatch!");

            // TODO: the rest (unk48, unk4c aka relations, unk50, unk54)

            // TODO: unk48
            Unk48 = new U48[Header.Unk48];
            for(var i=0; i<Header.Unk48; i++)
            {
                Unk48[i].d0 = reader.ReadUInt32();
                Unk48[i].d1 = reader.ReadUInt32();
            }

            var unk48Skipped = fileEntriesSkipped + (8 * Header.Unk48);
            // TODO: unk4c
            // parse unk4c here...

            var unk4cSkipped = unk48Skipped + (4 * Header.Unk4c);
            // TODO: unk50
            // parse unk50 here...

            var unk50SKipped = unk4cSkipped + (4 * Header.Unk50);
            // TODO: unk54
            // parse unk54 here...

            var unk54SKipped = unk50SKipped + (1 * Header.Unk54);

            if (Header.PartRPak != 0)
                unk54SKipped += wtfInt;

            reader.BaseStream.Seek(unk54SKipped, SeekOrigin.Begin);

            ParseParsedData();
        }

        //public Beauty.StringTable[] StringTables { get; private set; }
        public long[] DataChunkSeeks { get; private set; }
        private void ParseParsedData() // шиз бляь
        {
            //var stringTables = new List<Beauty.StringTable>();

            var minPos = reader.BaseStream.Position;

            var kekPos = reader.BaseStream.Length;

            var dataChunkSeeks = new long[DataChunks.Length];
            for(var i = DataChunks.Length-1; i>=0; i--)
            {
                var data = DataChunks[i];
                kekPos -= (long)data.Size;

                if(data.Size == 0)
                {
                    MinDataChunkID = i;
                    break;
                }

                dataChunkSeeks[i] = kekPos;

                if (minPos >= kekPos)
                {
                    MinDataChunkID = i - 1;
                    break;
                }

                /*if(data.DataType == DataChunk.EDataType.StringTable)
                {
                    if (sawStringTable)
                        throw new Exception("More than 1 string table, wtf?");
                    sawStringTable = true;

                    StringTable = new Beauty.StringTable(reader.ReadBytes((int)data.Size));
                } else*/

                /*if (SectionDescriptors[data.SectionID].SectionType == SectionDescriptor.ESectionType.StringTable)
                {
                    stringTables.Add(new Beauty.StringTable(reader.ReadBytes((int)data.Size), i));
                } else*/
                //reader.BaseStream.Seek((int)data.Size, SeekOrigin.Current); // skip data.Size bytes
            }

            DataChunkSeeks = dataChunkSeeks;

            //StringTables = stringTables.ToArray();
        }
    }
}
