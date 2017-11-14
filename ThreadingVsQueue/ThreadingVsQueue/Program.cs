using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingVsQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            // LET JIT do it's thing
            Console.WriteLine("Jit Stuff.");
            SqrAlot();
            SqrAlot2();
            SqrAlot3();

            RunTest("SqrAlot", SqrAlot);
            RunTest("SqrAlot2", SqrAlot2);
            RunTest("SqrAlot3", SqrAlot3);

            RunTest("SqrAlotLocking", SqrAlotLocking);
            RunTest("SqrAlot2Locking", SqrAlot2Locking);
            RunTest("SqrAlot3Locking", SqrAlot3Locking);

            RunTest("SqrAlotLocking", SqrAlotLockingInFor);
            RunTest("SqrAlot2Locking", SqrAlot2LockingInFor);
            RunTest("SqrAlot3Locking", SqrAlot3LockingInFor);

            Console.ReadLine();
        }

        private static void RunTest(string name, Action action)
        {
            Console.WriteLine("Test: " + name);
            Console.WriteLine("It took Threads with RampUp {0}ms to execute.", RunThreads(action, 100));
            Console.WriteLine("It took Queues with RampUp {0}ms to execute.", RunQueue(action, 100));
            Console.WriteLine("It took Threads without RampUp {0}ms to execute.", RunThreadsWithoutRampUp(action, 100));
            Console.WriteLine("It took Queues without RampUp {0}ms to execute.", RunQueueWithoutRampUp(action, 100));
            Console.WriteLine("done.");
        }

        private static void SqrAlot()
        {
            double result = 1;
            for (int i = 0; i < 1000; i++)
            {
                result++;
                result += Math.Sqrt(result);
            }
        }

        private static void SqrAlot2()
        {
            double result = 1;
            for (int i = 0; i < 1000000; i++)
            {
                result++;
                result += Math.Sqrt(result);
            }
        }

        private static void SqrAlot3()
        {
            double result = 1;
            for (int i = 0; i < 10000000; i++)
            {
                result++;
                result += Math.Sqrt(result);
            }
        }

        private static void SqrAlotLocking()
        {
            lock (lockObject)
            {
                double result = 1;
                for (int i = 0; i < 1000; i++)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void SqrAlot2Locking()
        {
            lock (lockObject)
            {
                double result = 1;
                for (int i = 0; i < 1000000; i++)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void SqrAlot3Locking()
        {
            lock (lockObject)
            {
                double result = 1;
                for (int i = 0; i < 10000000; i++)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void SqrAlotLockingInFor()
        {
            double result = 1;
            for (int i = 0; i < 1000; i++)
            {
                lock (lockObject)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void SqrAlot2LockingInFor()
        {
            double result = 1;
            for (int i = 0; i < 1000000; i++)
            {
                lock (lockObject)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void SqrAlot3LockingInFor()
        {
            double result = 1;
            for (int i = 0; i < 10000000; i++)
            {
                lock (lockObject)
                {
                    result++;
                    result += Math.Sqrt(result);
                }
            }
        }

        private static void QueueWorker(Queue<Action> queue)
        {
            while (queue.Count > 0 || waitForData)
            {
                if (queue.Count > 0)
                {
                    var action = queue.Dequeue();
                    action();
                }
            }
        }

        private static bool waitForData = true;
        private static object lockObject = new object();

        private static long RunQueue(Action action, int count)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            waitForData = true;
            List<Queue<Action>> queues = new List<Queue<Action>>();
            Task[] tasks = new Task[8];
            for (int i = 0; i < count; i++)
            {
                if (i < 8)
                {
                    int iClosureScoped = i;
                    queues.Add(new Queue<Action>(count));
                    tasks[i] = new Task(() =>
                    {
                        QueueWorker(queues[iClosureScoped]);
                    });
                    tasks[i].Start();
                }

                queues[i % 8].Enqueue(action);
            }

            waitForData = false;
            Task.WaitAll(tasks);

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private static long RunThreads(Action action, int count)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Task[] tasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = new Task(action);
                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private static long RunQueueWithoutRampUp(Action action, int count)
        {
            List<Queue<Action>> queues = new List<Queue<Action>>();
            Task[] tasks = new Task[8];
            for (int i = 0; i < count; i++)
            {
                if (i < 8)
                {
                    int iClosureScoped = i;
                    queues.Add(new Queue<Action>(count));
                    tasks[i] = new Task(() =>
                    {
                        QueueWorker(queues[iClosureScoped]);
                    });
                }

                queues[i % 8].Enqueue(action);
            }

            waitForData = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private static long RunThreadsWithoutRampUp(Action action, int count)
        {
            Task[] tasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = new Task(action);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                tasks[i].Start();
            }
            Task.WaitAll(tasks);

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }
    }
}
