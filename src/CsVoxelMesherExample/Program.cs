// basic setup for comfy experience

global using System.Numerics;
global using ZeroElectric.Vinculum;
global using static ZeroElectric.Vinculum.Raylib;
using CsVoxelMesherExample.rLights;

namespace CsVoxelMesherExample;

internal class Program
{
    public const int ChunkSize = 16;
    public const int ChunkDepth = 16;

    public static BlockType[] VoxelChunk = new BlockType[ChunkSize * ChunkSize * ChunkDepth];

    public static Rectangle[] BlockColors =
    {
        new Rectangle(0.0f, 0.0f, 0.25f, 1.0f),
        new Rectangle(0.25f, 0.0f, 0.5f, 1.0f),
        new Rectangle(0.5f, 0.0f, 0.75f, 1.0f),
        new Rectangle(0.75f, 0.0f, 1.0f, 1.0f)
    };

    public static unsafe void Main(string[] args)
    {
        InitWindow(1200, 800, "voxels!");
        SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);

        RenderTexture tileTexture = LoadRenderTexture(4 * 16, 1 * 16);
        BeginTextureMode(tileTexture);
        ClearBackground(BLANK);
        
        DrawRectangle(0, 0, 16, 16, DARKBROWN);
        DrawRectangle(16, 0, 16, 16, BROWN);
        DrawRectangle(32, 0, 16, 16, GREEN);
        DrawRectangle(48, 0, 16, 16, GOLD);
        
        EndTextureMode();

        Camera3D camera = new Camera3D(new Vector3(32, 16, 32), Vector3.Zero, Vector3.UnitY, 45,
            CameraProjection.CAMERA_PERSPECTIVE);

        Shader shader = LoadShader("Resources/Shaders/BaseLighting.vert", "Resources/Shaders/Lighting.frag");
        shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = GetShaderLocation(shader, "viewPos");

        int ambientLoc = GetShaderLocation(shader, "ambient");
        float[] color = { 0.1f, 0.1f, 0.1f, 1.0f };
        SetShaderValue(shader, ambientLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

        Light[] lights = new Light[Light.MaxLights];
        lights[0] = new Light(LightType.Directional, Vector3.Zero, new Vector3(-2, -4, -3), WHITE, shader);
        lights[1] = new Light(LightType.Directional, Vector3.Zero, new Vector3(2, 2, 5), GRAY, shader);
        
        BuildChunk();
        Mesh mesh = MeshChunk();

        Material mat = LoadMaterialDefault();
        mat.maps[(int)MaterialMapIndex.MATERIAL_MAP_ALBEDO].color = WHITE;
        mat.maps[(int)MaterialMapIndex.MATERIAL_MAP_ALBEDO].texture = tileTexture.texture;
        mat.shader = shader;

        while (!WindowShouldClose())
        {
            UpdateCamera(ref camera, CameraMode.CAMERA_ORBITAL);
            
            lights[0].UpdateValues(shader);
            lights[1].UpdateValues(shader);
            
            SetShaderValue(shader, shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
                            
            BeginDrawing();
            ClearBackground(SKYBLUE);
            
            BeginMode3D(camera);
            
            DrawGrid(10, 10);
            DrawSphere(Vector3.Zero, 0.125f, GRAY);
            DrawSphere(Vector3.UnitX, 0.125f, RED);
            DrawSphere(Vector3.UnitZ, 0.125f, GREEN);
            
            DrawMesh(mesh, mat, Matrix4x4.Transpose(Matrix4x4.CreateTranslation(-ChunkSize / 2f, 0, -ChunkSize / 2f)));
            
            EndMode3D();
            
            DrawFPS(0, 0);
            
            EndDrawing();
        }
        
        CloseWindow();
    }

    private static int GetIndex(int x, int y, int z)
    {
        return x + ChunkSize * (y + ChunkDepth * z);
    }

    private static void BuildChunk()
    {
        for (int y = 0; y < ChunkDepth; y++)
        {
            BlockType block = y switch
            {
                > 10 => BlockType.Air,
                > 8 => BlockType.Grass,
                > 6 => BlockType.Dirt,
                _ => BlockType.DarkDirt
            };

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    int i = GetIndex(x, y, z);
                    VoxelChunk[i] = block;
                }
            }
        }

        Random random = new Random();
        for (int i = 0; i < 500; i++)
        {
            int x = random.Next(ChunkSize - 1);
            int y = random.Next(10);
            int z = random.Next(ChunkSize - 1);

            int index = GetIndex(x, y, z);
            VoxelChunk[index] = BlockType.Air;
        }

        for (int i = 0; i < 100; i++)
        {
            int x = random.Next(ChunkSize - 1);
            int y = random.Next(10);
            int z = random.Next(ChunkSize - 1);

            int index = GetIndex(x, y, z);
            VoxelChunk[index] = BlockType.Gold;
        }
    }

    private static bool IsBlockSolid(int x, int y, int z)
    {
        if (x is < 0 or >= ChunkSize) return false;
        if (y is < 0 or >= ChunkSize) return false;
        if (z is < 0 or >= ChunkSize) return false;

        int index = GetIndex(x, y, z);
        return VoxelChunk[index] > BlockType.Air;
    }

    private static int GetChunkFaceCount()
    {
        int count = 0;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkDepth; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    if (!IsBlockSolid(x, y, z)) continue;

                    if (!IsBlockSolid(x + 1, y, z)) count++;
                    if (!IsBlockSolid(x - 1, y, z)) count++;
                    if (!IsBlockSolid(x, y + 1, z)) count++;
                    if (!IsBlockSolid(x, y - 1, z)) count++;
                    if (!IsBlockSolid(x, y, z + 1)) count++;
                    if (!IsBlockSolid(x, y, z - 1)) count++;
                }
            }
        }

        return count;
    }

    private static unsafe Mesh MeshChunk()
    {
        CubeGeometryBuilder builder = new CubeGeometryBuilder(new Mesh());
        
        builder.Allocate(GetChunkFaceCount());

        int count = 0;
        
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkDepth; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    if (!IsBlockSolid(x, y, z)) continue;

                    bool[] faces = { false, false, false, false, false, false };

                    if (!IsBlockSolid(x - 1, y, z)) faces[(int)Face.East] = true;
                    if (!IsBlockSolid(x + 1, y, z)) faces[(int)Face.West] = true;
                    if (!IsBlockSolid(x, y - 1, z)) faces[(int)Face.Down] = true;
                    if (!IsBlockSolid(x, y + 1, z)) faces[(int)Face.Up] = true;
                    if (!IsBlockSolid(x, y, z - 1)) faces[(int)Face.North] = true;
                    if (!IsBlockSolid(x, y, z + 1)) faces[(int)Face.South] = true;
                    
                    builder.AddCube(new Vector3(x, y, z), faces, VoxelChunk[GetIndex(x, y, z)]);
                }
            }
        }
        
        fixed (Mesh* m = &builder.Mesh)
            UploadMesh(m, false);

        return builder.Mesh;
    }
}