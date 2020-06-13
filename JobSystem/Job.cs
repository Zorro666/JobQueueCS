using System;

namespace JobSystem
{
    public struct Job
    {
        public UInt64 Handle;
        public bool Completed { get; set; }
    };
}
