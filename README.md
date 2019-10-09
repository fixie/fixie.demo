# fixie.demo

As an example of general integration testing guidance with Fixie, this is a sample Contact List application with integration tests and a customized testing convention.

## Integration Testing Principles

1. Exercise the production scenario as much as possible.
2. Develop “shorthand” test helper functions so that tests can read like a script of user interactions.

No two apps are the same, so the details will vary, but it helps to focus the discussion on several high-value specifics that we see in many projects. Specific MVC versions, specific IoC tools, specific databases will vary, but when we apply the above principles to these variations, we can still arrive at very similar patterns in our tests.

Consider a typical modern web application: ASP.NET MVC Core with its built-in IoC container, a SQL Server database, and a useful “unit of work” aligned with each web request. Each web request is set up to have a dedicated SQL transaction opened near the start of the request and committed/rolled back at the end, so that most of our code need not be concerned with transactions at all. Aligned with that transaction, we have a scoped IoC container so that each web request has its own dedicated container based on the “global” one initialized during application startup. Controllers use MediatR to `Send` `Command` and `Query` objects to their respective `Handler`s, where real work is performed. These handlers may be protected by some validation rule library like `FluentValidation`.

With that application in mind, we’ll often have test classes 1-1 with message handler classes. We won’t have automated tests of the controllers: these only really behave under the context of a real web request in a running web server. Thankfully the use of MediatR makes the controllers quite small. We’ll be satisfied with our handler test coverage so long as we do run the application and exercise the controller as part of a typical QA effort.

If our primary integration testing effort is in testing MediatR handlers, it may be tempting to write a test by explicitly constructing a MediatR handler, passing its dependencies both real and stub to the constructor, explicitly calling the `Handle` method, and asserting on the response and side effects. Doing so would be a mistake, as it violates both of our integration testing principles: it does not mimic production, and it is verbose.

We can do better:
1. It would be better if a test could simply `Send` the relevant `Command` or `Query` object just like a Controller class would, fully exercising the MediatR pipeline, just like in production.
2. It would be better if every such `Send` automatically took part in a unit of work transaction, just like in production.
3. It would be better if every such `Send` also exercised any validation rules before allowing the handler to run, just like in production.
4. It would be better if every such `Send` also executed in the context of a dedicated, scoped IoC container, just like in production.
5. It would be better if we could accomplish all of this in an exceptionally small number of characters, so that the test author can simply focus on the scenario at hand rather than any of these cross-cutting concerns.

> Implicit here is that our tests will really hit the database. "But that's not a unit test!" some may cry out, but that is not a valid argument. It's *not* a unit test, but it's not a *purple* test either and we wouldn't argue that these tests are bad because they're not purple enough. It's an integration test, and therefore tells me what's actually going to happen. Mocking out our Entity Framework interactions would make for very fast, very useless lies - not even *unit* tests, just lies.

All of these ideals can be seen in this Contact List sample application.

Applying these patterns in our integration tests results in tests that are easy to write, exercise as much of the production code as we realistically can, protect themselves from testing unrealistic or impossible scenarios, and remain meaningfully red or green throughout the life of a growing project. Armed with this style, we rarely need to reach for inheritance or test framework-specific oddities like class-level test lifecycle attributes.

## Global Test Helpers

We achieve shorthand in our tests by defining `static class Testing` which most test files will include with `using static Testing;`. All public methods in this class can therefore be called from any test, as if they were global functions.

The static constructor of `Testing` will establish some fundamentals used by the many global helper functions. It establishes the test project’s config file in exactly the same way as the running web application. Then, we set up the root IoC container that will be used across all tests. We don’t do so merely by performing similar IoC setup as the core app, but instead by calling into the same setup code exercised when the web app starts up in production. Yes, we can follow that up with a **rare** test stub or two registered with the container, overriding the production configuration, but only when the production setup poses too severe of an obstacle such as for interfaces dealing with third party APIs.

