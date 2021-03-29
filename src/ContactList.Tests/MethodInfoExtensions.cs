using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Tests
{
    public static class MethodInfoExtensions
    {
        public static object Execute(this MethodInfo method, object instance, params object[] parameters)
        {
            object result;

            try
            {
                result = method.Invoke(instance, parameters != null && parameters.Length == 0 ? null : parameters);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw; //Unreachable.
            }

            if (result == null)
                return null;

            if (!ConvertibleToTask(result, out var task))
                return result;

            task.GetAwaiter().GetResult();

            if (method.ReturnType.IsGenericType)
            {
                var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                return property.GetValue(task, null);
            }

            return null;
        }

        static bool ConvertibleToTask(object result, out Task task)
        {
            if (result is Task t)
            {
                task = t;
                return true;
            }

            task = null;
            return false;
        }
    }
}
