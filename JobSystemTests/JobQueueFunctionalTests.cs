using Xunit;
using JobSystem;

namespace JobQueueCS
{
    public class JobQueueFunctionalTests
    {
        [Fact]
        public void Schedule_SetsCompletedToFalse()
        {
            var queue = new JobQueue();
            var job = queue.Schedule();
            Assert.False(job.Completed);
        }

        [Fact]
        public void Completed_SetsCompletedToTrue()
        {
            var queue = new JobQueue();
            var job = queue.Schedule();
            queue.Complete(ref job);
            Assert.True(job.Completed);
        }
    }
}
