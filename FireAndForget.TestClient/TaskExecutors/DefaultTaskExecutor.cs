using System.Threading;
using FireAndForget.Core.TaskExecutor;

namespace FireAndForget.TestClient.TaskExecutors
{
    public class DefaultTaskExecutor : ITaskExecutor
    {
        public string MessageType
        {
            get { return "DefaultTask"; }
        }

        public void Process(string data)
        {
            int fib = Fibonacci(13);
            System.Console.WriteLine("Executed {0} in Thread {1} - {2}", this.GetType().Name, Thread.CurrentThread.ManagedThreadId, fib); 
        }

        private int Fibonacci(int number)
        {
            if (number == 0)
                return 0;
            else if (number == 1)
                return 1;
            else
            {
                return Fibonacci(number - 2) + Fibonacci(number - 1);
            }
        }
    }
}
