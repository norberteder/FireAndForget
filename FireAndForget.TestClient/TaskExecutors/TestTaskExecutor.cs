using System.Threading;
using FireAndForget.Core.TaskExecutor;

namespace FireAndForget.TestClient.TaskExecutors
{
    public class TestTaskExecutor : ITaskExecutor
    {
        public string MessageType
        {
            get { return "TestTask"; }
        }

        public void Process(string data)
        {
            Fibonacci(20);
            System.Console.WriteLine("Executed {0} in Thread {1} - {2}", this.GetType().Name, Thread.CurrentThread.ManagedThreadId, data); 
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
