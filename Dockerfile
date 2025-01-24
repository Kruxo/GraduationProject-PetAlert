# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy the entire project and build it
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Set the application to listen on port 5162
ENV ASPNETCORE_URLS=http://+:5162

# Expose port 5162 for HTTP traffic
EXPOSE 5162

# Set the entry point for the application
ENTRYPOINT ["dotnet", "GraduationProject.dll"]
