FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG BUILD_VERSION

ENV NUGET_API_KEY ""

COPY . /src
WORKDIR /src

RUN dotnet restore Gremlin.Net.Extensions.sln
RUN dotnet build Gremlin.Net.Extensions.sln -c Release --no-restore
RUN dotnet test Gremlin.Net.Extensions.sln --no-restore
RUN dotnet pack Gremlin.Net.Extensions/Gremlin.Net.Extensions.csproj -c Release --include-symbols --no-build --output packages -p:PackageVersion=$BUILD_VERSION

ENTRYPOINT dotnet nuget push /src/packages/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
