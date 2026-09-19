
ARG DOTNET_VERSION=9.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c "$BUILD_CONFIGURATION" -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
