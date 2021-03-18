namespace ContactList.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Fixie;
    using static System.Environment;
    using static Testing;

    public class TestingConvention : Execution
    {
        public async Task ExecuteAsync(TestClass testClass)
        {
            var instance = testClass.Construct();
            var methodWasExplicitlyRequested = testClass.Tests != null;

            foreach (var test in testClass.Tests)
            {
                await test.RunCasesAsync(UsingInputAttributes, instance);

                MethodInfo dispose = instance.GetType().GetMethod("Dispose");
                MethodInfoExtensions.Execute(dispose, instance, dispose.GetParameters());

                //if (methodWasExplicitlyRequested && @case.Exception is MatchException exception)
                //    LaunchDiffTool(exception);
            }
        }

        static IEnumerable<object[]> UsingInputAttributes(MethodInfo method)
           => method.GetCustomAttributes<InputAttribute>(true).Select(input => input.Parameters);

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputAttribute : Attribute
        {
            public InputAttribute(params object[] parameters)
            {
                Parameters = parameters;
            }

            public object[] Parameters { get; }
        }

        static void LaunchDiffTool(MatchException exception)
        {
            var tempPath = Path.GetTempPath();
            var expectedPath = Path.Combine(tempPath, "expected.txt");
            var actualPath = Path.Combine(tempPath, "actual.txt");

            var diffCommand = DiffCommand(expectedPath, actualPath);

            if (diffCommand != null)
            {
                File.WriteAllText(expectedPath, Json(exception.Expected));
                File.WriteAllText(actualPath, Json(exception.Actual));

                using (Process.Start("cmd", $"/c \"{diffCommand}\""))  {  }
            }
        }

        static string? DiffCommand(string expectedPath, string actualPath)
        {
            var gitconfig = Path.Combine(GetFolderPath(SpecialFolder.UserProfile), ".gitconfig");

            if (!File.Exists(gitconfig))
                return null;

            return File.ReadAllLines(gitconfig)
                .SkipWhile(x => !x.StartsWith("[difftool "))
                .Skip(1)
                .TakeWhile(x => !x.StartsWith("["))
                .Select(x => x.Split(new[] {'='}, 2))
                .Where(x => x[0].Trim() == "cmd")
                .Select(x => x[1].Trim()
                    .Replace("\\\"", "\"")
                    .Replace("$LOCAL", expectedPath)
                    .Replace("$REMOTE", actualPath))
                .SingleOrDefault();
        }
    }
}
