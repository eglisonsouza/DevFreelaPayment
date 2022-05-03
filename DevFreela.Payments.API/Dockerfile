#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DevFreela.Payments.API/DevFreela.Payments.API.csproj", "DevFreela.Payments.API/"]
RUN dotnet restore "DevFreela.Payments.API/DevFreela.Payments.API.csproj"
COPY . .
WORKDIR "/src/DevFreela.Payments.API"
RUN dotnet build "DevFreela.Payments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DevFreela.Payments.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevFreela.Payments.API.dll"]