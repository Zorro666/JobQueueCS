using System;

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
        public JobQueue()
        {
            FreeHandle = 0;
            NoParent = new Job
            {
                ParentHandle = AllocateHandle(),
                Parents = null,
                Handle = 0,
                Completed = true,
                MaxCount = 0
            };
        }

        private Job ScheduleInternal<T>(in T jobStruct, in uint maxCount, in Job parent, in Job[] parents) where T : struct, JobQueueHelper.IParallelFor
        {
            if ((parents != null) && (parent.Handle != NoParent.Handle))
            {
                throw new ArgumentException($"Specifying Single Parent and Multiple Parents is not specified");
            }

            var job = new Job
            {
                ParentHandle = parent.Handle,
                Parents = parents,
                Handle = AllocateHandle(),
                Completed = false,
                MaxCount = maxCount
            };
            job.JobStruct = jobStruct;
            return job;
        }

        public Job Schedule<T>(in T jobStruct, in uint maxCount) where T : struct, JobQueueHelper.IParallelFor
        {
            return ScheduleInternal(jobStruct, maxCount, NoParent, null);
        }

        public Job Schedule<T>(in T jobStruct, in uint maxCount, in Job parent) where T : struct, JobQueueHelper.IParallelFor
        {
            return ScheduleInternal(jobStruct, maxCount, parent, null);
        }

        public void Complete(ref Job job)
        {
            Console.WriteLine($"ParentHandle {job.ParentHandle} Handle {job.Handle}");
            job.JobStruct.Pre();
            for (var i = 0; i < job.MaxCount; ++i)
            {
                job.JobStruct.Main(i);
            }
            job.JobStruct.Post();
            job.Completed = true;
        }

        private Job NoParent;
        private UInt64 FreeHandle;

        private UInt64 AllocateHandle()
        {
            var handle = FreeHandle;
            ++FreeHandle;
            return handle;
        }
    };
}
