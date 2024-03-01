using Prowl.Runtime.GraphicsBackend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Prowl.Runtime
{
    public sealed class LayoutConstructor
    {
        public List<InputLayoutDescription> Inputs { get; private set; } = new();

        public void Add(uint slot, Format format, InputType inputType)
        {
            int size = 0;
            foreach (var input in Inputs)
                size += GraphicUtils.BitsPerPixel(input.Format) / 8;
            Inputs.Add(new InputLayoutDescription(format, (uint)size, slot, inputType));
        }
    }

    public sealed class Mesh : EngineObject, ISerializable
    {

        public InputLayout format;

        public int vertexCount => vertices.Length;
        public int triangleCount => triangles.Length / 3;

        // Vertex attributes data
        public Vertex[] vertices;
        public ushort[] triangles;

        // The array of bone paths under a root
        // The index of a path is the Bone Index
        public string[] boneNames;
        public (Vector3, Quaternion, Vector3)[] boneOffsets;

        public Mesh()
        {
            SetDefaultFormat();
        }

        public GraphicsBuffer? VertexBuffer { get; private set; }
        public GraphicsBuffer? IndexBuffer { get; private set; }
        public uint vao { get; private set; }
        public uint vbo { get; private set; }
        private int uploadedVBOSize = 0;
        public uint ibo { get; private set; }
        private int uploadedIBOSize = 0;

        public unsafe void Upload()
        {
            if (VertexBuffer != null) return; // Already loaded in, You have to Unload first!
            ArgumentNullException.ThrowIfNull(format);

            ArgumentNullException.ThrowIfNull(vertices);
            if (vertices.Length == 0) throw new($"The mesh argument '{nameof(vertices)}' is empty!");

            // Create our vertex and index buffers using the respective arrays.
            VertexBuffer = Graphics.Device.CreateBuffer(BufferType.VertexBuffer, vertices);
            uploadedVBOSize = vertices.Length;
            IndexBuffer = triangles != null ? Graphics.Device.CreateBuffer(BufferType.IndexBuffer, triangles) : null;
            uploadedIBOSize = triangles?.Length ?? 0;

        }

        // Update the data inside the VBO and IBO if it exist if not it just unloads and reuploads
        public void Update()
        {
            if (VertexBuffer == null) {
                Upload();
                return;
            }

            if (uploadedVBOSize != vertices.Length)
            {
                VertexBuffer?.Dispose();
                VertexBuffer = Graphics.Device.CreateBuffer(BufferType.VertexBuffer, vertices);
            }
            else
                Graphics.Device.UpdateBuffer(VertexBuffer, 0, vertices);

            if(triangles == null)
            {
                IndexBuffer?.Dispose();
                IndexBuffer = null;
            }
            else if (IndexBuffer == null || uploadedIBOSize != triangles.Length)
            {
                IndexBuffer?.Dispose();
                IndexBuffer = Graphics.Device.CreateBuffer(BufferType.IndexBuffer, triangles);
            }
            else
                Graphics.Device.UpdateBuffer(IndexBuffer, 0, triangles);

        }

        // Unload from memory (RAM and VRAM)
        public void Unload()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            VertexBuffer = null;
            IndexBuffer = null;
        }

        public override void OnDispose()
        {
            Unload();
        }

        #region Utilities
        public void RecalculateNormals()
        {
            int verticesNum = vertices.Length;
            int indiciesNum = triangles.Length;

            int[] counts = new int[verticesNum];
            for (int i = 0; i < indiciesNum - 3; i += 3)
            {
                int ai = triangles[i];
                int bi = triangles[i + 1];
                int ci = triangles[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 n = Vector3.Normalize(Vector3.Cross(
                        vertices[bi].Position - vertices[ai].Position,
                        vertices[ci].Position - vertices[ai].Position
                    ));

                    vertices[ai].Normal -= n.ToFloat();
                    vertices[bi].Normal -= n.ToFloat();
                    vertices[ci].Normal -= n.ToFloat();

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
                vertices[i].Normal /= counts[i];
        }

        // http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/
        public void RecalculateTangents()
        {
            int verticesNum = vertices.Length;
            int indiciesNum = triangles.Length;

            int[] counts = new int[verticesNum];
            for (int i = 0; i < indiciesNum - 3; i += 3)
            {
                int ai = triangles[i];
                int bi = triangles[i + 1];
                int ci = triangles[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 deltaPos1 = vertices[bi].Position - vertices[ai].Position;
                    Vector3 deltaPos2 = vertices[ci].Position - vertices[ai].Position;

                    Vector2 deltaUV1 = vertices[bi].TexCoord - vertices[ai].TexCoord;
                    Vector2 deltaUV2 = vertices[ci].TexCoord - vertices[ai].TexCoord;

                    double r = 1.0 / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);
                    Vector3 t = (deltaPos1 * deltaUV2.y - deltaPos2 * deltaUV1.y) * r;


                    vertices[ai].Tangent += t.ToFloat();
                    vertices[bi].Tangent += t.ToFloat();
                    vertices[ci].Tangent += t.ToFloat();

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
                vertices[i].Tangent /= counts[i];
        }

        private void SetDefaultFormat()
        {
            LayoutConstructor layout = new();
            layout.Add(0, Format.R32G32B32_Float, InputType.PerVertex);
            layout.Add(1, Format.R32G32_Float, InputType.PerVertex);
            layout.Add(2, Format.R32G32B32_Float, InputType.PerVertex);
            layout.Add(3, Format.R32G32B32_Float, InputType.PerVertex);
            layout.Add(4, Format.R32G32B32_Float, InputType.PerVertex);
            layout.Add(5, Format.R8G8B8A8_UInt, InputType.PerVertex);
            layout.Add(6, Format.R32G32B32A32_Float, InputType.PerVertex);
            format = Graphics.Device.CreateInputLayout(layout.Inputs.ToArray());
        }
        #endregion

        #region Create Primitives

        public struct CubeFace
        {
            public bool enabled;
            public Vector2[] texCoords;
        }

        /// <summary>
        /// 24 vertex cube with per face control
        /// </summary>
        /// <param name="size">Size of the cube</param>
        /// <param name="faces">0=(Z+) 1=(Z-) 2=(Y+) 3=(Y-) 4=(X+) 5=(X-)</param>
        public static Mesh CreateCube(Vector3 size, CubeFace[] faces)
        {
            if (faces.Length != 6) throw new($"The argument '{nameof(faces)}' must have 6 elements!");

            Mesh mesh = new();

            List<Vertex> vertices = new();
            List<ushort> indices = new();

            // Front Face (Z+) - 0
            if (faces[0].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, size.z), TexCoord = faces[0].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, size.z), TexCoord = faces[0].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, size.z), TexCoord = faces[0].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, size.z), TexCoord = faces[0].texCoords[3] });
                indices.AddRange(new ushort[] { 0, 1, 2, 0, 2, 3 });
            }
            // Back Face (Z-) - 1
            if (faces[1].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, -size.z), TexCoord = faces[1].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, -size.z), TexCoord = faces[1].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, -size.z), TexCoord = faces[1].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, -size.z), TexCoord = faces[1].texCoords[3] });
                indices.AddRange(new ushort[] { 4, 5, 6, 4, 6, 7 });
            }
            // Top Face (Y+) - 2
            if (faces[2].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, -size.z), TexCoord = faces[2].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, -size.z), TexCoord = faces[2].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, size.z), TexCoord = faces[2].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, size.z), TexCoord = faces[2].texCoords[3] });
                indices.AddRange(new ushort[] { 8, 9, 10, 8, 10, 11 });
            }
            // Bottom Face (Y-) - 3
            if (faces[3].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, -size.z), TexCoord = faces[3].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, -size.z), TexCoord = faces[3].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, size.z), TexCoord = faces[3].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, size.z), TexCoord = faces[3].texCoords[3] });
                indices.AddRange(new ushort[] { 12, 13, 14, 12, 14, 15 });
            }
            // Right Face (X+) - 4
            if (faces[4].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, size.z), TexCoord = faces[4].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, -size.y, -size.z), TexCoord = faces[4].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, -size.z), TexCoord = faces[4].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(size.x, size.y, size.z), TexCoord = faces[4].texCoords[3] });
                indices.AddRange(new ushort[] { 16, 17, 18, 16, 18, 19 });
            }
            // Left Face (X-) - 5
            if (faces[5].enabled) {
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, -size.z), TexCoord = faces[5].texCoords[0] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, -size.y, size.z), TexCoord = faces[5].texCoords[1] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, size.z), TexCoord = faces[5].texCoords[2] });
                vertices.Add(new Vertex { Position = new Vector3(-size.x, size.y, -size.z), TexCoord = faces[5].texCoords[3] });
                indices.AddRange(new ushort[] { 20, 21, 22, 20, 22, 23 });
            }

            mesh.vertices = [.. vertices];
            mesh.triangles = [.. indices];
            return mesh;
        }

        public static Mesh CreateSphere(float radius, int rings, int slices)
        {
            Mesh mesh = new();

            int vertexCount = (rings + 1) * (slices + 1);
            int triangleCount = rings * slices * 2;

            mesh.vertices = new Vertex[vertexCount];
            mesh.triangles = new ushort[triangleCount * 3];

            int vertexIndex = 0;
            int triangleIndex = 0;

            // Generate vertices and normals
            for (int i = 0; i <= rings; i++)
            {
                float theta = (float)i / rings * (float)Math.PI;
                for (int j = 0; j <= slices; j++)
                {
                    float phi = (float)j / slices * 2.0f * (float)Math.PI;

                    float x = (float)(Math.Sin(theta) * Math.Cos(phi));
                    float y = (float)Math.Cos(theta);
                    float z = (float)(Math.Sin(theta) * Math.Sin(phi));

                    Vertex v = new()
                    {
                        Position = new Vector3(x, y, z) * radius,
                        Normal = new Vector3(x, y, z),
                        TexCoord = new Vector2((float)j / slices, (float)i / rings)
                    };
                    mesh.vertices[vertexIndex++] = v;
                }
            }

            // Generate triangles
            ushort sliceCount = (ushort)(slices + 1);
            for (ushort i = 0; i < rings; i++)
            {
                for (ushort j = 0; j < slices; j++)
                {
                    ushort nextRing = (ushort)((i + 1) * sliceCount);
                    ushort nextSlice = (ushort)(j + 1);

                    mesh.triangles[triangleIndex] = (ushort)(i * sliceCount + j);
                    mesh.triangles[triangleIndex + 1] = (ushort)(nextRing + j);
                    mesh.triangles[triangleIndex + 2] = (ushort)(nextRing + nextSlice);

                    mesh.triangles[triangleIndex + 3] = (ushort)(i * sliceCount + j);
                    mesh.triangles[triangleIndex + 4] = (ushort)(nextRing + nextSlice);
                    mesh.triangles[triangleIndex + 5] = (ushort)(i * sliceCount + nextSlice);

                    triangleIndex += 6;
                }
            }

            return mesh;
        }

        private static Mesh fullScreenQuad;
        public static Mesh GetFullscreenQuad()
        {
            if (fullScreenQuad != null) return fullScreenQuad;

            Mesh mesh = new Mesh();

            mesh.vertices = 
            [
                new Vertex { Position = new Vector3(-1, -1, 0), TexCoord = new Vector2(0, 0) },
                new Vertex { Position = new Vector3(1, -1, 0), TexCoord = new Vector2(1, 0) },
                new Vertex { Position = new Vector3(-1, 1, 0), TexCoord = new Vector2(0, 1) },
                new Vertex { Position = new Vector3(1, 1, 0), TexCoord = new Vector2(1, 1) }
            ];

            mesh.triangles = [0, 2, 1, 2, 3, 1];

            fullScreenQuad = mesh;
            return mesh;
        }

        #endregion

        public CompoundTag Serialize(TagSerializer.SerializationContext ctx)
        {
            CompoundTag compoundTag = new CompoundTag();
            // Serialize to byte[]
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {

                // Serialize bone names
                writer.Write(boneNames?.Length ?? 0);
                if (boneNames != null)
                    for (int i = 0; i < boneNames.Length; i++)
                        writer.Write(boneNames[i]);

                // Serialize bone offsets
                writer.Write(boneOffsets?.Length ?? 0);
                if (boneOffsets != null)
                    for (int i = 0; i < boneOffsets.Length; i++)
                    {
                        writer.Write((float)boneOffsets[i].Item1.x);
                        writer.Write((float)boneOffsets[i].Item1.y);
                        writer.Write((float)boneOffsets[i].Item1.z);
                        writer.Write((float)boneOffsets[i].Item2.x);
                        writer.Write((float)boneOffsets[i].Item2.y);
                        writer.Write((float)boneOffsets[i].Item2.z);
                        writer.Write((float)boneOffsets[i].Item2.w);
                        writer.Write((float)boneOffsets[i].Item3.x);
                        writer.Write((float)boneOffsets[i].Item3.y);
                        writer.Write((float)boneOffsets[i].Item3.z);
                    }

                writer.Write(vertices.Length);
                foreach (var vertex in vertices)
                {
                    writer.Write(vertex.Position.X);
                    writer.Write(vertex.Position.Y);
                    writer.Write(vertex.Position.Z);

                    writer.Write(vertex.TexCoord.X);
                    writer.Write(vertex.TexCoord.Y);

                    writer.Write(vertex.Normal.X);
                    writer.Write(vertex.Normal.Y);
                    writer.Write(vertex.Normal.Z);

                    writer.Write(vertex.Color.X);
                    writer.Write(vertex.Color.Y);
                    writer.Write(vertex.Color.Z);

                    writer.Write(vertex.Tangent.X);
                    writer.Write(vertex.Tangent.Y);
                    writer.Write(vertex.Tangent.Z);

                    writer.Write(vertex.BoneIndex0);
                    writer.Write(vertex.BoneIndex1);
                    writer.Write(vertex.BoneIndex2);
                    writer.Write(vertex.BoneIndex3);

                    writer.Write(vertex.Weight0);
                    writer.Write(vertex.Weight1);
                    writer.Write(vertex.Weight2);
                    writer.Write(vertex.Weight3);
                }

                SerializeArray(writer, triangles);

                compoundTag.Add("Data", new ByteArrayTag(memoryStream.ToArray()));
            }
            // TODO: Serialize the format? Might not be needed since in editor its always the default, only time its not is when the user procedurally creates a mesh
            return compoundTag;
        }

        public void Deserialize(CompoundTag value, TagSerializer.SerializationContext ctx)
        {
            using (MemoryStream memoryStream = new MemoryStream(value["Data"].ByteArrayValue))
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                int boneCount = reader.ReadInt32();
                if (boneCount > 0)
                {
                    boneNames = new string[boneCount];
                    for (int i = 0; i < boneCount; i++)
                        boneNames[i] = reader.ReadString();
                }

                int boneOffsetCount = reader.ReadInt32();
                if (boneOffsetCount > 0)
                {
                    boneOffsets = new (Vector3, Quaternion, Vector3)[boneOffsetCount];
                    for (int i = 0; i < boneOffsetCount; i++)
                    {
                        Vector3 v = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        Quaternion q = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        Vector3 s = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        boneOffsets[i] = (v, q, s);
                    }
                }


                int vertexCount = reader.ReadInt32();
                vertices = new Vertex[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = new Vertex
                    {
                        Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        TexCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                        Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        Color = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        Tangent = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        BoneIndex0 = reader.ReadByte(),
                        BoneIndex1 = reader.ReadByte(),
                        BoneIndex2 = reader.ReadByte(),
                        BoneIndex3 = reader.ReadByte(),
                        Weight0 = reader.ReadSingle(),
                        Weight1 = reader.ReadSingle(),
                        Weight2 = reader.ReadSingle(),
                        Weight3 = reader.ReadSingle()
                    };
                }
                triangles = DeserializeArray<ushort> (reader);

            }
            SetDefaultFormat();
        }

        // Helper method to serialize an array
        private static void SerializeArray<T>(BinaryWriter writer, T[] array) where T : struct
        {
            if (array == null)
            {
                writer.Write(false);
                return;
            }
            writer.Write(true);
            int length = array.Length;
            writer.Write(length);
            int elementSize = Marshal.SizeOf<T>();
            byte[] bytes = new byte[length * elementSize];
            System.Buffer.BlockCopy(array, 0, bytes, 0, bytes.Length);
            writer.Write(bytes);
        }

        // Helper method to deserialize an array
        private static T[] DeserializeArray<T>(BinaryReader reader) where T : struct
        {
            bool isNotNull = reader.ReadBoolean();
            if (!isNotNull) return null;
            int length = reader.ReadInt32();
            int elementSize = Marshal.SizeOf<T>();

            byte[] bytes = reader.ReadBytes(length * elementSize);
            T[] array = new T[length];

            System.Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);

            return array;
        }

        public struct Vertex
        {
            public System.Numerics.Vector3 Position;
            public System.Numerics.Vector2 TexCoord;
            public System.Numerics.Vector3 Normal;
            public System.Numerics.Vector3 Color;
            public System.Numerics.Vector3 Tangent;
            public byte BoneIndex0, BoneIndex1, BoneIndex2, BoneIndex3;
            public float Weight0, Weight1, Weight2, Weight3;
        }
    }

}
