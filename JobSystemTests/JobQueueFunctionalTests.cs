using Xunit;
using JobSystem;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests
    {
        class TestJob : IParallelFor
        {
            public int Counter;
            public int CounterInPre;
            public int CounterInMain;
            public int CounterInPost;

            public void Pre()
            {
                CounterInPre = Counter;
                ++Counter;
            }

            public void Main(int index)
            {
                CounterInMain = Counter;
                ++Counter;
            }

            public void Post()
            {
                CounterInPost = Counter;
                ++Counter;
            }
        };

        [Fact]
        public void Schedule_SetsCompletedToFalse()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            var job = queue.Schedule(testJob, 1);
            Assert.False(job.Completed);
        }

        [Fact]
        public void Completed_SetsCompletedToTrue()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            var job = queue.Schedule(testJob, 2);
            queue.Complete(ref job);
            Assert.True(job.Completed);
        }

        [Fact]
        public void PreIsExecutedBeforeMain()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            testJob.Counter = 0;
            var job = queue.Schedule(testJob, 2);
            queue.Complete(ref job);
            Assert.Equal(0, testJob.CounterInPre);
            Assert.True(testJob.CounterInMain > testJob.CounterInPre);
        }

        [Fact]
        public void MainIsExecutedBeforePost()
        {
            var queue = new JobQueue();
            var testJob = new TestJob();
            testJob.Counter = 0;
            var job = queue.Schedule(testJob, 2);
            queue.Complete(ref job);
            Assert.True(testJob.CounterInPost > testJob.CounterInMain);
        }

        // Parent is run before child
        // N Parents are run before child
    }
}

