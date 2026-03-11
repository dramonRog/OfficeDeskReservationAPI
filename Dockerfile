FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["OfficeDeskReservation.API/OfficeDeskReservation.API.csproj", "OfficeDeskReservation.API/"]
RUN dotnet restore "OfficeDeskReservation.API/OfficeDeskReservation.API.csproj"

COPY . .
WORKDIR	"/src/OfficeDeskReservation.API"

RUN dotnet build "OfficeDeskReservation.API.csproj" -c Release -o /app/build
RUN dotnet publish "OfficeDeskReservation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OfficeDeskReservation.API.dll"]