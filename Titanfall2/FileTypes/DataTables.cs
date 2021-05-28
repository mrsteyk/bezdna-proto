using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Titanfall2.FileTypes
{
    class DataTables
    {
        public enum EColumnType
        {
            Bool = 0,
            Int = 1,
            Float = 2,
            Vector = 3,
            String = 4,
            Asset = 5,
            AssetNoprecache = 6,
        }
        public struct Column
        {
            public string name;
            public EColumnType type;
            public uint offset;
        }
        public uint ColumnNum { get; private set; }
        public uint RowNum { get; private set; }
        public uint ElemSize { get; private set; }

        public DataDescriptor Columns { get; private set; }
        public DataDescriptor Rows { get; private set; }

        public Column[] ColumnPretty { get; private set; }
        //public long RowSeeks { get; private set; }

        public string[] Pretty { get; private set; }

        public DataTables(RPakFile rpak, FileEntryInternal file)
        {
            if (rpak.MinDataChunkID > file.Description.id)
            {
                return;
            }

            var description = file.Description;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[description.id] + description.offset, System.IO.SeekOrigin.Begin);

            ColumnNum = rpak.reader.ReadUInt32();
            RowNum = rpak.reader.ReadUInt32();

            DataDescriptor d;
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Columns = d;

            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Rows = d;

            ElemSize = rpak.reader.ReadUInt32();

            Pretty = new string[RowNum + 1];
            Pretty[0] = "";

            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[Columns.id] + Columns.offset, System.IO.SeekOrigin.Begin);
            ColumnPretty = new Column[ColumnNum];
            for (var i = 0; i < ColumnNum; i++) 
            {
                d.id = rpak.reader.ReadUInt32();
                d.offset = rpak.reader.ReadUInt32();
                
                var backup = rpak.reader.BaseStream.Position;
                rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[d.id] + d.offset, System.IO.SeekOrigin.Begin);
                ColumnPretty[i].name = rpak.reader.ReadNTString();
                rpak.reader.BaseStream.Position = backup;

                ColumnPretty[i].type = (EColumnType)rpak.reader.ReadUInt32();
                ColumnPretty[i].offset = rpak.reader.ReadUInt32();

                Pretty[0] += ColumnPretty[i].name;
                Pretty[0] += "\t";
            }


            for (var i = 1; i <= RowNum; i++)
            {
                Pretty[i] = "";
                var basePos = rpak.DataChunkSeeks[Rows.id] + Rows.offset + ElemSize * (i-1);
                for (var j = 0; j < ColumnNum; j++)
                {
                    rpak.reader.BaseStream.Position = basePos + ColumnPretty[j].offset;
                    switch (ColumnPretty[j].type)
                    {
                        case EColumnType.Bool:
                            Pretty[i] += $"{rpak.reader.ReadUInt32() != 0}\t";
                            break;
                        case EColumnType.Int:
                            Pretty[i] += $"{rpak.reader.ReadInt32()}\t";
                            break;
                        case EColumnType.Float:
                            Pretty[i] += $"{rpak.reader.ReadSingle()}\t";
                            break;
                        case EColumnType.Vector:
                            throw new NotImplementedException("EColumnType.Vector");
                            break;
                        case EColumnType.String:
                        case EColumnType.Asset:
                        case EColumnType.AssetNoprecache:
                            DataDescriptor str;
                            str.id = rpak.reader.ReadUInt32();
                            str.offset = rpak.reader.ReadUInt32();
                            rpak.reader.BaseStream.Position = rpak.DataChunkSeeks[str.id] + str.offset;
                            if (ColumnPretty[j].type >= EColumnType.Asset)
                                Pretty[i] += "$";
                            Pretty[i] += rpak.reader.ReadNTString();
                            Pretty[i] += "\t";
                            break;
                    }
                }
            }
        }
    }
}
