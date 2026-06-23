using BenchmarkDotNet.Running;

// Entry point: hands our benchmark class to BenchmarkDotNet's runner.
BenchmarkRunner.Run<MatchingEngineBenchmarks>();
