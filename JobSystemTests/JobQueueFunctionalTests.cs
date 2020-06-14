using Xunit;
using JobSystem;

using System;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests : IDisposable
    {
        public readonly JobQueue queue;
        public readonly TestJob testJob;
        public Job jobTestJob;

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

        public class TestJob : IParallelFor
        {
            public int Counter;
            public int CounterInPre;
            public int CounterInMain;
            public int CounterInPost;
            public int Completed;

            public TestJob[] Parents;
            public int ParentCount;
            public int ParentCompletedCount;

            public TestJob()
            {
                Counter = 0;
                CounterInPre = int.MinValue;
                CounterInMain = int.MinValue;
                CounterInPost = int.MinValue;
                Completed = 0;

                Parents = null;
                ParentCount = 0;
                ParentCompletedCount = 0;
            }

            public void Pre()
            {
                Completed = 0;
                ParentCompletedCount = 0;
                for (var i = 0; i < ParentCount; ++i)
                {
                    ParentCompletedCount += Parents[i].Completed;
                }

                CounterInPre = Counter;
                ++Counter;
            }

            public void Main(int index)
            {
                Completed = 0;
                CounterInMain = Counter;
                ++Counter;
            }

            public void Post()
            {
                CounterInPost = Counter;
                ++Counter;
                Completed = 1;
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
            const int parentsCount = 1;
            var parentTestJob = new TestJob();

            testJob.Parents = new TestJob[parentsCount] { parentTestJob };
            testJob.ParentCount = parentsCount;

            var jobParent = queue.Schedule(parentTestJob, 1);
            jobTestJob = queue.Schedule(testJob, 1, jobParent);

            queue.Complete(ref jobTestJob);
            Assert.Equal(parentsCount, testJob.ParentCompletedCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(1024)]
        [InlineData(1024 * 1024)]
        public void MultipleParentDependencyIsCompletedBeforeChild(int multipleParentsCount)
        {
            Job[] jobParents = new Job[multipleParentsCount];
            testJob.Parents = new TestJob[multipleParentsCount];
            testJob.ParentCount = multipleParentsCount;
            for (var i = 0; i < multipleParentsCount; ++i)
            {
                var parentTestJob = new TestJob();
                testJob.Parents[i] = parentTestJob;
                jobParents[i] = queue.Schedule(parentTestJob, 1);
            }

            jobTestJob = queue.Schedule(testJob, 1, jobParents);
            queue.Complete(ref jobTestJob);

            Assert.Equal(multipleParentsCount, testJob.ParentCompletedCount);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(1024)]
        public void ParentDependencyChainIsCompletedBeforeChild(uint parentChainDepth)
        {
            const int parentsCount = 1;
            Job jobParent = null;
            TestJob parentTestJob = null;
            TestJob thisJob;
            for (var i = 0; i < parentChainDepth; ++i)
            {
                thisJob = new TestJob();
                if (parentTestJob != null)
                {
                    Assert.NotNull(jobParent);
                    thisJob.Parents = new TestJob[parentsCount] { parentTestJob };
                    thisJob.ParentCount = parentsCount;
                    jobParent = queue.Schedule(thisJob, 1, jobParent);
                }
                else
                {
                    Assert.Null(jobParent);
                    thisJob.Parents = null;
                    thisJob.ParentCount = 0;
                    jobParent = queue.Schedule(thisJob, 1);
                }
                parentTestJob = thisJob;
            }
            Assert.NotNull(parentTestJob);
            Assert.NotNull(jobParent);

            testJob.Parents = new TestJob[1] { parentTestJob };
            testJob.ParentCount = parentsCount;

            jobTestJob = queue.Schedule(testJob, 1, jobParent);
            queue.Complete(ref jobTestJob);

            thisJob = testJob;
            for (var i = 0; i < parentChainDepth; ++i)
            {
                var parent = thisJob.Parents[0];
                if (parent.ParentCount == 1)
                {
                    Assert.Equal(parentsCount, parent.ParentCompletedCount);
                }
                else
                {
                    Assert.Equal(0, parent.ParentCompletedCount);
                }
                thisJob = parent;
            }
        }
    }
}

