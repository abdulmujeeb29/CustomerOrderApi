# Adjust DOTNET_OS_VERSION as desired
ARG DOTNET_OS_VERSION="-alpine"
ARG DOTNET_SDK_VERSION=8.0.100  # Use the full version

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION}${DOTNET_OS_VERSION} AS build
WORKDIR /src

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o /app
# Run EF migrations
RUN dotnet ef database update

# Final Stage/Image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_SDK_VERSION}
ENV ASPNETCORE_URLS http://+:8080
ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 8080
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app .

ENTRYPOINT ["dotnet", "CustomerOrderApi.dll"]  # Ensure the DLL name matches your project
