using System;
using System.Diagnostics;

namespace FileInfoDb
{
    class ExecutionTimeLogger : IDisposable
    {
        readonly Stopwatch m_Stopwatch;


        public ExecutionTimeLogger()
        {
            m_Stopwatch = new Stopwatch();
            m_Stopwatch.Start();
        }


        public void Dispose()
        {
            m_Stopwatch.Stop();
            Console.WriteLine($"Completed, Elapsed time {m_Stopwatch.Elapsed}");
        }
    }
}
