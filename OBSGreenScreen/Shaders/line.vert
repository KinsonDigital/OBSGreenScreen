﻿#version 450 core

layout(location = 0) in vec2 a_vertexPos;
layout(location = 1) in vec4 a_color;

out vec4 pass_color; // Should be in 0-1 range, not 0-255 range

void main()
{
    pass_color = a_color;

    gl_Position = vec4(a_vertexPos, 1.0, 1.0);
}
