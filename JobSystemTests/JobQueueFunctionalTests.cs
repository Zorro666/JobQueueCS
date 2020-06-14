using Xunit;
using JobSystem;

using System;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests : IDisposable
    {
        JobQueue queue;
        TestJob testJob;
        Job job;

        public JobQueueFunctionalTests()
        {
            queue = new JobQueue();
            testJob = new TestJob();
            job = null;
        }

        public void Dispose()
        {
            if (job != null)
            {
                if (job.Completed == false)
                {
                    queue.Complete(ref job);
                }
                Assert.True(job.Completed);
            }
        }

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
            job = queue.Schedule(testJob, 1);
            Assert.False(job.Completed);
        }

        [Fact]
        public void Completed_SetsCompletedToTrue()
        {
            job = queue.Schedule(testJob, 1);
            queue.Complete(ref job);
            Assert.True(job.Completed);
        }

        [Fact]
        public void PreIsExecutedBeforeMain()
        {
            job = queue.Schedule(testJob, 1);
            queue.Complete(ref job);
            Assert.Equal(0, testJob.CounterInPre);
            Assert.Equal(testJob.CounterInPre + 1, testJob.CounterInMain);
        }

        [Fact]
        public void MainIsExecutedBeforePost()
        {
            job = queue.Schedule(testJob, 1);
            queue.Complete(ref job);
            Assert.Equal(testJob.CounterInMain + 1, testJob.CounterInPost);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1024)]
        public void MainIsExecutedCountTimes(uint count)
        {
            job = queue.Schedule(testJob, count);
            queue.Complete(ref job);
            var expectedCounter = (int)count;
            Assert.Equal(expectedCounter, testJob.CounterInMain);
        }

        // Parent is run before child
        // N Parents are run before child
        [Fact]
        public void Jake()
        {
        }
    }
}

