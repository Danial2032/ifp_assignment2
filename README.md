# ifp_assignment2

# Alibayev Danial, IT-2504

Build,Run and Test Instructions:

Copy this in terminal in the root directory: 

dotnet build

dotnet test --logger "console;verbosity=detailed"

Brief Architecture and Component breakdown:

The code is structured into two main projects: TelecomCdr and TelecomCDR_Tests. CallRecord is pure component because it uses readonly record struct representing an immutable call event. CallPricing is pure component because it is static class with side-effect free methods. CdrProcessor is impure component because it manages task partitioning.

Partitioning Rationale:

To achieve Lock-Free Parallelism the array of call records is split evenly into distinct contiguous chinks using C# range slicing syntax (records .. mid and records mid..).

Each worker thread operates on its slice and writes results directly into a pre-allocated segment of the result array (decima []).

Test Results:

Total Executed Tests: 25

Status: All passed.

Limitations: 

Fixed Thread Count: Partitioning logic assumes balanced binary splits. It is possible to use only 2 because mid has only ..mid and mid..

Memory Footprint: Results are using in-memory array , which means that it needs sufficient RAM for large datasets.

Task 3: Race Condition Explanation:

The statement globalCallCounter++ is unsafe in multi-threaded code because it is not an atomic operation. It can lead to a lost update.
