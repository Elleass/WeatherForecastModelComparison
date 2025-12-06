using Xunit;

// Disable parallel tests to avoid potential issues with LocalDB or shared resources.
// This file requires the xunit package to be restored.
[assembly: CollectionBehavior(DisableTestParallelization = true)]