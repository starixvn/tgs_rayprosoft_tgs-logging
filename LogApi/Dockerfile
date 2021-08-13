#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
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

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "LogApi.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Apis.LogApi.dll"]