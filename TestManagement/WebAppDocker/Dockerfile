FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["WebAppDocker/WebAppDocker.csproj", "WebAppDocker/"]
RUN dotnet restore "WebAppDocker/WebAppDocker.csproj"
COPY . .
WORKDIR "/src/WebAppDocker"
RUN dotnet build "WebAppDocker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebAppDocker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "WebAppDocker.dll"]