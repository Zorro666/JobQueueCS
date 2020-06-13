namespace JobSystem
{
    public class Job
    {
        public Job Parent;
        public Job[] Parents { get; set; }
        public ulong Handle { get; set; }
        public uint MaxCount { get; set; }
        public bool Completed { get; set; }

        public IParallelFor JobStruct;
    };
}
