using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncWpf {
    /// <summary>
    /// A demo service that has some delays to make async work visible.
    /// Async versions are really just wrappers for async/await use.
    /// Not much to see here.
    /// </summary>
    public static class SlowService {
        static int[] numbers = Enumerable.Range(1, 10).ToArray();
        public static int GetNextNumber() {
            lock(numbersLock) {
                int next = numbers[nextNumberIndex];
                nextNumberIndex += 1;
                if (nextNumberIndex >= numbers.Length) {
                    nextNumberIndex = 0;
                }
                Thread.Sleep(rand.Next(5, 35) * 100);
                return next;
            }
        }
        public static Task<int> GetNextNumberAsync() {
            return Task.Run(() => {
                return GetNextNumber();
            });
        }
        public static int Fail() {
            Thread.Sleep(rand.Next(5, 35) * 100);
            throw new NotImplementedException("FAIL!");
        }
        public static Task<int> FailAsync() {
            return Task.Run(() => {
                return Fail();
            });
        }
        static object numbersLock = new object();
        static int nextNumberIndex = 0;
        static Random rand = new Random();
    }
}