# Documentation Index

Complete documentation for the FLAC Capture utility.

## ?? Getting Started

### For New Users
1. **[Deployment Comparison](DEPLOYMENT_COMPARISON.md)** - Choose the right method for you
2. **[Quick Start Guide](QUICKSTART.md)** - Get running in 5 minutes (Windows)
3. **[User Manual](README.md)** - Complete feature documentation
4. **[Usage Examples](EXAMPLES.md)** - Common scenarios and tips

### For Docker/Container Users
1. **[Deployment Comparison](DEPLOYMENT_COMPARISON.md)** - Portainer vs Docker Compose
2. **[Container Quick Start](CONTAINER_QUICKSTART.md)** - Quick reference for Docker
3. **[Docker Deployment Guide](DOCKER_DEPLOYMENT.md)** - Complete command-line setup
4. **[Portainer Deployment Guide](PORTAINER_DEPLOYMENT.md)** - Web UI deployment for Synology NAS
5. **[Containerization Summary](CONTAINERIZATION_SUMMARY.md)** - Architecture and design

## ?? Technical Documentation

### Audio Capture
- **[Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md)** - Understanding WASAPI loopback, bit-perfect capture, and why volume doesn't affect recording quality

### FLAC Encoding
- **[Internal FLAC Update](INTERNAL_FLAC_UPDATE.md)** - How internal FLAC encoding works with Windows Media Foundation and automatic fallback to external encoder

### Troubleshooting
- **[File Locking Fix](FILE_LOCKING_FIX.md)** - Technical details on file handle management and retry logic

## ?? Documentation by Topic

### Installation & Setup
- [Quick Start Guide](QUICKSTART.md) - Windows installation
- [Docker Deployment](DOCKER_DEPLOYMENT.md) - Command-line setup
- [Portainer Deployment](PORTAINER_DEPLOYMENT.md) - Web UI setup
- [Container Quick Start](CONTAINER_QUICKSTART.md) - Docker commands

### Usage
- [User Manual](README.md) - All features explained
- [Usage Examples](EXAMPLES.md) - Practical scenarios
- [Container Quick Start](CONTAINER_QUICKSTART.md) - Service mode usage

### Configuration
- [Docker Deployment](DOCKER_DEPLOYMENT.md) - Environment variables
- [Portainer Deployment](PORTAINER_DEPLOYMENT.md) - Web UI configuration
- [Containerization Summary](CONTAINERIZATION_SUMMARY.md) - Configuration options

### Technical Deep Dives
- [Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md) - Audio capture theory
- [Internal FLAC Update](INTERNAL_FLAC_UPDATE.md) - Encoding implementation
- [File Locking Fix](FILE_LOCKING_FIX.md) - File handling details
- [Containerization Summary](CONTAINERIZATION_SUMMARY.md) - Architecture

### Troubleshooting
- [File Locking Fix](FILE_LOCKING_FIX.md) - Common file access issues
- [Docker Deployment](DOCKER_DEPLOYMENT.md) - Container troubleshooting
- [Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md) - Clipping and audio issues

## ?? Quick Links by Scenario

### "I want to capture audio on Windows"
? Start with [Quick Start Guide](QUICKSTART.md)

### "I want to run this on my Synology NAS"
? Choose one:
- **Prefer web interface?** ? [Portainer Deployment Guide](PORTAINER_DEPLOYMENT.md)
- **Prefer command line?** ? [Docker Deployment Guide](DOCKER_DEPLOYMENT.md)

### "I use Portainer for container management"
? Follow [Portainer Deployment Guide](PORTAINER_DEPLOYMENT.md)

### "I need to understand WASAPI capture"
? Read [Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md)

### "FLAC conversion is failing"
? Check [Internal FLAC Update](INTERNAL_FLAC_UPDATE.md)

### "I'm getting file locking errors"
? Review [File Locking Fix](FILE_LOCKING_FIX.md)

### "I want to see usage examples"
? Browse [Usage Examples](EXAMPLES.md)

### "I need to troubleshoot Docker issues"
? See [Container Quick Start](CONTAINER_QUICKSTART.md) or [Docker Deployment](DOCKER_DEPLOYMENT.md)

## ?? Document Summaries

| Document | Pages | Topics | Audience |
|----------|-------|--------|----------|
| [Quick Start](QUICKSTART.md) | 2-3 | Setup, first capture | Beginners |
| [User Manual](README.md) | 8-10 | All features, configuration | All users |
| [Examples](EXAMPLES.md) | 6-8 | Practical scenarios, tips | Intermediate |
| [Container Quick Start](CONTAINER_QUICKSTART.md) | 3-4 | Docker commands, monitoring | Docker users |
| [Docker Deployment](DOCKER_DEPLOYMENT.md) | 15-20 | Complete CLI deployment | DevOps |
| [Portainer Deployment](PORTAINER_DEPLOYMENT.md) | 12-15 | Web UI deployment | Synology users |
| [Containerization](CONTAINERIZATION_SUMMARY.md) | 8-10 | Architecture, design | Technical |
| [Audio Levels](AUDIO_LEVELS_EXPLAINED.md) | 10-12 | WASAPI theory, bit-perfect | Audio engineers |
| [FLAC Update](INTERNAL_FLAC_UPDATE.md) | 6-8 | Encoding implementation | Developers |
| [File Locking](FILE_LOCKING_FIX.md) | 5-6 | File handling, debugging | Developers |

## ?? Search by Keyword

- **WASAPI**: [Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md)
- **Docker**: [Docker Deployment](DOCKER_DEPLOYMENT.md), [Container Quick Start](CONTAINER_QUICKSTART.md)
- **Portainer**: [Portainer Deployment](PORTAINER_DEPLOYMENT.md)
- **Synology**: [Docker Deployment](DOCKER_DEPLOYMENT.md), [Portainer Deployment](PORTAINER_DEPLOYMENT.md)
- **Web UI**: [Portainer Deployment](PORTAINER_DEPLOYMENT.md)
- **FLAC**: [Internal FLAC Update](INTERNAL_FLAC_UPDATE.md), [User Manual](README.md)
- **M3U**: [Quick Start](QUICKSTART.md), [Examples](EXAMPLES.md)
- **Troubleshooting**: [File Locking Fix](FILE_LOCKING_FIX.md), [Docker Deployment](DOCKER_DEPLOYMENT.md), [Portainer Deployment](PORTAINER_DEPLOYMENT.md)
- **Configuration**: [Docker Deployment](DOCKER_DEPLOYMENT.md), [Portainer Deployment](PORTAINER_DEPLOYMENT.md), [Containerization](CONTAINERIZATION_SUMMARY.md)
- **Volume/Clipping**: [Audio Levels Explained](AUDIO_LEVELS_EXPLAINED.md)
- **Service Mode**: [Containerization](CONTAINERIZATION_SUMMARY.md), [Container Quick Start](CONTAINER_QUICKSTART.md)
- **Interactive Mode**: [Quick Start](QUICKSTART.md), [User Manual](README.md)

## ?? Version History

All documentation updated for **v1.0.0** (November 2025)

## ?? Contributing to Documentation

Found a typo or want to improve the docs? Documentation files are in the `docs/` directory of the repository.

---

**Need help?** Start with the appropriate guide above, or return to the [main README](../README.md).
