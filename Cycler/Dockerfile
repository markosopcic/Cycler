FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["Cycler.csproj", "./"]
RUN dotnet restore "./Cycler.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Cycler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cycler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "Cycler.dll"]