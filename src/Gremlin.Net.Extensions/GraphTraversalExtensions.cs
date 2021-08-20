using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Gremlin.Net.Process.Traversal;

namespace Gremlin.Net.Extensions
{
    public static class GraphTraversalExtensions
    {
        public static GremlinQuery ToGremlinQuery(this ITraversal traversal) => BuildGremlinQuery(traversal, true);

        private static GremlinQuery BuildGremlinQuery(ITraversal traversal, bool qualify, int? innerArgIndex = null)
        {
            var builder = new StringBuilder();

            if (qualify)
            {
                builder.Append("g");
            }

            var arguments = new Dictionary<string, object>();
            var first = true;

            foreach (var step in traversal.Bytecode.StepInstructions)
            {
                if (first && qualify || !first)
                {
                    builder.Append(".");
                }

                first = false;

                builder.Append($"{step.OperatorName}(");

                if (step.Arguments.Any())
                {
                    if (((object)step.Arguments.First()).GetType().Name.StartsWith("GraphTraversal"))
                    {
                        var index = innerArgIndex ?? 0;
                        foreach (var innerStep in step.Arguments)
                        {
                            var innerTraversal = (ITraversal)innerStep;
                            var innerQuery = BuildGremlinQuery(innerTraversal, step.OperatorName == "to", index + 1);

                            foreach (var innerArg in innerQuery.Arguments)
                            {
                                arguments.Add(innerArg.Key, innerArg.Value);
                            }

                            if (!innerArgIndex.HasValue && index > 0) builder.Append(", ");
                            builder.Append(innerQuery.ToString());
                            index++;
                        }

                    }
                    else
                    {
                        if (step.OperatorName == "property" || 
                            (step.OperatorName == "has" && step.Arguments.Last() is string))
                        {
                            var (key, value) = ((string)step.Arguments.First(), (object)step.Arguments.Last());

                            if (innerArgIndex.HasValue)
                            {
                                key = $"{key}_{innerArgIndex.Value}";
                            }

                            arguments.Add(key, value);

                            builder.Append($"'{step.Arguments.First()}', {key}");
                        }
                        else
                        {
                            var args = string.Join(", ", step.Arguments.Select(a => CalculateArgValue((object)a)));

                            builder.Append($"{args}");
                        }
                    }
                }

                builder.Append(")");
            }

            return new GremlinQuery(builder, arguments);
        }

        private static string CalculateArgValue(object arg)
        {
            arg.ThrowIfNull(nameof(arg));

            if (arg is string a)
            {
                return $"'{Regex.Replace(a, @"[^\w\s-]", "")}'";
            }

            if (arg is bool b)
            {
                return b ? "true" : "false";
            }

            if (arg.GetType().GetProperties().Where(x => x.CanRead).Any(x => x.Name == "EnumValue"))
            {
                return arg.GetType().GetProperty("EnumValue")?.GetValue(arg)?.ToString();
            }

            return arg.ToString();
        }
    }
}
