using System;
using System.Collections.Generic;
using FluentAssertions;
using Gremlin.Net.Process.Traversal;
using Xunit;
using static Gremlin.Net.Process.Traversal.__;
using Order = Gremlin.Net.Process.Traversal.Order;

namespace Gremlin.Net.Extensions.Tests
{
    public class BytecodeExtensionsTests
    {
        private readonly GraphTraversalSource _g;

        public BytecodeExtensionsTests()
        {
            _g = AnonymousTraversalSource.Traversal();
        }

        [Fact]
        public void TestToGremlinQueryWithSimpleTraversal()
        {
            var query = _g.AddV("Organisation").Property("id", "acme-inc").Property("name", "Acme Inc").ToGremlinQuery();

            query.ToString().Should().Be("g.addV('Organisation').property('id', id).property('name', name)");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["id"] = "acme-inc",
                ["name"] = "Acme Inc"
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);
        }

        [Fact]
        public void TestToGremlinQueryWithInnerPropertyTraversal()
        {
            var query = _g
                .AddV("Organisation")
                .Property("id", "acme-inc")
                .Property("name", "Acme Inc")
                .AddE("linked_to")
                .To(_g.AddV("Organisation").Property("id", "some-other-company").Property("name", "Some other company"))
                .ToGremlinQuery();

            query.ToString().Should().Be("g.addV('Organisation').property('id', id).property('name', name).addE('linked_to').to(g.addV('Organisation').property('id', id_1).property('name', name_1))");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["id"] = "acme-inc",
                ["name"] = "Acme Inc",
                ["id_1"] = "some-other-company",
                ["name_1"] = "Some other company"
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);
        }

        [Fact]
        public void TestToGremlinQueryWithMultilayerInnerPropertyTraversal()
        {
            var query = _g
                .AddV("Organisation")
                .Property("id", "acme-inc")
                .Property("name", "Acme Inc")
                .AddE("linked_to")
                .To(_g.AddV("Organisation").Property("id", "some-other-company").Property("name", "Some other company").AddE("linked_to").To(_g.AddV("Company").Property("name", "some inner name")))
                .ToGremlinQuery();

            query.ToString().Should().Be("g.addV('Organisation').property('id', id).property('name', name).addE('linked_to').to(g.addV('Organisation').property('id', id_1).property('name', name_1).addE('linked_to').to(g.addV('Company').property('name', name_2)))");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["id"] = "acme-inc",
                ["name"] = "Acme Inc",
                ["id_1"] = "some-other-company",
                ["name_1"] = "Some other company",
                ["name_2"] = "some inner name"
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);
        }

        [Fact]
        public void TestToGremlinQueryWithNestedTraversal()
        {
            string query = _g.V("acme-inc").AddE("supplies").To(_g.V("google")).ToGremlinQuery();

            query.Should().Be("g.V('acme-inc').addE('supplies').to(g.V('google'))");
        }

        [Fact]
        public void TestToGremlinQueryComplex()
        {
            var query = _g.V("thomas").OutE("knows").Where(InV().Has("id", "mary")).Drop().ToGremlinQuery();

            query.ToString().Should().Be("g.V('thomas').outE('knows').where(inV().has('id', id_1)).drop()");

            query.Arguments
                .Should().HaveCount(1)
                .And
                .Contain(new KeyValuePair<string, object>("id_1", "mary"));
        }

        [Fact]
        public void TestToGremlinQuerySort()
        {
            string query = _g.V().HasLabel("person").Order().By("firstName", Order.Decr).ToGremlinQuery();

            query.Should().Be("g.V().hasLabel('person').order().by('firstName', decr)");
        }

        [Fact]
        public void TestToGremlinQueryHandlesNumbers()
        {
            var query = _g.V("thomas").Property("age", 44).ToGremlinQuery();

            query.ToString().Should().Be("g.V('thomas').property('age', age)");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["age"] = 44
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);
        }

        [Fact]
        public void TestToGremlinQueryFilterRange()
        {
            string query = _g.V().HasLabel("person").Has("age", P.Gt(40)).ToGremlinQuery();

            query.Should().Be("g.V().hasLabel('person').has('age', gt(40))");
        }

        [Fact]
        public void TestToGremlinQueryLoop()
        {
            var query = _g.V("thomas").Repeat(Out()).Until(Has("id", "robin")).Path().ToGremlinQuery();

            query.ToString().Should().Be("g.V('thomas').repeat(out()).until(has('id', id_1)).path()");

            query.Arguments
                .Should().HaveCount(1)
                .And
                .Contain(new KeyValuePair<string, object>("id_1", "robin"));
        }

        [Fact]
        public void TestToGremlinQueryCoalesce()
        {
            var query = _g.V("thomas")
                .Has("id", "123")
                .Coalesce<string>(
                    Has("id", "robin"),
                    Has("id", "leon"))
                .ToGremlinQuery();

            query.ToString().Should().Be("g.V('thomas').has('id', id).coalesce(has('id', id_1), has('id', id_2))");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["id"] = "123",
                ["id_1"] = "robin",
                ["id_2"] = "leon"
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);

        }

        [Fact]
        public void TestToGremlinMultipleArguments()
        {
            var query = _g.V("person")
                    .Has("name", "Thomas")
                    .Out("friend")
                    .Has("name", "Mary")
                    .Out("parent")
                    .Has("name", "Steve")
                    .ToGremlinQuery();

            query.ToString().Should().Be("g.V('person').has('name', name).out('friend').has('name', name_2).out('parent').has('name', name_3)");

            query.Arguments.Should().BeEquivalentTo(new Dictionary<string, object>
            {
                ["name"] = "Thomas",
                ["name_2"] = "Mary",
                ["name_3"] = "Steve"
            });
        }
    }
}
