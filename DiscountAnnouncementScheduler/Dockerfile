FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MessageScheduler/MessageScheduler.csproj", "MessageScheduler/"]
COPY ["Scraper/Scraper.csproj", "Scraper/"]
COPY ["SharedServices/SharedServices.csproj", "SharedServices/"]
RUN dotnet restore "MessageScheduler/MessageScheduler.csproj"
COPY . .
WORKDIR "/src/MessageScheduler"
RUN dotnet build "MessageScheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessageScheduler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageScheduler.dll"]
