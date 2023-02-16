using System.Numerics;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;


namespace raylibExtras.CsVoxelMesherExample.rLights;

public struct Light
{
    public const int MaxLights = 4;
    public static int LightsCount { get; set; } = 0;

    public LightType Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Color Color { get; set; }
    public bool Enabled { get; set; }

    public int EnabledLoc { get; set; }
    public int TypeLoc { get; set; }
    public int PosLoc { get; set; }
    public int TargetLoc { get; set; }
    public int ColorLoc { get; set; }

    public static Light CreateLight(LightType type, Vector3 position, Vector3 target, Color color, Shader shader)
    {
        Light light = new Light();

        if (LightsCount < MaxLights)
        {
            light.Enabled = true;
            light.Type = type;
            light.Position = position;
            light.Target = target;
            light.Color = color;

            string enabledName = $"lights[{LightsCount.ToString()}].enabled";
            string typeName = $"lights[{LightsCount.ToString()}].type";
            string posName = $"lights[{LightsCount.ToString()}].position";
            string targetName = $"lights[{LightsCount.ToString()}].target";
            string colorName = $"lights[{LightsCount.ToString()}].color";

            light.EnabledLoc = GetShaderLocation(shader, enabledName);
            light.TypeLoc = GetShaderLocation(shader, typeName);
            light.PosLoc = GetShaderLocation(shader, posName);
            light.TargetLoc = GetShaderLocation(shader, targetName);
            light.ColorLoc = GetShaderLocation(shader, colorName);

            UpdateLightValues(shader, ref light);

            LightsCount++;
        }

        return light;
    }

    public static void UpdateLightValues(Shader shader, ref Light light)
    {
        SetShaderValue(shader, light.EnabledLoc, light.Enabled, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(shader, light.TypeLoc, light.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);

        float[] position = { light.Position.X, light.Position.Y, light.Position.Z };
        SetShaderValue(shader, light.PosLoc, position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        float[] target = { light.Target.X, light.Target.Y, light.Target.Z };
        SetShaderValue(shader, light.TargetLoc, target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        float[] color = { (float)light.Color.r/(float)255, (float)light.Color.g/(float)255,
                       (float)light.Color.b/(float)255, (float)light.Color.a/(float)255 };

        SetShaderValue(shader, light.ColorLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
}