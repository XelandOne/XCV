#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["XCV/XCV.csproj", "XCV/"]
RUN dotnet restore "XCV/XCV.csproj"
COPY . .
WORKDIR "/src/XCV"
RUN dotnet build "XCV.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "XCV.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "XCV.dll"]