Core helpers `Transaction`, `Query`, `Count`, etc provide shorthand for database interactions, each opearting under a dedicated, short-lived `DbContext` and corresponding transaction boundary. `DbContext` objects are very easy to abuse, especially within test code, so we want tests to directly care about their lifecycles as little as possible. The `Transaction` helper method allows a test to trivially run any code within the context of a web-request-like unit of work. The supplied action runs within a dedicated transaction, just like production, and a dedicated scoped IoC container, just like production.

The `Validation` helper gives you the validation result for a given `Command` or `Query`. Combined with some custom `ShouldValidate` and `ShouldNotValidate` assertion extensions, this makes it easy to write tests of the form, "Given a form filled out like so, we expect it to fail validation with these expected error messages." Since production validation rules often need to inject dependencies of their own, and because they often need to query the database, `Validation(...)` works by establishing the now-familiar database transaction and IoC scope, resolving the validator as we would in production, and then exercising it. Tests need only say, "This form should validate or not." Some validation rule libraries urge people to write useless assertions of the form "Some validation error, any validation error, should exist" but such tests give a false sense of security. `ShouldNotValidate` on the other hand, is *complete* and *deliberately brittle*. It asserts on exactly the full set of expected error messages, because you're interested in actually testing your validation rules.

The two most powerful helpers, overloads of `Send(...)`, build atop these concepts. They ultimately `Send` a given `Command` or `Query` through the MediatR pipeline, in a unit of work transaction, in a scoped IoC container, just like production. Also just like production, validation rules are executed and must pass before we bother to send the message to its handler. This validation check protects us from writing tests that pass while testing impossible scenarios. Your test will only pass if the scenario in question is one that *a user could actually arrive at themselves*.

## Avoiding Manual Database Setup

Whenever possible, we must avoid overly-manual setup steps in our code, such as constructing and filling in a few entity classes and asking our ORM to save them all. It is far too easy to set up a scenario that is incomplete, unrealistic, or impossible to arrive at as a real user. These setup steps also tend to age poorly, such that even if they are complete and realistic when first written, they degrade to being incomplete as the surrounding system and data model grows.

Instead, whenever possible, set up the scenario under test by exercising (`Send()`ing) the same `Command` objects that the user would. The scenario mimics production, we further exercise those preliminary command handlers, avoid brittle setup, and generally simplify the test as a script of user actions.

As you start to use your own Commands for test setup, you'll find yourself introducing even more shorthand `Testing.cs` helpers for the most common operations. This builds up a Domain Specific Language, the "vocabulary" of *your* application, further simplifying the writing and reading of each new test. We don't want our tests to read like "Insert this, then insert that, then call this handler method". Instead, we want our tests to read like "Register a user account, log in as that user, upload this document, archive that document..."

## Live Testing Database

Testing with an incomplete data set is the most dangerous pitfall in persistence testing. It's very easy to miss code behaviors that are already present but not apparent because we're starting with a minimally populated database, or perhaps worse, an empty database.

Beware testing Entity Framework using an InMemoryContext. These tests won’t actually generate SQL executed against a real database, and they will suffer from the empty database problem.

Temporal coupling: tests that add their own test records and assert around this data can be prone to a situation where you accidentally rely on data added during another test running in same transaction cycle. If the order in which your tests run suddenly changes, you may see new, confusing test failures. Pay careful attention that your test cases are sufficiently isolated from data added outside of your specific test. Again, we're simulating production, which will definitely have pre-existing records as the user interacts with the system. Write tests that meaningfully pass no matter what has already been saved.

Starting with an empty database on each test is too simple compared to the DB state that your handlers will run under in production, so it's easy to fool yourself that your test coverage is right (e.g. a DELETE without a WHERE clause can easily pass a naive test). Ensure your test truly exercises the database interactions pessimisitically.

## Use separate Development and Test Databases

If your build script only sets up one local database, shared by test runs and by the running application itself, you create unnecessary obstacles. Imagine you have used the running application to set up a scenario useful to feature development: merely running the test suite could easily erase or otherwise invalidate that feature development setup effort. Having a single database for development and tests also means that while a long test suite is running, you have to wait for it to finish before you can meaningfully run the application.

