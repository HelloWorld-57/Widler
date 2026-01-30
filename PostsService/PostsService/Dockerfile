ARG DOTNET_VERSION=9.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c "$BUILD_CONFIGURATION" -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PostsService.dll"]
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80
