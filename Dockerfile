#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
ENV TasksLimit=1000
ENV Scan=True

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PrimesFinder.csproj", "."]
RUN dotnet restore "./PrimesFinder.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "PrimesFinder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PrimesFinder.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrimesFinder.dll"]