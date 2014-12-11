namespace Mesmer.Tests
{
    public class StatefulObject
    {
        public StatefulObject()
        {
            CurrentState = 0;
        }
        public long CurrentState { get; set; }

        public long AddNumber(int number)
        {
            return CurrentState += Calculator.Fibonacci(number);
        }
    }
}