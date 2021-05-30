using System;
using System.IO;

namespace bezdna_proto
{
    class Program
    {
        static void Main(string[] args)
        {
            //var f = new Titanfall2.RPakFile(new FileStream(@"D:\OriginGays\Titanfall2\r2\paks\Win64\common(01).rpak", FileMode.Open, FileAccess.Read));
            var fstream = new FileStream(args[0], FileMode.Open, FileAccess.Read);

            var header = new byte[4];
            fstream.Read(header, 0, 4);
            if(!Utils.ValidRPakHeader(header))
            {
                Console.WriteLine($"File ${args[0]} isn't valid RPak file!");
            }


            var version = Utils.GetRPakVersion(fstream);
            fstream.Position = 0;
            if (version == 7)
            {
                r2(fstream);
            } else if(version == 8)
            {
                if (!File.Exists(args[0] + ".raw"))
                {
                    var Header = new Apex.RPakHeader(fstream);
                    if (Header.Compressed)
                    {
                        var k0k = Utils.Decompress(fstream, Header.SizeDecompressed, Utils.HEADER_SIZE8);
                        File.WriteAllBytes(args[0] + ".raw", k0k);
                    }
                }
                apex(fstream);
            } else
            {
                Console.WriteLine($"Version {version} isn't supported!");
            }
        }

        static void apex(FileStream fstream)
        {
            var f = new Apex.RPakFile(fstream);

            Console.WriteLine($"{f.Header.PartRPak}");

            Console.WriteLine($"Starpak?: {f.Starpak}");

            Console.WriteLine("\nSections:");
            for (var i = 0; i < f.SectionDescriptors.Length; i++)
            {
                var section = f.SectionDescriptors[i];
                Console.WriteLine($"{i}: {section.SectionType}({(Titanfall2.SectionDescriptor.ESectionType)((int)section.SectionType & 3)}) {section.SizeUnaligned.ToString("X").PadLeft(8, '0')} 0b{Convert.ToString(section.AlignByte, 2)}");
            }

            Console.WriteLine("\nData Chunks:");
            for (var i = 0; i < f.DataChunks.Length; i++)
            {
                var chunk = f.DataChunks[i];
                Console.WriteLine($"{i}: @{f.DataChunkSeeks[i].ToString("X").PadLeft(16, '0')} {f.SectionDescriptors[chunk.SectionID].SectionType} {chunk.Size.ToString("X")}");
            }

            Console.WriteLine("\nFile list:");
            foreach (var file in f.FilesInternal)
            {
                //var name = f.StringTable.Strings[file.Unk28];
                var ext = file.ShortName;
                var guid = file.GUID;

                if (ext == "txtr")
                {
                    var texture = new Apex.FileTypes.Texture(f, file);
                    Console.WriteLine($"{texture.Name}.{guid.ToString("X").PadLeft(16, '0')}.txtr {texture.Width}x{texture.Height}");
                    if (texture.GUID != file.GUID)
                        Console.WriteLine($"\t{file.GUID} != {texture.GUID}");

                    Console.WriteLine($"\t Type: {texture.TextureType} | Total size: {texture.Unk18.ToString("X")} | MipMaps: {texture.MipMaps} | StarPakMipMaps: {texture.StarpakTotalCount} | StarPakMandat: {texture.StarPakMandatoryMipMapsCount} | StarPakOpt: {texture.StarpakOptionalMipMaps} | RPakMipMapsCnt: {texture.RPakMipMapsCount} | Compression '{texture.Algorithm}' | StarpakNum {texture.StarpakNum}");
                    if (texture.TextureDatas != null)
                        foreach (var e in texture.TextureDatas)
                        {
                            Console.WriteLine($"\t\t{e.seek.ToString("X").PadLeft(16, '0')} {e.width}x{e.height} - {e.size.ToString("X")} | {e.streaming} | {e.optional}");
                        }
                } else
                {
                    Console.WriteLine($"0x{guid.ToString("X").PadLeft(16, '0')}.{ext} {file.NamePad.ToString("X")}");

                    var descOffset = f.DataChunkSeeks[file.Description.id] + file.Description.offset;
                    Console.WriteLine($"\tDesc@{descOffset.ToString("X").PadLeft(16, '0')} size 0x{file.DescriptionSize.ToString("X")}");

                    if (file.Data.id != uint.MaxValue)
                    {
                        var dataOffset = f.DataChunkSeeks[file.Data.id] + file.Data.offset;
                        Console.WriteLine($"\tData@{dataOffset.ToString("X").PadLeft(16, '0')} | 0x{file.StarpakOffset.ToString("X")} | 0x{file.StarpakOffsetOptional.ToString("X")}");
                    }
                    else
                    {
                        Console.WriteLine($"\tNOT IN RPAK | 0x{file.StarpakOffset.ToString("X")} | 0x{file.StarpakOffsetOptional.ToString("X")}");
                        if (file.StarpakOffset == ulong.MaxValue)
                        {
                            Console.Write($"\t\tNOT IN STARPAK EITHER!!!\n");
                            if(file.StarpakOffsetOptional == ulong.MaxValue)
                            {
                                Console.Write($"\t\tNOT IN STARPAK2? EITHER!!!\n");
                            }
                            //Console.Write($"{file.Unk28.ToString("X")} ");
                            //Console.Write($"{file.Unk2a.ToString("X")} ");
                            //Console.Write($"{file.Unk2c.ToString("X")} ");
                            //Console.Write($"{file.StartIdx.ToString("X")} ");
                            //Console.Write($"{file.Unk34.ToString("X")} ");
                            //Console.WriteLine($"{file.Count.ToString("X")}");
                        }
                    }
                }
            }
        }

