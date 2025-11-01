# Use .NET 9 runtime as base
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Install ALSA (Advanced Linux Sound Architecture) and PulseAudio for audio support
RUN apt-get update && apt-get install -y \
    alsa-utils \
    pulseaudio \
    flac \
    && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FlacCapture/FlacCapture.csproj", "FlacCapture/"]
RUN dotnet restore "FlacCapture/FlacCapture.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/FlacCapture"
RUN dotnet build "FlacCapture.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FlacCapture.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for input/output
RUN mkdir -p /app/input /app/output /app/logs

# Set environment variables
ENV INPUT_DIR=/app/input
ENV OUTPUT_DIR=/app/output
ENV LOG_DIR=/app/logs
ENV PLAYBACK_VOLUME=0.7
ENV AUTO_DELETE_WAV=true
ENV SCAN_INTERVAL_SECONDS=30

# Volume mounts for Synology NAS
VOLUME ["/app/input", "/app/output", "/app/logs"]

# Run the application
ENTRYPOINT ["dotnet", "FlacCapture.dll", "--service"]
