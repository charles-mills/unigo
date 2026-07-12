using System.Linq;
using UnityEngine;

namespace APIs_Helpers
{
    public class GetRandomCode
    {
        private static readonly string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";


        /// <summary>
        ///     Returns a random string of a given length (Default 10)
        /// </summary>
        /// <remarks>
        ///     Adapted from https://stackoverflow.com/questions/65781121/unity-random-characters-in-string
        ///     Answer by user: Lexicon
        /// </remarks>
        public static string GetRandom(int length = 10)
        {
            return string.Join(
                "",
                Enumerable
                    .Range(0, length)
                    .Select(k => chars[Random.Range(0, chars.Length)])
            );
        }
    }
}