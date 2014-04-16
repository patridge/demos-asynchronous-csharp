[Asynchronous Code Wrangling](https://speakerdeck.com/patridge/async-code-wrangling), Demo Code
======================

While the talk was mostly live-coded, and will have some differences from what was presented, this repo covers those same asynchronous concepts.

*[Slides](https://speakerdeck.com/patridge/async-code-wrangling)*

## Demo 1: Getting your code to be asynchronous

1. Locking up the UI thread with a long-running synchronous operation.
2. [fail1] Wrapping with a `Task.Run` is good, but modifying the UI from outside the UI thread will blow up.
3. [fail2] Not waiting on the task result will produce inconsistencies.
4. [fail3] Coming back to the UI thread too soon in a task will result still lock it up.
5. [success1] Return to UI thread after long-running work will work out.
6. [success2] Return to UI thread in task continuation will work out.
7. [success3] Return to prior synchronization context (UI thread, in this case) will work out.
8. Locking up the UI thread allows for traditional try/catch.
9. Traditional try/catch will not catch errors inside your tasks. (As demoed, not in code.)
10. Not handling errors properly 
11. [success1] Using `Task.IsFaulted` and `Task.Exception` gets the tasks `AggregateException`.
12. [success2] Adding a task continuation that only runs when the task fails.

## Demo 2: Incorporating `async`/`await`

1. Clearing out the task overhead by switching to `async`/`await`.
2. Using `try`/`catch` with `async`/`await` keeps things really clean.

## Bonus Demo 1: `async`/`await` the user interaction (AsyncUser project)

This was a complex example of working with tasks and `async`/`await` together to make a fun keypad system. Entering the right combination will "unlock" the keypad.

1. [Close] Creates a chain of `await`ed tasks based on the combination, but can be brute-forced because incorrect combination progression doesn't reset the chain.
2. [Better] Properly handles resetting for invalid combinations at the sake of a few more lines of event-handling code.

*[Note: the bug that was present in the live demo, where an incorrect last digit still unlocked the keypad, has been [fixed](https://github.com/patridge/demos-asynchronous-csharp/commit/8b38b63122b875d5890db399e6c9875963710fe0).]

## Bonus Demo 2: Interacting with legacy code asynchronously (back in AsyncWpf project)

1. Task helper method for legacy `Begin*`/`End*` code.
2. Using `TaskCompletionSource` to work with old callback-based code.
