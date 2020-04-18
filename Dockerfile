FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG SLN=Gremlin.Net.Extensions.sln
ARG PROJECT_PATH=Gremlin.Net.Extensions
ARG PROJECT=Gremlin.Net.Extensions.csproj
#ARG BUILD_VERSION

ENV NUGET_API_KEY ""

COPY . /src
WORKDIR /src

RUN dotnet restore $SLN
RUN dotnet build $SLN -c Release --no-restore
RUN dotnet test $SLN --no-restore
RUN dotnet pack $PROJECT_PATH/$PROJECT -c Release --include-symbols --no-build --output packages

# -p:PackageVersion=$BUILD_VERSION

ENTRYPOINT dotnet nuget push /src/packages/*.nupkg -k $NUGET_API_KEY -s https://api.nuget.org/v3/index.json
