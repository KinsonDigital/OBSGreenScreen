using System;
using System.Collections.ObjectModel;

namespace OBSGreenScreen
{
    /// <summary>
    /// Manages the process of batching textures together when rendering them.
    /// </summary>
    internal interface IBatchManagerService<T>
    {
        /// <summary>
        /// Occurs when a batch is ready to be rendered.
        /// </summary>
        /// <remarks>
        /// Scenarios When The Batch Is Ready:
        /// <para>
        ///     1. The batch is ready when draw calls switch to another circle
        /// </para>
        /// <para>
        ///     2. The batch is ready when the total amount of render calls have
        ///     reached the <see cref="BatchSize"/>.
        /// </para>
        /// </remarks>
        event EventHandler<EventArgs>? BatchFilled;

        /// <summary>
        /// Gets or sets the size of the batch.
        /// </summary>
        uint BatchSize { get; set; }

        /// <summary>
        /// Gets the list of batch items.
        /// </summary>
        /// <remarks>
        ///     Represents a list of items that are ready or not ready to be rendered.
        /// </remarks>
        ReadOnlyDictionary<uint, (bool shouldRender, T item)> AllBatchItems { get; }

        /// <summary>
        /// Gets the total number of batch items that are ready for rendering.
        /// </summary>
        uint TotalItemsToRender { get; }

        /// <summary>
        /// Gets a value indicating whether the entire batch is ready for rendering.
        /// </summary>
        /// <returns>True if every batch item is ready.</returns>
        bool BatchEmpty { get; }

        ReadOnlyCollection<(uint batchIndex, T item)> RenderableItems { get;  }

        /// <summary>
        /// Updates the batch using the given <paramref name="circle"/> and other parameters about
        /// where to render a section of the circle to the screen with a given <paramref name="size"/>,
        /// <paramref name="angle"/>, and <paramref name="tintColor"/>.
        /// </summary>
        /// <param name="circle">The circle to add to the batch.</param>
        /// <param name="srcRect">The rectangle of the circle area to render.</param>
        /// <param name="destRect">The rectangle of the destination of where to render the circle.</param>
        /// <param name="size">The size of the circle.</param>
        /// <param name="angle">The angle of the circle.</param>
        /// <param name="tintColor">The color to apply to the circle.</param>
        /// <param name="effects">The effects to apply to the circle.</param>
        // void UpdateBatch(Ellipse circle, Rectangle srcRect, Rectangle destRect, float size, float angle, Color tintColor, RenderEffects effects);

        void Add(T rect);

        /// <summary>
        /// Empties the entire batch.
        /// </summary>
        void EmptyBatch();
    }
}
