using System;

namespace JobSystem
{
    public struct Job
    {
        public UInt64 ParentHandle { get; set; }
        public Job[] Parents { get; set; }
        public UInt64 Handle { get; set; }
        public uint MaxCount { get; set; }
        public bool Completed { get; set; }

        public JobQueueHelper.IParallelFor JobStruct;
    };
}
