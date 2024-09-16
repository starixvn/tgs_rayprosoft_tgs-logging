#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG BuildConfiguration=Release
ARG CI_REGISTRY_IMAGE

FROM $CI_REGISTRY_IMAGE:builder as deps

RUN mkdir -p /src/packages

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS builder
# TODO: Remove for phase 1
# RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
ARG BuildConfiguration=Release
ENV NUGET_PACKAGES=/src/packages

COPY --from=deps $NUGET_PACKAGES/. $NUGET_PACKAGES/
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["LogApi/LogApi.csproj", "LogApi/"]
COPY ["LogApi.Core/LogApi.Core.csproj", "LogApi.Core/"]
COPY ["NotifyServiceClient/NotifyServiceClient.csproj", "NotifyServiceClient/"]
COPY ["LogApi.Models/LogApi.Models.csproj", "LogApi.Models/"]
COPY ["LogApi.Repositories/LogApi.Repositories.csproj", "LogApi.Repositories/"]
RUN dotnet restore -v n -r linux-x64  "LogApi/LogApi.csproj"
COPY . .
WORKDIR "/src/LogApi"
RUN dotnet build "LogApi.csproj" -c $BuildConfiguration -o /app/build

FROM builder AS publish
ARG BuildConfiguration
RUN dotnet publish -r linux-x64 --self-contained false "LogApi.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Apis.LogApi.dll"]