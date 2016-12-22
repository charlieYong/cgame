using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;


namespace ZR_MasterTools
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom {
            get {
                return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
            }
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Shuffle the specified list with System.Random().
        /// </summary>
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Clear(this StringBuilder value) {
            value.Length = 0;
            // Clear memory
            value.Capacity = 0;
            // Reset Original Capacity
            value.Capacity = 16;
        }
    }
}