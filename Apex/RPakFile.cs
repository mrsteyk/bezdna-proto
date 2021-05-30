using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bezdna_proto.Apex
{
    class RPakFile
    {
        public RPakHeader Header { get; private set; }
        public BinaryReader reader { get; private set; }

        public string Starpak { get; private set; }

        public Titanfall2.SectionDescriptor[] SectionDescriptors { get; private set; }
        public Titanfall2.DataChunk[] DataChunks { get; private set; }
        public FileEntryInternal[] FilesInternal { get; private set; }

        public RPakFile(FileStream file)
        {
            Header = new RPakHeader(file);

            if (Header.Compressed)
            {
                if (File.Exists(file.Name + ".raw"))
                    reader = new BinaryReader(new FileStream(file.Name + ".raw", FileMode.Open, FileAccess.Read));
                else
                    reader = new BinaryReader(new MemoryStream(Utils.Decompress(file, Header.SizeDecompressed, Utils.HEADER_SIZE8)));
                GC.Collect();
            }
            else
            {
                reader = new BinaryReader(file);
            }

            reader.BaseStream.Seek(Utils.HEADER_SIZE8, SeekOrigin.Begin);

            if (Header.PartRPak != 0)
                throw new Exception("PART RPAK IN APEX????");

            Starpak = reader.ReadNTString();

            var starpakSkipped = Utils.HEADER_SIZE8 + Header.SkipShit;
            // unk4a here

            var unk4aSkipped = starpakSkipped + Header.Unk4A;
            reader.BaseStream.Position = unk4aSkipped;
            SectionDescriptors = Titanfall2.SectionDescriptor.Parse(reader, Header.SectionsNum);

            var sectionsSkipped = unk4aSkipped + (16 * Header.SectionsNum);
            reader.BaseStream.Position = sectionsSkipped;
            DataChunks = Titanfall2.DataChunk.Parse(reader, Header.DataChunksNum);

            var dataChunksSkipped = sectionsSkipped + (12 * Header.DataChunksNum);
            // unk54 here

            var unk54Skipped = dataChunksSkipped + (8 * Header.Unk54);
            reader.BaseStream.Position = unk54Skipped;
            FilesInternal = FileEntryInternal.Parse(reader, Header.NumFiles);

            var fileEntrieSkipped = unk54Skipped + (0x50 * Header.NumFiles);
            // unk5c here

            var unk5cSkipped = fileEntrieSkipped + (8 * Header.Unk5C);
            // unk60 here

            var unk60SKipped = unk5cSkipped + (4 * Header.Unk60);
            // unk64 here

            var unk64SKipped = unk60SKipped + (4 * Header.Unk64);
            // unk68 here

            var unk68SKipped = unk64SKipped + (1 * Header.Unk68);
            // unk6C here

            var unk6CSKipped = unk68SKipped + (16 * Header.Unk6C);
            // unk70 here

            var unk70SKipped = unk6CSKipped + (16 * Header.Unk6C);
            // unk74 here

            if (Header.Unk74 != 0)
                throw new Exception("Unk74 != 0!");

            reader.BaseStream.Position = unk70SKipped;

            ParseParsedData();
        }

        public long[] DataChunkSeeks { get; private set; }
        private void ParseParsedData() // шиз бляь
        {
            var minPos = reader.BaseStream.Position;

            var kekPos = reader.BaseStream.Length;

            var dataChunkSeeks = new long[DataChunks.Length];
            for (var i = DataChunks.Length - 1; i >= 0; i--)
            {
                var data = DataChunks[i];
                kekPos -= (long)data.Size;

                if (data.Size == 0)
                {
                    //MinDataChunkID = i;
                    break;
                }

                dataChunkSeeks[i] = kekPos;

                if (minPos == kekPos)
                {
                    //MinDataChunkID = i - 1;
                    break;
                }
                else if (minPos > kekPos)
                {
                    //MinDataChunkID = i; // I'm retarded
                    break;
                }
            }

            DataChunkSeeks = dataChunkSeeks;
        }
    }
}
