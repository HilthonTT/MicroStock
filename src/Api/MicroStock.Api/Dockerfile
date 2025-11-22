# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Api/MicroStock.Api/MicroStock.Api.csproj", "src/Api/MicroStock.Api/"]
COPY ["src/Modules/Users/Modules.Users.Infrastructure/Modules.Users.Infrastructure.csproj", "src/Modules/Users/Modules.Users.Infrastructure/"]
COPY ["src/Common/MicroStock.Common.Infrastructure/MicroStock.Common.Infrastructure.csproj", "src/Common/MicroStock.Common.Infrastructure/"]
COPY ["src/Common/MicroStock.Common.Application/MicroStock.Common.Application.csproj", "src/Common/MicroStock.Common.Application/"]
COPY ["src/Common/MicroStock.Common.Domain/MicroStock.Common.Domain.csproj", "src/Common/MicroStock.Common.Domain/"]
COPY ["src/Modules/Users/Modules.Users.Application/Modules.Users.Application.csproj", "src/Modules/Users/Modules.Users.Application/"]
COPY ["src/Modules/Users/Modules.Users.Domain/Modules.Users.Domain.csproj", "src/Modules/Users/Modules.Users.Domain/"]
COPY ["src/Modules/Users/Modules.Users.Presentation/Modules.Users.Presentation.csproj", "src/Modules/Users/Modules.Users.Presentation/"]
COPY ["src/Common/MicroStock.Common.Presentation/MicroStock.Common.Presentation.csproj", "src/Common/MicroStock.Common.Presentation/"]
RUN dotnet restore "./src/Api/MicroStock.Api/MicroStock.Api.csproj"
COPY . .
WORKDIR "/src/src/Api/MicroStock.Api"
RUN dotnet build "./MicroStock.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MicroStock.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroStock.Api.dll"]