using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Mesmer.Tests
{
    [TestFixture]
    public class MesmerTests
    {
        [Test]
        public void FibonacciExampleMemoised()
        {
            Func<int, long> f = Calculator.Fibonacci;
            var memoisedFibonacci = f.Memoise();

            var memoisedResults = Run100Times(memoisedFibonacci);
            var nonMemoisedResults = Run100Times(Calculator.Fibonacci);

            Console.WriteLine("memoisedDuration Took: " + memoisedResults.Duration);
            Console.WriteLine("nonMemoisedDuration Took: " + nonMemoisedResults.Duration);
            Assert.Less(memoisedResults.Duration, nonMemoisedResults.Duration);

            CollectionAssert.AreEqual(memoisedResults.Results, nonMemoisedResults.Results);
        }

        [Test]
        public void CanNotCaterforStatefulFunctions()
        {
            var obj1 = new StatefulObject();
            var obj2 = new StatefulObject();
            Func<int, long> f = obj1.AddNumber;
            var memoisedObjectMethod = f.Memoise();

            var memoisedResults = Run100Times(memoisedObjectMethod);
            var nonMemoisedResults = Run100Times(obj2.AddNumber);

            Console.WriteLine("memoisedDuration Took: " + memoisedResults.Duration);
            Console.WriteLine("nonMemoisedDuration Took: " + nonMemoisedResults.Duration);
            Assert.Less(memoisedResults.Duration, nonMemoisedResults.Duration);
            //they wont be equal because we are inhibiting state changes
            CollectionAssert.AreNotEqual(memoisedResults.Results, nonMemoisedResults.Results);
        }

        private static TimeTrialResults Run100Times(Func<int, long> action)
        {
            var results = new TimeTrialResults();
            var watch = new Stopwatch();
            watch.Start();

            results.Results = Enumerable.Repeat(Enumerable.Range(30, 10), 10)
                .SelectMany(x => x)
                .Select(x => new CalcResult { Input = x, Result = action(x) })
                .ToList();

            watch.Stop();
            results.Duration = watch.Elapsed;
            return results;
        }
    }
}
