namespace Rydo.HttpClient
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    internal static class Helper
    {
        internal static void GuardNull<T>(
            [NotNull] T value,
            [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
        {
            if(value is null)
                throw new ArgumentNullException(paramName);
        }
    }
}