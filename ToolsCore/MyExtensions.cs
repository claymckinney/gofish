using System;
using System.Collections.Generic;

namespace ToolsCore
{
    public static class MyExtensions
    {

        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            // https://stackoverflow.com/questions/273313/randomize-a-listt
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