        static void r2(FileStream fstream)
        {
            var f = new Titanfall2.RPakFile(fstream);

            Console.WriteLine($"{f.Header.PartRPak}");

            Console.WriteLine("StarPaks???:");
            for (var i = 0; i < f.StarPaks.Length; i++)
            {
                Console.WriteLine($"{i}: {f.StarPaks[i]}");
            }

            Console.WriteLine("\nSections:");
            for (var i = 0; i < f.SectionDescriptors.Length; i++)
            {
                var section = f.SectionDescriptors[i];
                Console.WriteLine($"{i}: {section.SectionType}({(Titanfall2.SectionDescriptor.ESectionType)((int)section.SectionType & 3)}) {section.SizeUnaligned.ToString("X").PadLeft(8, '0')} 0b{Convert.ToString(section.AlignByte, 2)}");
            }

            /*Console.WriteLine("\nU48:");
            for (var i = 0; i < f.Unk48.Length; i++)
            {
                var u = f.Unk48[i];
                Console.WriteLine($"{i}: {u.d0.ToString("X")} {u.d1.ToString("X")}");
            }*/

            Console.WriteLine("\nData Chunks:");
            for (var i = 0; i < f.DataChunks.Length; i++)
            {
                var chunk = f.DataChunks[i];
                Console.WriteLine($"{i}: @{f.DataChunkSeeks[i].ToString("X").PadLeft(16, '0')} {f.SectionDescriptors[chunk.SectionID].SectionType} {chunk.Size.ToString("X")}");
            }

            Console.WriteLine("\nFile list:");
            foreach (var file in f.FilesInternal)
            {
                //var name = f.StringTable.Strings[file.Unk28];
                var ext = file.ShortName;
                var guid = file.GUID;
                if (ext == "txtr")
                {
                    var texture = new Titanfall2.FileTypes.Texture(f, file);
                    Console.WriteLine($"{texture.Name}.{guid.ToString("X").PadLeft(16, '0')}.txtr {texture.Width}x{texture.Height}");
                    if (texture.GUID != file.GUID)
                        Console.WriteLine($"\t{file.GUID} != {texture.GUID}");

                    Console.WriteLine($"\t Type: {texture.TextureType} | MipMaps: {texture.MipMaps} | StarPakMipMaps: {texture.StarPakMipMaps} | RPakMipMapsCnt: {texture.RPakMipMapsCount} | Compression '{texture.Algorithm}' | StarpakNum {texture.StarpakNum}");
                    if (texture.TextureDatas != null)
                        foreach (var e in texture.TextureDatas)
                        {
                            Console.WriteLine($"\t\t{e.seek.ToString("X").PadLeft(16, '0')} {e.width}x{e.height} - {e.size.ToString("X")} | {e.streaming}");
                        }
                }
                else if (ext == "matl")
                {
                    var material = new Titanfall2.FileTypes.Material(f, file);
                    Console.WriteLine($"{material.Name}.{guid.ToString("X").PadLeft(16, '0')}.matl");
                    if (material.GUID != file.GUID)
                        Console.WriteLine($"\t{file.GUID} != {material.GUID}");
                }
                else if (ext == "shdr")
                {
                    var shader = new Titanfall2.FileTypes.Shader(f, file);
                    Console.WriteLine($"{shader.Name}.{guid.ToString("X").PadLeft(16, '0')}.shdr");
                    Console.WriteLine($"\t{shader.ShaderType} | {shader.NumShaders}");
                    if (shader.ShaderElements != null)
                        foreach (var e in shader.ShaderElements)
                        {
                            var offset = f.DataChunkSeeks[e.data.id] + e.data.offset;
                            Console.WriteLine($"\t\tData@{offset.ToString("X").PadLeft(16, '0')} | {e.data.id} {e.data.offset.ToString("X")} size {e.size.ToString("X")}");
                        }
                }
                else if (ext == "dtbl")
                {
                    var datatable = new Titanfall2.FileTypes.DataTables(f, file);
                    Console.WriteLine($"{file.GUID.ToString("X").PadLeft(16, '0')}.dtbl {datatable.ColumnNum}x{datatable.RowNum}");
                    if (datatable.Pretty != null)
                        foreach (var e in datatable.Pretty)
                        {
                            Console.WriteLine($"\t{e}");
                        }
                }
                else
                {
                    Console.WriteLine($"0x{guid.ToString("X").PadLeft(16, '0')}.{ext} {file.NamePad.ToString("X")}");

                    var descOffset = f.DataChunkSeeks[file.Description.id] + file.Description.offset;
                    Console.WriteLine($"\tDesc@{descOffset.ToString("X").PadLeft(16, '0')} size 0x{file.DescriptionSize.ToString("X")}");

                    if (file.Data.id != uint.MaxValue)
                    {
                        var dataOffset = f.DataChunkSeeks[file.Data.id] + file.Data.offset;
                        Console.WriteLine($"\tData@{dataOffset.ToString("X").PadLeft(16, '0')}");
                    }
                    else
                    {
                        Console.WriteLine($"\t NOT IN RPAK | 0x{file.StarpakOffset.ToString("X")}");
                        if (file.StarpakOffset == ulong.MaxValue)
                        {
                            Console.Write($"\t\tNOT IN STARPAK EITHER!!! ");
                            Console.Write($"{file.Unk28.ToString("X")} ");
                            Console.Write($"{file.Unk2a.ToString("X")} ");
                            Console.Write($"{file.Unk2c.ToString("X")} ");
                            Console.Write($"{file.StartIdx.ToString("X")} ");
                            Console.Write($"{file.Unk34.ToString("X")} ");
                            Console.WriteLine($"{file.Count.ToString("X")}");
                        }
                    }
                }
            }
        }
    }
}
