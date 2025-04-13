FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef

# Install additional tools if needed
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    curl \
    git \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*

# Add dotnet tools to PATH
ENV PATH $PATH:/root/.dotnet/tools

# Set the default shell to bash rather than sh
ENV SHELL /bin/bash