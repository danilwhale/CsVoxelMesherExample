using System.Numerics;
using System.Runtime.CompilerServices;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

namespace raylibExtras.CsVoxelMesherExample;

/// <summary>
/// Contains tools to manage chunk rendering using Mesh
/// </summary>
public unsafe class CubeGeometryBuilder
{
    // setup the builder with the mesh it is going to fill out
    public CubeGeometryBuilder(ref Mesh mesh)
    {
        MeshRef = (Mesh*)Unsafe.AsPointer<Mesh>(ref mesh);
    }

    // indexes for the 6 faces of a cube
    public const int SouthFace = 0;
    public const int NorthFace = 1;
    public const int WestFace = 2;
    public const int EastFace = 3;
    public const int UpFace = 4;
    public const int DownFace = 5;

    // we need to know how many triangles are going to be in the mesh before we start
    // this way we can allocate the correct buffer sizes for the mesh
    public void Allocate(int triangles)
    {
        // there are 
        MeshRef->vertexCount = triangles * 6;
        MeshRef->triangleCount = triangles * 2;

        MeshRef->vertices = (float*)MemAlloc((uint)(sizeof(float) * 3 * MeshRef->vertexCount));
        MeshRef->normals = (float*)MemAlloc((uint)(sizeof(float) * 3 * MeshRef->vertexCount));
        MeshRef->texcoords = (float*)MemAlloc((uint)(sizeof(float) * 2 * MeshRef->vertexCount));
        MeshRef->colors = null;  // static_cast<unsigned char*>(MemAlloc(sizeof(unsigned char) * 4 * MeshRef.vertexCount));

        MeshRef->animNormals = null;
        MeshRef->animVertices = null;
        MeshRef->boneIds = null;
        MeshRef->boneWeights = null;
        MeshRef->tangents = null;
        MeshRef->indices = null;
        MeshRef->texcoords2 = null;
    }

    public void SetNormal(Vector3 value) { Normal = value; }
    public void SetNormal(float x, float y, float z) { Normal = new Vector3(x, y, z); }
    public void SetSetUV(Vector2 value) { UV = value; }
    public void SetSetUV(float x, float y) { UV = new Vector2(x, y); }

    public void PushVertex(Vector3 vertex, float xOffset = 0, float yOffset = 0, float zOffset = 0)
    {
        int index = TriangleIndex * 12 + VertIndex * 3;

        if (MeshRef->colors != null)
        {
            MeshRef->colors[index] = VertColor.r;
            MeshRef->colors[index + 1] = VertColor.g;
            MeshRef->colors[index + 2] = VertColor.b;
            MeshRef->colors[index + 3] = VertColor.a;
        }

        if (MeshRef->texcoords != null)
        {
            index = TriangleIndex * 6 + VertIndex * 2;
            MeshRef->texcoords[index] = UV.X;
            MeshRef->texcoords[index + 1] = UV.Y;
        }

        if (MeshRef->normals != null)
        {
            index = TriangleIndex * 9 + VertIndex * 3;
            MeshRef->normals[index] = Normal.X;
            MeshRef->normals[index + 1] = Normal.Y;
            MeshRef->normals[index + 2] = Normal.Z;
        }

        index = TriangleIndex * 9 + VertIndex * 3;
        MeshRef->vertices[index] = vertex.X + xOffset;
        MeshRef->vertices[index + 1] = vertex.Y + yOffset;
        MeshRef->vertices[index + 2] = vertex.Z + zOffset;

        VertIndex++;
        if (VertIndex > 2)
        {
            TriangleIndex++;
            VertIndex = 0;
        }
    }

    public void AddCube(Vector3 position, bool[] faces, int block)
    {
        Rectangle uvRect = Program.BlockColors[block];
        SetSetUV(0, 0);
        //z-
        if (faces[NorthFace])
        {
            SetNormal(0, 0, -1);
            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 0);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 0, 0);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 1, 0);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 1, 0);
        }

        // z+
        if (faces[SouthFace])
        {
            SetNormal(0, 0, 1);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 0, 1);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 0, 1);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 1);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 0, 1);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 1);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 1, 1);
        }

        // x+
        if (faces[WestFace])
        {
            SetNormal(1, 0, 0);
            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 1, 0, 1);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 1, 0, 0);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 1, 0);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 1, 0, 1);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 1, 0);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 1);
        }

        // x-
        if (faces[EastFace])
        {
            SetNormal(-1, 0, 0);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 0, 1);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 0, 1, 0);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 0, 0);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 0, 1);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 0, 1, 1);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 0, 1, 0);
        }

        if (faces[UpFace])
        {
            SetNormal(0, 1, 0);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 1, 0);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 1);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 1, 0);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 1, 0);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 1, 1);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 1, 1);
        }

        SetSetUV(0, 0);
        if (faces[DownFace])
        {
            SetNormal(0, -1, 0);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 0, 0);

            SetSetUV(uvRect.width, uvRect.y);
            PushVertex(position, 1, 0, 0);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 0, 1);

            SetSetUV(uvRect.x, uvRect.y);
            PushVertex(position, 0, 0, 0);

            SetSetUV(uvRect.width, uvRect.height);
            PushVertex(position, 1, 0, 1);

            SetSetUV(uvRect.x, uvRect.height);
            PushVertex(position, 0, 0, 1);
        }
    }

    protected Mesh* MeshRef;

    protected int TriangleIndex = 0;
    protected int VertIndex = 0;

    protected Vector3 Normal = Vector3.Zero;
    protected Color VertColor = WHITE;
    protected Vector2 UV = Vector2.Zero;
};