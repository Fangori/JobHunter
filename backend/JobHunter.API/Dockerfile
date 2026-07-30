FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY JobHunter.API/JobHunter.API.csproj JobHunter.API/
RUN dotnet restore JobHunter.API/JobHunter.API.csproj
COPY JobHunter.API/ JobHunter.API/
WORKDIR /src/JobHunter.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JobHunter.API.dll"]
