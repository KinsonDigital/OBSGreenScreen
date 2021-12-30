using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OBSGreenScreen
{
    public class RectangleBatchService : IBatchManagerService<Rectangle>
    {
        private readonly Dictionary<uint, (bool shouldRender, Rectangle rect)> _batchItems = new ();
        private uint _currentBatchItem;
        private uint _batchSize;

        public event EventHandler<EventArgs>? BatchFilled;

        public uint BatchSize
        {
            get => _batchSize;
            set
            {
                _batchSize = value;
                for (var i = 0u; i < _batchSize; i++)
                {
                    _batchItems.Add(i, (false, default));
                }
            }
        }

        public ReadOnlyDictionary<uint, (bool shouldRender, Rectangle item)> AllBatchItems => new (_batchItems);

        public ReadOnlyCollection<(uint batchIndex, Rectangle item)> RenderableItems
        {
            get
            {
                var foundItems = _batchItems.Where(i => i.Value.shouldRender)
                    .Select(i => (i.Key, i.Value.rect)).ToArray();

                return new ReadOnlyCollection<(uint, Rectangle)>(foundItems);
            }
        }

        public uint TotalItemsToRender => (uint)_batchItems.Count(i => i.Value.shouldRender);

        public bool BatchEmpty => _batchItems.All(i => i.Value.rect.IsEmpty());

        public void Add(Rectangle rect)
        {
            _batchItems[_currentBatchItem] = (true, rect);
            _currentBatchItem += 1;

            if (_currentBatchItem < BatchSize)
            {
                return;
            }

            BatchFilled?.Invoke(this, EventArgs.Empty);

            EmptyBatch();
        }

        public void EmptyBatch()
        {
            for (var i = 0u; i < _batchItems.Count; i--)
            {
                if (_batchItems[i].shouldRender is false)
                {
                    continue;
                }

                var itemToEmpty = _batchItems[i];
                itemToEmpty.shouldRender = false;
                itemToEmpty.rect.Empty();

                _batchItems[i] = itemToEmpty;
            }

            _currentBatchItem = 0;
        }
    }
}
