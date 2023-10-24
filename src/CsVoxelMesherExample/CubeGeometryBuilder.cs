namespace CsVoxelMesherExample;

public class CubeGeometryBuilder
{
    public Mesh Mesh;

    private int _triangleIndex = 0;
    private int _vertIndex = 0;

    private Vector3 _normal = new Vector3(0, 0, 0);
    private Color _vertColor = WHITE;
    private Vector2 _uv = new Vector2(0, 0);

    public CubeGeometryBuilder(Mesh mesh)
    {
        Mesh = mesh;
    }
    
    public unsafe void Allocate(int faces)
    {
        Mesh.vertexCount = faces * 6;
        Mesh.triangleCount = faces * 2;

        Mesh.vertices = (float*)MemAlloc((uint)(Mesh.vertexCount * 3 * sizeof(float)));
        Mesh.normals = (float*)MemAlloc((uint)(Mesh.vertexCount * 3 * sizeof(float)));
        Mesh.texcoords = (float*)MemAlloc((uint)(Mesh.vertexCount * 2 * sizeof(float)));
        Mesh.colors = null; // (byte*)MemAlloc((uint)(Mesh.vertexCount * 4 * sizeof(byte))); // uncomment this if you want colored vertices

        Mesh.animNormals = null;
        Mesh.animVertices = null;
        Mesh.boneIds = null;
        Mesh.boneWeights = null;
        Mesh.tangents = null;
        Mesh.indices = null;
        Mesh.texcoords2 = null;
    }

    public void SetNormal(Vector3 normal)
    {
        _normal = normal;
    }

    public void SetNormal(float x, float y, float z)
    {
        _normal = new Vector3(x, y, z);
    }

    public void SetUv(Vector2 uv)
    {
        _uv = uv;
    }

    public void SetUv(float x, float y)
    {
        _uv = new Vector2(x, y);
    }

    public unsafe void PushVertex(Vector3 vertex, float x, float y, float z)
    {
        // index = tri * (n * 3) + vert * n
        // n = component count (vector2 = 2, vector3 = 3, etc.)
        
        // color index: tri * (4 * 3) + vert * 4
        int index = _triangleIndex * 12 + _vertIndex * 4;

        if (Mesh.colors != null)
        {
            Mesh.colors[index + 0] = _vertColor.r;
            Mesh.colors[index + 1] = _vertColor.g;
            Mesh.colors[index + 2] = _vertColor.b;
            Mesh.colors[index + 3] = _vertColor.a;
        }

        if (Mesh.texcoords != null)
        {
            // texcoords index: tri * (2 * 3) + vert * 2
            index = _triangleIndex * 6 + _vertIndex * 2;
            Mesh.texcoords[index + 0] = _uv.X;
            Mesh.texcoords[index + 1] = _uv.Y;
        }

        if (Mesh.normals != null)
        {
            // normals index: tri * (3 * 3) + vert * 3
            index = _triangleIndex * 9 + _vertIndex * 3;
            Mesh.normals[index + 0] = _normal.X;
            Mesh.normals[index + 1] = _normal.Y;
            Mesh.normals[index + 2] = _normal.Z;
        }
        
        // vertex index: try * (3 * 3) + vert * 3
        index = _triangleIndex * 9 + _vertIndex * 3;
        Mesh.vertices[index + 0] = vertex.X + x;
        Mesh.vertices[index + 1] = vertex.Y + y;
        Mesh.vertices[index + 2] = vertex.Z + z;

        _vertIndex++;
        if (_vertIndex > 2)
        {
            _triangleIndex++;
            _vertIndex = 0;
        }
    }

    public void AddCube(Vector3 position, bool[] faces, BlockType block)
    {
        Rectangle uv = Program.BlockColors[(int)(block - 1)];
        
        SetUv(0, 0);
        
        // Z-
        if (faces[(int)Face.North])
        {
            SetNormal(0, 0, -1);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 0);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 0);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 0, 0);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 1, 0);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 1, 0);
        }
        
        // Z+
        if (faces[(int)Face.South])
        {
            SetNormal(0, 0, 1);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 1);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 1);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 1);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 1);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 1, 1);
        }
        
        // X+
        if (faces[(int)Face.West])
        {
            SetNormal(1, 0, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 1, 0, 0);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 1, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 1, 0);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 1);
        }
        
        // X-
        if (faces[(int)Face.East])
        {
            SetNormal(-1, 0, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 0, 1);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 0, 1, 0);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 0, 1);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 0, 1, 1);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 0, 1, 0);
        }
        
        // Y+
        if (faces[(int)Face.Up])
        {
            SetNormal(0, 1, 0);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 1, 0);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 1);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 1, 0);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 1, 0);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 1, 1);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 1, 1);
        }
        
        SetUv(0, 0);
        // Y-
        if (faces[(int)Face.Down])
        {
            SetNormal(0, -1, 0);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 0);
            
            SetUv(uv.width, uv.y);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.x, uv.y);
            PushVertex(position, 0, 0, 0);
            
            SetUv(uv.width, uv.height);
            PushVertex(position, 1, 0, 1);
            
            SetUv(uv.x, uv.height);
            PushVertex(position, 0, 0, 1);
        }
    }
}