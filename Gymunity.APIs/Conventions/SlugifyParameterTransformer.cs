using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Gymunity.APIs.Conventions
{
    /// <summary>
    /// Transforms route tokens to kebab-case (lowercase with hyphens).
    /// Converts PascalCase controller names to kebab-case for URLs.
    /// Example: "ClientProfile" → "client-profile"
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <summary>
        /// Transforms an outbound route value to kebab-case.
        /// </summary>
        /// <param name="value">The value to transform (e.g., controller name).</param>
        /// <returns>The kebab-case representation of the value.</returns>
        public string? TransformOutbound(object? value)
        {
            if (value == null) return null;

            string? stringValue = value.ToString();
            if (string.IsNullOrEmpty(stringValue)) return stringValue;

            // Convert PascalCase to kebab-case
            // Example: "ClientProfile" → "client-profile"
            // Example: "WorkoutLog" → "workout-log"
            return Regex.Replace(
                stringValue,
                "([a-z])([A-Z])",
                "$1-$2",
                RegexOptions.CultureInvariant,
                TimeSpan.FromMilliseconds(100))
                .ToLowerInvariant();
        }
    }
}