using System;

namespace OBSGreenScreen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GPUBufferNameAttribute : Attribute
    {
        public GPUBufferNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
