using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bezdna_proto.Titanfall2.FileTypes
{
    class Shader
    {
        public enum EShaderType
        {
            Pixel = 0,
            Vertex = 1,
            Geometry = 2,
            Compute = 5, // WTF?
        }

        public struct ShaderElement
        {
            public DataDescriptor data;
            public uint size;
        }

        public DataDescriptor NameDesc { get; private set; }

        public EShaderType ShaderType { get; private set; }
        public uint NumShaders { get; private set; }

        public DataDescriptor Idk1 { get; private set; }
        public DataDescriptor Idk2 { get; private set; }

        // ---

        public string Name { get; private set; }

        public ShaderElement[] ShaderElements { get; private set; }

        public Shader(RPakFile rpak, FileEntryInternal file)
        {
            if (rpak.MinDataChunkID > file.Description.id)
            {
                Name = "OOB";
                return;
            }

            var description = file.Description;
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[description.id] + description.offset, System.IO.SeekOrigin.Begin);

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

            ShaderType = (EShaderType)rpak.reader.ReadUInt32();
            NumShaders = rpak.reader.ReadUInt32();

            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Idk1 = d;
            d.id = rpak.reader.ReadUInt32();
            d.offset = rpak.reader.ReadUInt32();
            Idk2 = d;

            // ---

            var shaderElements = new ShaderElement[NumShaders];
            rpak.reader.BaseStream.Seek(rpak.DataChunkSeeks[file.Data.id] + file.Data.offset, System.IO.SeekOrigin.Begin);
            for (var i=0; i<NumShaders; i++)
            {
                shaderElements[i].data.id = rpak.reader.ReadUInt32();
                shaderElements[i].data.offset = rpak.reader.ReadUInt32();
                shaderElements[i].size = rpak.reader.ReadUInt32();

                if (ShaderType == EShaderType.Vertex)
                    rpak.reader.BaseStream.Seek(24 - 12, System.IO.SeekOrigin.Current);
                else
                    rpak.reader.BaseStream.Seek(16 - 12, System.IO.SeekOrigin.Current);
            }
            ShaderElements = shaderElements;
        }
    }
}
