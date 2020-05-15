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
            string query = _g.V("thomas").OutE("knows").Where(InV().Has("id", "mary")).Drop().ToGremlinQuery();
            
            query.Should().Be("g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()");
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
            string query = _g.V("thomas").Repeat(Out()).Until(Has("id", "robin")).Path().ToGremlinQuery();

            query.Should().Be("g.V('thomas').repeat(out()).until(has('id', 'robin')).path()");
        }

        [Fact]
        public void TestSetProperties()
        {
            var customer = new Customer("John");
            var query = _g
                .AddV(nameof(Customer))
                .SetProperties(customer)
                .ToGremlinQuery();

            query.ToString().Should().Be("g.addV('Customer').property('Id', Id).property('Name', Name)");

            IReadOnlyDictionary<string, object> expectedArguments = new Dictionary<string, object>
            {
                ["Id"] = "00000000-0000-0000-0000-000000000000",
                ["Name"] = "John"
            };

            query.Arguments.Should().BeEquivalentTo(expectedArguments);
        }

        internal class Customer
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public Customer(string name) => (Id, Name) = (Guid.Empty, name);
        }

    }
}