When your build script sets up two databases, one dedicated to developing the application, and one dedicated to test runs, you eliminate these obstacles.

## Consider using Complete Assertions

A common complaint about tests comes when they are *brittle*. A brittle test is one that fails in response to some small change in the system under test. It can be frustrating when you arrive at a failing test only to realize that it is not failing in a useful way but merely failing due to circumstance.

For instance, if a test makes assertions that are needlessly specific, they may begin to fail when the implementation details of the system change. The test needed to be updated just to keep up with the design change, even though the overall effect of the feature didn't really change and the intended claims made by the test didn't really change. This experience thwarts the two qualities of a good test: it's not providing me with useful information during such failures, and it's not giving me the confidence to make further changes as it hurts too much.

**We need to distinguish, though, between this bad kind of brittleness and the kind of brittleness that actually helps us to keep a sound test suite.**

Consider a feature whose test is complete today. It works against today's schema, filling in some entities appropriately and asserting on the entities' state after some feature gets exercised. Although it's completely testing the feature today, it may become meaningfully incomplete tomorrow after the schema changes. Worse yet, although it's no longer telling the full story, it may still be passing by unfortunate coincidence! This test was originally brittle in a bad way: a small change to the system caused the test to stop providing meaningful information whether passing or failing. The schema change caused the test's passing status to become a bit of a lie. Should the test be updated? Removed? Augmented with additional tests? **We don't know, because it is still green.**

We would rather this green brittle test at least be a red brittle test. This is the kind of brittleness we can value: a meaningful change in the system, which would have caused the test to be incomplete, causes the test to fail so that we at least know that it deserves attention to remain meaningful.

In a perfect world, we could even make it so that neither kind of brittleness, neither good nor bad brittleness, was even necessary. Some tests can be written so that they are always complete. It's not always possible, but a goal we can strive for. We do so by using "Complete Assertions" whenever possible.

Complete assertions are often tailored to the application in question, but their most common form is to provide an automatic, deep comparison between two complex objects. If (part of) your data model has a natural JSON representation, for instance, you can make a useful `ShouldMatch` assertion that takes two complex objects of the same type, turns them each to JSON, and asserts on the resulting strings being equal. The `ShouldValidate` and `ShouldNotValidate` assertions mentioned earlier are also examples of deliberately strict, brittle-in-a-good-way, complete assertions.

When the intent of your test is to show the full impact of a feature on your model, you are safer with this course-grained comparison than you would be with many individual property assertions. Without `ShouldMatch`, you have no way of knowing that you need to add a new assertion for a new property. With `ShouldMatch`, you're alerted the moment that your expected value became incomplete.

The validation helpers similarly protect us from misleadingly-green tests of validation rules. By asserting on the exact and complete set of error messages, we know that the validator is treating the model as valid or invalid as expected, and for the reason we expected. Avoid validation testing that only asserts on the bool of whether a model is valid or not; such tests can quickly become misleadingly-green.

## Diff Tool Convention

Since `ShouldMatch` essentially performs one large string comparison, failure messages of the "Expected (long string), Actual (long string)" can make it hard to quickly diagnose what went wrong. To provide a better developer experience, we can make it so that the test run opens such a failure in the developer’s diff tool of choice. The test fails, I see right away what part of my complete assertion failed, and get right into addressing the change.

`ShouldMatch` as defined in these examples is a good starting point, but this can vary based on project needs. In a project backed by MongoDb, for instance, you'd be better off using the MongDb C# library’s own notion of BSON string representations, so that the comparisons will be even more complete with respect to BSON types.

When testing that an expected exception is thrown, avoid NUnit-style `[ExpectedException]` attributes, instead using your assertion library's own ability to assert on exceptions. A complete assertion here may assert on both the expected exception type and the message. It is a judgement call whether doing so would be overly brittle. Simply be wary of whether your avoiding that brittleness allows the test to become incorrectly green. For instance, you may compromise by asserting on a pivotal part of an otherwise brittle exception message.
