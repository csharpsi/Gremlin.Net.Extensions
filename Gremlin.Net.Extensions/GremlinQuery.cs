using System.Collections.Generic;
using System.Text;

namespace Gremlin.Net.Extensions
{
    public class GremlinQuery
    {
        private readonly StringBuilder _queryBuilder;

        public IReadOnlyDictionary<string, object> Arguments { get; }

        internal GremlinQuery(StringBuilder queryBuilder, Dictionary<string, object> arguments)
        {
            queryBuilder.ThrowIfNull(nameof(queryBuilder));
            arguments.ThrowIfNull(nameof(arguments));

            _queryBuilder = queryBuilder;
            Arguments = arguments;
        }

        public override string ToString() => _queryBuilder.ToString();

        public static implicit operator string(GremlinQuery q) => q.ToString();
    }
}