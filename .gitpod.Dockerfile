FROM gitpod/workspace-full

USER root

# Install custom tools, runtime, etc. using apt-get
# For example, the command below would install "bastet" - a command line tetris clone:
#
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && rm -rf packages-microsoft-prod.deb && \
    add-apt-repository universe && \
    apt-get update && apt-get -y -o APT::Install-Suggests="true" install dotnet-sdk-3.1 && \
    apt -y clean;
#
# More information: https://www.gitpod.io/docs/42_config_docker/
