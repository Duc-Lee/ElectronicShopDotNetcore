# ===== Stage 1: Build ung dung =====
# Su dung image .NET SDK de build project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy file solution va cac file csproj truoc
# Muc dich la tan dung cache cua Docker khi restore
COPY ["ElectronicShopDotNETcore.sln", "."]
COPY ["ElectronicShopMVC/ElectronicShopMVC.csproj", "ElectronicShopMVC/"]
COPY ["ElectronicShopMVC.DataAccess/ElectronicShopMVC.DataAccess.csproj", "ElectronicShopMVC.DataAccess/"]
COPY ["ElectronicShopMVC.Model/ElectronicShopMVC.Model.csproj", "ElectronicShopMVC.Model/"]
COPY ["ElectronicShopMVC.Utility/ElectronicShopMVC.Utility.csproj", "ElectronicShopMVC.Utility/"]
# Restore cac thu vien can thiet cho solution
RUN dotnet restore "ElectronicShopDotNETcore.sln"
# Copy toan bo source code vao container
COPY . .
# Chuyen den thu muc project MVC 
WORKDIR "/src/ElectronicShopMVC"
# Build va publish ung dung o che do Release
RUN dotnet publish "ElectronicShopMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Stage 2: Chay ung dung =====
# Su dung image ASP.NET nhe hon de chay web
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Copy file da publish tu stage build sang
COPY --from=build /app/publish .
# Mo cong cho web chay
EXPOSE 5000
EXPOSE 5001
# Cau hinh URL cho Kestrel server
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
# Lenh chay ung dung khi container khoi dong
ENTRYPOINT ["dotnet", "ElectronicShopMVC.dll"]
