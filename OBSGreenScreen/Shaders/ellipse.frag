#version 450 core

/* NOTES:
    The 'pass_' prefix means the variable was passed into this
    fragment shader from the vertex shader.
*/
const vec4 ERROR_COLOR = vec4(1.0, 0.0, 1.0, 1.0);

/*
    pass_shape can represent either a rectangle or an ellipse.
    The center of either shape is the position represented by
    vec4.x and vec4.y.  The width is represented by vec4.z
    and the height is represented by vec4.w
*/
in vec4 pass_shape;

/*
    The color of the shape.
    vec4.x = red
    vec4.y = green
    vec4.z = blue
    vec4.w = alpha
*/
in vec4 pass_color;

// This represents a boolean value with 0.0 == false and 1.0 == true
in float pass_isFilled;
in float pass_borderThickness;
in float pass_cornerRadius;

// NOTE: The frag coord is in pixel units.
// The line below sets the frag coordinate to be relative
// to the origin being at the upper left corner of the window
layout(origin_upper_left) in vec4 gl_FragCoord;

// The final color of the current pixel depending on where in the shape it is at
out vec4 finalColor;

/*
    Represents an ellipse to be rendered to the screen.
*/
struct Ellipse
{
    vec2 Position;
    float RadiusX;
    float RadiusY;
};

float borderThickness = 1.0; // The clamped border thickness

/*
    Returns the value squared.
*/
float squared(float value)
{
    return pow(value, 2.0);
}

/*
    Returns a value indicating if the given ellipse contains the current pixel/fragment.
*/
bool containedByEllipse(Ellipse ellipse)
{
    // Refer to link for more info
    // https://www.geeksforgeeks.org/check-if-a-point-is-inside-outside-or-on-the-ellipse/
    return squared(gl_FragCoord.x - ellipse.Position.x) /
        squared(ellipse.RadiusX) +
        squared(gl_FragCoord.y - ellipse.Position.y) /
        squared(ellipse.RadiusY) <= 1.0;
}

/*
    Maps the given value from one range to another.

    @param value The value to map.
    @param fromStart The from starting range value.
    @param fromStop The from ending range value.
    @param toStart The to starting range value.
    @param toStop The to ending range value.
*/
float mapValue(float value, float fromStart, float fromStop, float toStart, float toStop)
{
    return toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));
}

/*
    Converts the given color in pixel units to a color with
    NDC(Normalized Device Coordinate) units.
*/
vec4 toNDCColor(vec4 pixelColor)
{
    float red = mapValue(pixelColor.r, 0.0, 255.0, 0.0, 1.0);
    float green = mapValue(pixelColor.g, 0.0, 255.0, 0.0, 1.0);
    float blue = mapValue(pixelColor.b, 0.0, 255.0, 0.0, 1.0);
    float alpha = mapValue(pixelColor.a, 0.0, 255.0, 0.0, 1.0);

    return vec4(red, green, blue, alpha);
}

void main()
{
    // Set the width and height with a limit of 0.0
    vec4 shape = pass_shape;
    shape.z = pass_shape.z < 0.0 ? 0.0 : pass_shape.z;
    shape.w = pass_shape.w < 0.0 ? 0.0 : pass_shape.w;

    float halfWidth = shape.z / 2.0;
    float halfHeight = shape.w / 2.0;
    bool isFilled = pass_isFilled > 0.0;

    // Clamp the corner radius
    borderThickness = clamp(pass_borderThickness, 1.0, halfWidth <= halfHeight ? halfWidth : halfHeight);

    vec4 ndcColor = toNDCColor(pass_color);

    Ellipse outerEllipse;
    outerEllipse.Position = shape.xy;
    outerEllipse.RadiusX = shape.z;
    outerEllipse.RadiusY = shape.w;

    bool isContainedByOuterEllipse = containedByEllipse(outerEllipse);

    if (isFilled)
    {
        finalColor = isContainedByOuterEllipse
            ? ndcColor
            : vec4(0.0);
    }
    else
    {
        Ellipse innerEllipse;
        innerEllipse.Position = shape.xy;
        innerEllipse.RadiusX = shape.z - (borderThickness * 2.0);
        innerEllipse.RadiusY = shape.w - (borderThickness * 2.0);

        bool isNotContainedByInnerEllipse = isFilled ? true : !containedByEllipse(innerEllipse);

        finalColor = isContainedByOuterEllipse && isNotContainedByInnerEllipse
            ? ndcColor
            : vec4(0.0);
    }
}
