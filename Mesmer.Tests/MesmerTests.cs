using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Mesmer.Tests
{

    [TestFixture]
    public class MesmerTests
    {

        static int Fibonacci(int n)
        {
            if (n == 0) return 0;
            if (n == 1) return 1;

            return Fibonacci(n - 1) + Fibonacci(n - 2);
        }

        [Test]
        public void FibonacciExampleMemoised()
        {

            var watch = new Stopwatch();
            watch.Start();

            Func<int, int> f = Fibonacci;
            var m = f.Memoise();

            int result = 0;

            Enumerable.Range(0,1000)
                      .ToList()
                      .ForEach(_ => result = m(40));
            
            watch.Stop();

            Console.WriteLine("Result: " + result + " Took " + watch.ElapsedMilliseconds + " milliseconds");
        }

        [Test]
        public void FibonacciExampleNonMemoised()
        {

            var watch = new Stopwatch();
            watch.Start();

            Func<int, int> f = Fibonacci;
            int result = 0;
            Enumerable.Range(0, 10)
                      .ToList()
                      .ForEach(_ => result = f(40));

            watch.Stop();

            Console.WriteLine("Result: " + result + " Took " + watch.ElapsedMilliseconds + " milliseconds");
        }
    }
}
