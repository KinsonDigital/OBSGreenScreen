#version 450 core

layout(location = 0) in vec2 a_vertexPos;
/*
    pass_shape can represent either a rectangle or an ellipse.
    The center of either shape is the position represented by
    vec4.x and vec4.y.  The width is represented by vec4.z
    and the height is represented by vec4.w
*/
layout(location = 1) in vec4 a_shape;

layout(location = 2) in vec4 a_color;

// This represents a boolean value with 0.0 == false and 1.0 == true
layout(location = 3) in float a_isFilled;
layout(location = 4) in float a_borderThickness;
layout(location = 5) in float a_cornerRadius;

out vec4 pass_shape;
out vec4 pass_color;
out float pass_isFilled;
out float pass_borderThickness;
out float pass_cornerRadius;

void main()
{
    // Pass all of the shape information to the fragment shader
    pass_shape = a_shape;
    pass_color = a_color;
    pass_isFilled = a_isFilled;
    pass_borderThickness = a_borderThickness;
    pass_cornerRadius = a_cornerRadius;

    gl_Position = vec4(a_vertexPos, 1.0, 1.0);
}
