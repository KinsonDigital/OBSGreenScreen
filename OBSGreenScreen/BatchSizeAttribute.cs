using System;

namespace OBSGreenScreen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpriteBatchSizeAttribute : Attribute
    {
        public SpriteBatchSizeAttribute(uint batchSize)
        {
            BatchSize = batchSize;
        }

        public uint BatchSize { get; }
    }
}
