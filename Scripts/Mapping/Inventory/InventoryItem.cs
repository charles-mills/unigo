using System;

namespace Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public string name;
        public string code;
        public long capturedAtUnixSeconds;
        public long expiresAtUnixSeconds;

        /// <summary>
        ///     InventoryItem type, used for brand discount codes.
        /// </summary>
        /// <param name="name">Name of code or brand</param>
        /// <param name="code">The code string</param>
        /// <param name="capturedAtUnixSeconds">When the code was captured, in UTC unix seconds</param>
        /// <param name="expiresAtUnixSeconds">When the code expires, in UTC unix seconds</param>
        public InventoryItem(string name, string code, long capturedAtUnixSeconds, long expiresAtUnixSeconds)
        {
            this.name = name;
            this.code = code;
            this.capturedAtUnixSeconds = capturedAtUnixSeconds;
            this.expiresAtUnixSeconds = expiresAtUnixSeconds;
        }

        public bool IsExpired(DateTimeOffset nowUtc)
        {
            return expiresAtUnixSeconds > 0 && nowUtc.ToUnixTimeSeconds() >= expiresAtUnixSeconds;
        }
    }
}