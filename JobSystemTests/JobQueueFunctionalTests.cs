using System;
using Xunit;
using JobSystem;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests
    {
        struct TestJob : JobQueueHelper.IParallelFor
        {
            public void Pre()
            {
                Console.WriteLine("Pre");
            }

            public void Main(int index)
            {
                Console.WriteLine($"Main[{index}]");
            }

            public void Post()
            {
                Console.WriteLine("Post");
            }
        };

        [Fact]
        public void Schedule_SetsCompletedToFalse()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            var job = queue.Schedule(testJob);
            Assert.False(job.Completed);
        }

        [Fact]
        public void Completed_SetsCompletedToTrue()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            var job = queue.Schedule(testJob);
            queue.Complete(ref job);
            Assert.True(job.Completed);
        }
    }
}
