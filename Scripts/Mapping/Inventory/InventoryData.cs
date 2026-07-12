using System;
using System.Collections.Generic;

namespace Inventory
{
    [Serializable]
    public class InventoryData
    {
        public List<InventoryItem> items = new();
    }
}