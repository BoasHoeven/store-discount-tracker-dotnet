FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiscountAnnouncementScheduler/DiscountAnnouncementScheduler.csproj", "DiscountAnnouncementScheduler/"]
COPY ["ProductMonitoringService/ProductMonitoringService.csproj", "ProductMonitoringService/"]
COPY ["SharedUtilities/SharedUtilities.csproj", "SharedUtilities/"]
RUN dotnet restore "DiscountAnnouncementScheduler/DiscountAnnouncementScheduler.csproj"
COPY . .
WORKDIR "/src/DiscountAnnouncementScheduler"
RUN dotnet build "DiscountAnnouncementScheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscountAnnouncementScheduler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscountAnnouncementScheduler.dll"]
