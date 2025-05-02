using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class Help
    {
        public static string GetMethodContextName()
        {
            var name = (new StackTrace().GetFrame(1)?.GetMethod() ?? throw new InvalidOperationException()).GetMethodContextName();
            return name;
        }

        private static string GetMethodContextName(this MethodBase method)
        {
            if (method.DeclaringType is null) return method.Name;

            if (method.DeclaringType.GetInterfaces().All(i => i != typeof(IAsyncStateMachine)))
                return method.DeclaringType.Name + "." + method.Name;

            var generatedType = method.DeclaringType;
            var originalType = generatedType.DeclaringType;

            if (originalType is null) return method.Name;

            var foundMethod = originalType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Static
                                          | BindingFlags.Public
                                          | BindingFlags.NonPublic
                                          | BindingFlags.DeclaredOnly)
                .Single(m => m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == generatedType);

            return method.DeclaringType.Name + "." + foundMethod.Name;

        }
    }
}
