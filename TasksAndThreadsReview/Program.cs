/*
This project is a review of tasks and threads in C#.
If your code implements I/O-bound scenarios to support network data requests, database access, or file system read/writes, 
asynchronous programming is the best approach. You can also write asynchronous code for CPU-bound scenarios like expensive calculations.

IO bound and CPU bound scenarios can leverage this.
*/

namespace TasksAndThreadsReview
{
    class Program
    {
        static void Main(string[] args)
        {
            // example of asynchronous programming
            string data = await GetDataAsync();
            Console.WriteLine(data);

            // example of a background thread
            // (once the foreground thread is killed, the background thread will also be killed)
            Thread backgroundThread = new Thread(BackgroundTask);
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        /// <summary>
        /// This is an example of an asynchronous method that returns a string. This is the basic pattern. the line that is awaited will
        /// free up the thread and allow the method to continue executing.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetDataAsync()
        {
            return await Task.FromResult("Hello, World!");
        }

        public void BackgroundTask()
        {
            while(true)
            {
                Console.WriteLine("Background task is running");
                Thread.Sleep(1000);
            }
        }
    }
}