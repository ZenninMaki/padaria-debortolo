FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY InfiniteCoffee2/InfiniteCoffee2.csproj InfiniteCoffee2/
RUN dotnet restore InfiniteCoffee2/InfiniteCoffee2.csproj
COPY InfiniteCoffee2/ InfiniteCoffee2/
RUN dotnet publish InfiniteCoffee2/InfiniteCoffee2.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "dotnet InfiniteCoffee2.dll --urls http://0.0.0.0:${PORT:-8080}"]
