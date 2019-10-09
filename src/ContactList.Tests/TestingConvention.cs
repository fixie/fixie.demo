namespace ContactList.Tests
{
    using System.Diagnostics;
    using System.IO;
    using Fixie;
    using static Testing;

    public class TestingConvention : Execution
    {
        public void Execute(TestClass testClass)
        {
            testClass.RunCases(@case =>
            {
                var instance = testClass.Construct();

                @case.Execute(instance);

                instance.Dispose();

                var methodWasExplicitlyRequested = testClass.TargetMethod != null;

                if (methodWasExplicitlyRequested && @case.Exception is MatchException exception)
                    LaunchDiffTool(exception);
            });
        }

        static void LaunchDiffTool(MatchException exception)
        {
            var diffTool = Configuration["Tests:DiffTool"];

            if (!File.Exists(diffTool)) return;

            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            File.WriteAllText(expectedPath, Json(exception.Expected));
            File.WriteAllText(actualPath, Json(exception.Actual));

            using (Process.Start(diffTool, $"{expectedPath} {actualPath}")) { }
        }
    }
}
