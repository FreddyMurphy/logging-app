## Work outcome

The code has been refactored into multiple smaller functions instead of all the logic being in the main loop.

A `ITimeService` has been implemented to handle all DateTime operations, this also allows for the `FakeTimeService` to be used in testing.

The `AsyncLogInterface` and `LogInterface` has been renamed to `IAsyncLogger` and `AsyncLogger` to follow Microsoft best practise.

Unit tests have been implemented to test the desired behaviour

## Future work
If I had more time I would abstract the file writing into a `IFileWriter`, so it would be possible to create a test implementation like I did with `FakeTimeService`. This test implementation shouldn't save the files locally, but perhaps rather save them in a dict with the key being the file name and the value being the text in the file. This would also save me from being worried that the different tests are writting to the same file, as each test would have its own `IFileWriter`.

I would also look into not having to use Thread.Sleep() in the unit tests, as waiting a certain amount of time could waste time. Here I can see that `ManualResetEventSlim` is something which might be worth looking into.

Finally I would also make sure that the solution had proper CI/CD through Github Actions or Azure DevOps pipeline, to make sure that the tests are passing before I can merge to main.

