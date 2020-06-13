namespace JobSystem
{
    public struct JobQueue
    {
        public Job Schedule()
        {
            var job = new Job
            {
                Completed = false
            };
            return job;
        }

        public void Complete(ref Job job)
        {
            job.Completed = true;
        }
    }
}
