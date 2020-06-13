namespace JobSystem
{
    public static class JobQueueHelper
    {
        public interface IParallelFor
        {
            void Pre();
            void Main(int index);
            void Post();
        };
    };

    public class JobQueue
    {
        public Job Schedule<T>(T jobStruct) where T : struct, JobQueueHelper.IParallelFor
        {
            var job = new Job
            {
                Completed = false
            };
            job.JobStruct = jobStruct;
            return job;
        }
        /*
        public Job Schedule<T>(Pre pre, Main main, Post post, T jobStruct) where T : struct, JobQueueHelper.IParallelFor
        {
            var job = new Job
            {
                Completed = false
            };
            return job;
        }
        */

        public void Complete(ref Job job)
        {
            job.JobStruct.Pre();
            job.JobStruct.Main(0);
            job.JobStruct.Post();
            job.Completed = true;
        }
    }
}
