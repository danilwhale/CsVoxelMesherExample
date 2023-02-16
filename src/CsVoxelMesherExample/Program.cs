using System.Numerics;
using raylibExtras.CsVoxelMesherExample.rLights;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayMath;

namespace raylibExtras.CsVoxelMesherExample;

internal class Program
{
    const int ChunkSize = 16;
    const int ChunkDepth = 16;

    static short[] VoxelChunk = new short[ChunkSize * ChunkSize * ChunkDepth];

    public static Rectangle[] BlockColors =
    {
        new Rectangle(0, 0, .25f, 1),
        new Rectangle(0.25f, 0, 0.5f, 1),
        new Rectangle(0.5f, 0, 0.75f, 1),
        new Rectangle(0.75f, 0, 1, 1)
    };

    static int GetIndex(int h, int v, int d) => (d * (ChunkSize * ChunkSize)) + (v * ChunkSize) + h;

    static void Main()
    {
        InitWindow(1200, 800, "voxels!");
        SetTargetFPS(144);

        RenderTexture tileTexture = LoadRenderTexture(64, 16);
        BeginTextureMode(tileTexture);
        ClearBackground(BLANK);
        DrawRectangle(0, 0, 16, 16, DARKBROWN);
        DrawRectangle(16, 0, 16, 16, BROWN);
        DrawRectangle(32, 0, 16, 16, GREEN);
        DrawRectangle(48, 0, 16, 16, GOLD);
        EndTextureMode();

        Camera3D camera = new Camera3D();
        camera.fovy = 45;
        camera.up.Y = 1;
        camera.position.X = 32;
        camera.position.Z = 32;
        camera.position.Y = 16;

        Shader shader = LoadShader("resources/shaders/base_lighting.vs", "resources/shaders/lighting.fs");
        unsafe
        {
            shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = GetShaderLocation(shader, "viewPos");
        }

        int ambientLoc = GetShaderLocation(shader, "ambient");
        float[] val = { .1f, .1f, .1f, 1.0f };
        SetShaderValue(shader, ambientLoc, val, ShaderUniformDataType.SHADER_UNIFORM_VEC4);

        Light[] lights = new Light[4];
        lights[0] = Light.CreateLight(LightType.Directional, Vector3.Zero, new Vector3(-2, -4, -3), WHITE, shader);
        lights[1] = Light.CreateLight(LightType.Directional, Vector3.Zero, new Vector3(2, 2, 5), GRAY, shader);

        BuildChunk();

        Mesh mesh = MeshChunk();

        Material mat = LoadMaterialDefault();
        unsafe
        {
            mat.maps[0].color = WHITE;
            mat.maps[0].texture = tileTexture.texture;
        }
        mat.shader = shader;

        while (!WindowShouldClose())
        {
            camera.position = Vector3.Transform(camera.position, Matrix4x4.CreateRotationY(GetFrameTime() * DEG2RAD * 15));

            Light.UpdateLightValues(shader, ref lights[0]);
            Light.UpdateLightValues(shader, ref lights[1]);

            unsafe
            {
                SetShaderValue(shader, shader.locs[(int)ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], camera.position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
            }

            BeginDrawing();
            ClearBackground(SKYBLUE);

            BeginMode3D(camera);

            DrawGrid(10, 10);
            DrawSphere(Vector3.Zero, .125f, GRAY);
            DrawSphere(Vector3.UnitX, .0125f, RED);
            DrawSphere(Vector3.UnitZ, .0125f, GREEN);

            DrawMesh(mesh, mat, MatrixIdentity());

            EndMode3D();

            DrawFPS(0, 0);
            EndDrawing();
        }
    }

    // build a simple random voxel chunk
    static void BuildChunk()
    {
        // fill the chunk with layers of blocks
        for (int d = 0; d < ChunkDepth; d++)
        {
            short block = 0;

            if (d > 6)
            {
                block = 1;
                if (d > 8)
                {
                    block = 2;
                    if (d > 10)
                        block = -1;
                }
            }

            for (int v = 0; v < ChunkSize; v++)
            {
                for (int h = 0; h < ChunkSize; h++)
                {
                    int index = GetIndex(h, v, d);

                    VoxelChunk[index] = block;
                }
            }
        }

        // Remove some chunks 
        for (int i = 0; i < 500; i++)
        {
            int h = GetRandomValue(0, ChunkSize - 1);
            int v = GetRandomValue(0, ChunkSize - 1);
            int d = GetRandomValue(0, 10);

            int index = GetIndex(h, v, d);

            VoxelChunk[index] = -1;
        }

        // Add some gold
        for (int i = 0; i < 100; i++)
        {
            int h = GetRandomValue(0, ChunkSize - 1);
            int v = GetRandomValue(0, ChunkSize - 1);
            int d = GetRandomValue(0, 10);

            int index = GetIndex(h, v, d);

            VoxelChunk[index] = 3;
        }
    }

    static bool BlockIsSolid(int h, int v, int d)
    {
        if (h < 0 || h >= ChunkSize)
            return false;

        if (v < 0 || v >= ChunkSize)
            return false;

        if (d < 0 || d >= ChunkDepth)
            return false;

        return VoxelChunk[GetIndex(h, v, d)] >= 0;
    }

    //check all the adjacent blocks to see if they are open, if they are, we need a face for that side of the block.
    static int GetChunkFaceCount()
    {
        int count = 0;
        for (int d = 0; d < ChunkDepth; d++)
        {
            for (int v = 0; v < ChunkSize; v++)
            {
                for (int h = 0; h < ChunkSize; h++)
                {
                    if (!BlockIsSolid(h, v, d))
                        continue;

                    if (!BlockIsSolid(h + 1, v, d))
                        count++;

                    if (!BlockIsSolid(h - 1, v, d))
                        count++;

                    if (!BlockIsSolid(h, v + 1, d))
                        count++;

                    if (!BlockIsSolid(h, v - 1, d))
                        count++;

                    if (!BlockIsSolid(h, v, d + 1))
                        count++;

                    if (!BlockIsSolid(h, v, d - 1))
                        count++;
                }
            }
        }

        return count;
    }

    static Mesh MeshChunk()
    {
        Mesh mesh = new Mesh();
        CubeGeometryBuilder builder = new CubeGeometryBuilder(ref mesh);

        // figure out how many faces will be in this chunk and allocate a mesh that can store that many
        builder.Allocate(GetChunkFaceCount());

        for (int d = 0; d < ChunkDepth; d++)
        {
            for (int v = 0; v < ChunkSize; v++)
            {
                for (int h = 0; h < ChunkSize; h++)
                {
                    if (!BlockIsSolid(h, v, d))
                        continue;

                    // build up the list of faces that this block needs
                    bool[] faces = { false, false, false, false, false, false };

                    if (!BlockIsSolid(h - 1, v, d))
                        faces[CubeGeometryBuilder.EastFace] = true;

                    if (!BlockIsSolid(h + 1, v, d))
                        faces[CubeGeometryBuilder.WestFace] = true;

                    if (!BlockIsSolid(h, v - 1, d))
                        faces[CubeGeometryBuilder.NorthFace] = true;

                    if (!BlockIsSolid(h, v + 1, d))
                        faces[CubeGeometryBuilder.SouthFace] = true;

                    if (!BlockIsSolid(h, v, d + 1))
                        faces[CubeGeometryBuilder.UpFace] = true;

                    if (!BlockIsSolid(h, v, d - 1))
                        faces[CubeGeometryBuilder.DownFace] = true;

                    // build the faces that hit open air for this voxel block
                    builder.AddCube(new Vector3((float)h, (float)d, (float)v), faces, (int)VoxelChunk[GetIndex(h, v, d)]);
                }
            }
        }

        unsafe { UploadMesh(&mesh, false); }

        return mesh;
    }
}