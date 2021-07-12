using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Apex.FileTypes
{
    class RUI
    {

        // 0x0
        public DataDescriptor NameDesc { get; private set; }
        public DataDescriptor Unk1 { get; private set; }
        public DataDescriptor Unk2 { get; private set; }

        //0x30
        public DataDescriptor ArgClusterStart { get; private set; }
        public DataDescriptor ArgStart { get; private set; }

        // 0x4e
        public ushort ArgClusterCnt { get; private set; }

        public string Name { get; private set; }

        public struct ArgCluster
        {
            public ushort num_start; // 0x0, from which number to start
            public ushort num_arg; // 0x2, how many args
            public byte hash_mul; // 0x4, to which num mul
            public byte hash_add; // 0x5, which num to add
            // everything else is unk
        }

        public ArgCluster[] ArgClusters { get; private set; }

        public enum ArgTypes : byte
        {
            INVALID = 0,
            STRING = 12,
            STRING_OLD = 1,
            ASSET = 2,
            BOOLEAN,
            INT,
            FLOAT,
            FLOAT2,
            FLOAT3,
            FLOAT4,
            GAMETIME,
            WALLTIME,
            UIHANDLE,
            IMAGE,
            FONTFACE,
            FONTHASH,
            ARRAY,
        }

        public struct Arg
        {
            public ArgTypes type;
            public bool ro; // can be edited?
            public ushort off; // ???
            public ushort unk; // ???
            public ushort hash16_shr4;
        }

        public Arg[] Args { get; private set; }

        private uint hash(string str, byte v9_4, byte v9_5)
        {
            uint hash = 0;
            foreach(var v10 in str)
            {
                var v12 = hash >> 20;
                var v13 = v9_5 + hash * v9_4 + v10;
                hash = v12 ^ v13;
            }

            return hash;
        }

        public RUI(RPakFile rpak, FileEntryInternal file)
        {
            var description = file.Description;
            var descOff = rpak.DataChunkSeeks[description.id] + description.offset;
            rpak.reader.BaseStream.Seek(descOff, System.IO.SeekOrigin.Begin);

            DataDescriptor d;
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            NameDesc = d;

            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Unk1 = d;

            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Unk2 = d;

            // skip to 0x30
            rpak.reader.BaseStream.Seek(descOff + 0x30, System.IO.SeekOrigin.Begin);
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            ArgClusterStart = d;

            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            ArgStart = d;

            // skip to 0x4e
            rpak.reader.BaseStream.Seek(descOff + 0x4e, System.IO.SeekOrigin.Begin);
            ArgClusterCnt = rpak.reader.ReadUInt16();

            // Beautify
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[NameDesc.id] + NameDesc.offset, System.IO.SeekOrigin.Begin);
            Name = rpak.reader.ReadNTString();

            var clusterOff = rpak.DataChunkSeeks[ArgClusterStart.id] + ArgClusterStart.offset;
            //rpak.reader.BaseStream.Seek(clusterOff, System.IO.SeekOrigin.Begin);
            var clusters = new List<ArgCluster>();
            var argsNum = 0;
            for(var i=0; i < ArgClusterCnt; i++)
            {
                rpak.reader.BaseStream.Seek(clusterOff + 18*i, System.IO.SeekOrigin.Begin);
                // cluster? :clownflushed:
                ArgCluster cluster;
                cluster.num_start = rpak.reader.ReadUInt16();
                cluster.num_arg = rpak.reader.ReadUInt16();
                cluster.hash_mul = rpak.reader.ReadByte();
                cluster.hash_add = rpak.reader.ReadByte();
                // we read 2+2+1+1 = 6 bytes, skip 12
                //rpak.reader.ReadUInt64();
                //rpak.reader.ReadUInt32();

                argsNum += cluster.num_arg;

                clusters.Add(cluster);
            }
            ArgClusters = clusters.ToArray();

            var argOff = rpak.DataChunkSeeks[ArgStart.id] + ArgStart.offset;
            rpak.reader.BaseStream.Seek(argOff, System.IO.SeekOrigin.Begin);
            var args = new List<Arg>();
            for(var i = 0; i < argsNum; i++)
            {
                Arg arg;
                arg.type = (ArgTypes)rpak.reader.ReadByte();
                arg.ro = rpak.reader.ReadByte() != 0;
                arg.off = rpak.reader.ReadUInt16();
                arg.unk = rpak.reader.ReadUInt16();
                arg.hash16_shr4 = rpak.reader.ReadUInt16();

                args.Add(arg);
            }
            Args = args.ToArray();
        }
    }
}
