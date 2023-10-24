/**********************************************************************************************
*
*   raylib.lights - Some useful functions to deal with lights data
*
*   CONFIGURATION:
*
*   #define RLIGHTS_IMPLEMENTATION
*       Generates the implementation of the library into the included file.
*       If not defined, the library is in header only mode and can be included in other headers 
*       or source files without problems. But only ONE file should hold the implementation.
*
*   LICENSE: zlib/libpng
*
*   Copyright (c) 2017-2020 Victor Fisac (@victorfisac) and Ramon Santamaria (@raysan5)
*
*   This software is provided "as-is", without any express or implied warranty. In no event
*   will the authors be held liable for any damages arising from the use of this software.
*
*   Permission is granted to anyone to use this software for any purpose, including commercial
*   applications, and to alter it and redistribute it freely, subject to the following restrictions:
*
*     1. The origin of this software must not be misrepresented; you must not claim that you
*     wrote the original software. If you use this software in a product, an acknowledgment
*     in the product documentation would be appreciated but is not required.
*
*     2. Altered source versions must be plainly marked as such, and must not be misrepresented
*     as being the original software.
*
*     3. This notice may not be removed or altered from any source distribution.
*
**********************************************************************************************/

namespace CsVoxelMesherExample.rLights;

public struct Light
{
    public const int MaxLights = 4;
    public static int LightsCount = 0;

    public LightType Type;
    public Vector3 Position;
    public Vector3 Target;
    public Color Color;
    public bool Enabled;

    public int EnabledLoc;
    public int TypeLoc;
    public int PosLoc;
    public int TargetLoc;
    public int ColorLoc;

    public Light(LightType type, Vector3 position, Vector3 target, Color color, Shader shader)
    {
        if (LightsCount >= MaxLights) return;

        Enabled = true;
        Type = type;
        Position = position;
        Target = target;
        Color = color;

        string enabledName = $"lights[{LightsCount}].enabled";
        string typeName = $"lights[{LightsCount}].type";
        string posName = $"lights[{LightsCount}].position";
        string targetName = $"lights[{LightsCount}].target";
        string colorName = $"lights[{LightsCount}].color";

        EnabledLoc = GetShaderLocation(shader, enabledName);
        TypeLoc = GetShaderLocation(shader, typeName);
        PosLoc = GetShaderLocation(shader, posName);
        TargetLoc = GetShaderLocation(shader, targetName);
        ColorLoc = GetShaderLocation(shader, colorName);

        UpdateValues(shader);

        LightsCount++;
    }

    public void UpdateValues(Shader shader)
    {
        SetShaderValue(shader, EnabledLoc, Enabled, ShaderUniformDataType.SHADER_UNIFORM_INT);
        SetShaderValue(shader, TypeLoc, Type, ShaderUniformDataType.SHADER_UNIFORM_INT);

        float[] position = { Position.X, Position.Y, Position.Z };
        SetShaderValue(shader, PosLoc, position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        float[] target = { Target.X, Target.Y, Target.Z };
        SetShaderValue(shader, TargetLoc, target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);

        float[] color = { Color.r / 255f, Color.g / 255f, Color.b / 255f, Color.a / 255f };
        SetShaderValue(shader, ColorLoc, color, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
}