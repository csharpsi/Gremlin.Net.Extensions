# Gremlin.Net.Extensions

[![Actions Status](https://github.com/csharpsi/Gremlin.Net.Extensions/workflows/Build/badge.svg)](https://github.com/csharpsi/Gremlin.Net.Extensions/actions)
[![Nuget Version](https://img.shields.io/nuget/v/Gremlin.Net.Extensions)](https://nuget.org/packages/Gremlin.Net.Extensions)

An extension framework to make building Gremlin.Net queries for CosmosDB a little less painful. 

Until CosmosDB supports Bytecode, it is necessary to write gremlin queries as strings. This lightweight extension framework will convert Bytecode into a useable query string.

## Install

```
Install-Package Gremlin.Net.Extensions
```

Or using the CLI

```
dotnet add package Gremlin.Net.Extensions
```

## Usage
Convert any `ITraversal` into a `GremlinQuery` object by calling `.ToGremlinQuery()`

Example:
```c#
var g = AnonymousTraversalSource.Traversal();
string query = g.V("thomas").OutE("knows").Where(__.InV().Has("id", "mary")).Drop().ToGremlinQuery();

// query will be "g.V('thomas').outE('knows').where(inV().has('id', 'mary')).drop()"
```

Properties are mapped to arguments:
```c#
var g = AnonymousTraversalSource.Traversal();
var query = g.AddV("Organisation").Property("id", "acme-inc").Property("name", "Acme Inc").ToGremlinQuery();

// query.ToString() will be "g.addV('Organisation').property('id', id).property('name', name)"
// query.Arguments dictionary will be:
// ["id"] = "acme-inc"
// ["name"] = "Acme Inc
```

[More examples can be found in the tests](https://github.com/csharpsi/Gremlin.Net.Extensions/blob/master/test/Gremlin.Net.Extensions.Tests/GraphTraversalExtensionsTests.cs)

With your query constructed, you can pass the string result into the gremlin client call:
```c#
var result = await gremlinClient.SubmitAsync<dynamic>(query.ToString());

// or with arguments...

var result = await gremlinClient.SubmitAsync<dynamic>(query.ToString(), query.Arguments);
```
