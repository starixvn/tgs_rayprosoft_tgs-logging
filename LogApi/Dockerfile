#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG BuildConfiguration=Release

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS builder
# TODO: Remove for phase 1
# RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["LogApi/LogApi.csproj", "LogApi/"]
COPY ["LogApi.Core/LogApi.Core.csproj", "LogApi.Core/"]
COPY ["NotifyServiceClient/NotifyServiceClient.csproj", "NotifyServiceClient/"]
COPY ["LogApi.Models/LogApi.Models.csproj", "LogApi.Models/"]
COPY ["LogApi.Repositories/LogApi.Repositories.csproj", "LogApi.Repositories/"]
RUN dotnet restore "LogApi/LogApi.csproj"
COPY . .
WORKDIR "/src/LogApi"
RUN dotnet build "LogApi.csproj" -c $BuildConfiguration -o /app/build

FROM builder AS publish
ARG BuildConfiguration
RUN dotnet publish "LogApi.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Apis.LogApi.dll"]