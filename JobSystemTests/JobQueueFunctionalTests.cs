using Xunit;
using JobSystem;

using System;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests : IDisposable
    {
        JobQueue queue;
        TestJob testJob;
        Job jobTestJob;

        public JobQueueFunctionalTests()
        {
            queue = new JobQueue();
            testJob = new TestJob();
            jobTestJob = null;
        }

        public void Dispose()
        {
            if (jobTestJob != null)
            {
                if (jobTestJob.Completed == false)
                {
                    queue.Complete(ref jobTestJob);
                }
                Assert.True(jobTestJob.Completed);
            }
        }

        class TestJob : IParallelFor
        {
            public int Counter;
            public int CounterInPre;
            public int CounterInMain;
            public int CounterInPost;
            public TestJob ParentTestJob;
            public bool Completed;
            public bool ParentWasCompleted;

            public TestJob()
            {
                Counter = 0;
                CounterInPre = int.MinValue;
                CounterInMain = int.MinValue;
                CounterInPost = int.MinValue;
                ParentTestJob = null;
                ParentWasCompleted = false;
                Completed = false;
            }

            public void Pre()
            {
                ParentWasCompleted = ParentTestJob == null || ParentTestJob.Completed;
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
                Completed = true;
            }
        };

        [Fact]
        public void Schedule_SetsCompletedToFalse()
        {
            jobTestJob = queue.Schedule(testJob, 1);
            Assert.False(jobTestJob.Completed);
        }

        [Fact]
        public void Completed_SetsCompletedToTrue()
        {
            jobTestJob = queue.Schedule(testJob, 1);
            queue.Complete(ref jobTestJob);
            Assert.True(jobTestJob.Completed);
        }

        [Fact]
        public void PreIsExecutedBeforeMain()
        {
            jobTestJob = queue.Schedule(testJob, 1);
            queue.Complete(ref jobTestJob);
            Assert.Equal(0, testJob.CounterInPre);
            Assert.Equal(testJob.CounterInPre + 1, testJob.CounterInMain);
        }

        [Fact]
        public void MainIsExecutedBeforePost()
        {
            jobTestJob = queue.Schedule(testJob, 1);
            queue.Complete(ref jobTestJob);
            Assert.Equal(testJob.CounterInMain + 1, testJob.CounterInPost);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1024)]
        public void MainIsExecutedCountTimes(uint count)
        {
            jobTestJob = queue.Schedule(testJob, count);
            queue.Complete(ref jobTestJob);
            var expectedCounter = (int)count;
            Assert.Equal(expectedCounter, testJob.CounterInMain);
        }

        [Fact]
        public void ParentDependencyIsCompletedBeforeChild()
        {
            var parentTestJob = new TestJob();
            testJob.ParentTestJob = parentTestJob;
            var jobParent = queue.Schedule(parentTestJob, 1);
            jobTestJob = queue.Schedule(testJob, 1, jobParent);
            queue.Complete(ref jobTestJob);
            Assert.True(testJob.ParentWasCompleted);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(1024)]
        public void MultipleParentDependencyIsCompletedBeforeChild(uint multipleParentsCount)
        {
            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(1024)]
        public void ParentDependencyChainIsCompletedBeforeChild(uint parentChainDepth)
        {
            Job jobParent = null;
            for (var i = 0; i < parentChainDepth; ++i)
            {
                var parentTestJob = new TestJob();
                testJob.ParentTestJob = parentTestJob;
                jobParent = queue.Schedule(parentTestJob, 1);
            }
            Assert.NotNull(jobParent);
            jobTestJob = queue.Schedule(testJob, 1, jobParent);
            queue.Complete(ref jobTestJob);
            Assert.True(testJob.ParentWasCompleted);
        }
    }
}

