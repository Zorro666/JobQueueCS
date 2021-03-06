﻿using System;

namespace JobSystem
{
    public interface IParallelFor
    {
        void Pre();
        void Main(int index);
        void Post();
    };

    public class JobQueue
    {
        public JobQueue()
        {
            FreeHandle = 0;
            NoParent = new Job
            {
                Parent = null,
                Parents = null,
                Handle = AllocateHandle(),
                Completed = true,
                MaxCount = 0
            };
        }

        public Job Schedule<T>(T jobStruct, in uint maxCount) where T : class, IParallelFor
        {
            return ScheduleInternal(jobStruct, maxCount, NoParent, null);
        }

        public Job Schedule<T>(T jobStruct, in uint maxCount, in Job parent) where T : class, IParallelFor
        {
            return ScheduleInternal(jobStruct, maxCount, parent, null);
        }

        public Job Schedule<T>(T jobStruct, in uint maxCount, in Job[] parents) where T : class, IParallelFor
        {
            return ScheduleInternal(jobStruct, maxCount, NoParent, parents);
        }

        public void Complete(ref Job job)
        {
            if (job.Completed == false)
            {
                Execute(ref job);
            }
        }

        private void Execute(ref Job job)
        {
            if (job.Parents != null)
            {
                var parentsCount = job.Parents.Length;
                for (var p = 0; p < parentsCount; ++p)
                {
                    ref var parent = ref job.Parents[p];
                    Execute(ref parent);
                }
            }
            else if (job.Parent?.Completed == false)
            {
                ref var parent = ref job.Parent;
                if (parent.Completed == false)
                {
                    Execute(ref parent);
                }
            }

            job.JobStruct.Pre();
            for (var i = 0; i < job.MaxCount; ++i)
            {
                job.JobStruct.Main(i);
            }
            job.JobStruct.Post();
            job.Completed = true;
        }

        private Job ScheduleInternal<T>(T jobStruct, in uint maxCount, in Job parent, in Job[] parents) where T : class, IParallelFor
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if ((parents != null) && (parent.Handle != NoParent.Handle))
            {
                throw new ArgumentException($"Specifying Single Parent and Multiple Parents is not specified");
            }

            var job = new Job
            {
                Parent = parent,
                Parents = parents,
                Handle = AllocateHandle(),
                Completed = false,
                MaxCount = maxCount
            };
            job.JobStruct = jobStruct;
            return job;
        }

        private ulong AllocateHandle()
        {
            var handle = FreeHandle;
            ++FreeHandle;
            return handle;
        }

        private readonly Job NoParent;
        private ulong FreeHandle;

    };
}
