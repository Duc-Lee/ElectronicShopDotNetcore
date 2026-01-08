# Stage 1: Build the application
# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and project files first to leverage Docker layer caching
COPY ["ElectronicShopDotNETcore.sln", "."]
COPY ["ElectronicShopMVC/ElectronicShopMVC.csproj", "ElectronicShopMVC/"]
COPY ["ElectronicShopMVC.DataAccess/ElectronicShopMVC.DataAccess.csproj", "ElectronicShopMVC.DataAccess/"]
COPY ["ElectronicShopMVC.Model/ElectronicShopMVC.Model.csproj", "ElectronicShopMVC.Model/"]
COPY ["ElectronicShopMVC.Utility/ElectronicShopMVC.Utility.csproj", "ElectronicShopMVC.Utility/"]

# Restore dependencies for all projects
RUN dotnet restore "ElectronicShopDotNETcore.sln"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/ElectronicShopMVC"

# Publish the application to a single folder, with release configuration
RUN dotnet publish "ElectronicShopMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create the final runtime image
# Use the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose ports for HTTP and HTTPS traffic
EXPOSE 5000
EXPOSE 5001

# It's a good practice to set the URLs for the Kestrel server
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "ElectronicShopMVC.dll"]
