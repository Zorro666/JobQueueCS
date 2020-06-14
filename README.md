# JobQueueCS
Playground to explore writing a JobQueue in C#

## Versions

### V0.1
Basic API with unit tests to define execpted behaviour
Synchronous implementation as a basis to define behaviour and build an asynchronous version on top of
Not happy with the "Job" class and the way it works that you need an instance of the job data and a job handle.
Be interesting to explore if a single "Job" class could be implemented which contains the job instance data and the job queue information.

## TODO
* Asynchronous implementation using locked containers
* Asynchronous implementation using lockless containers
* Performance tests
* Investigate different parallel for algorithms : single work queue, shared work queue, divide and conquer
