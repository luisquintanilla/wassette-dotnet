FROM mcr.microsoft.com/dotnet/sdk:10.0.112-bookworm-slim

ARG TARGETARCH
ARG WASM_TOOLS_VERSION=1.261.0
ARG WASSETTE_VERSION=0.8.0

RUN apt-get update \
    && apt-get install --yes --no-install-recommends ca-certificates curl git bash sudo tar \
    && rm -rf /var/lib/apt/lists/*

RUN set -eux; \
    case "${TARGETARCH}" in \
      amd64) wasm_asset="wasm-tools-${WASM_TOOLS_VERSION}-x86_64-linux.tar.gz"; wassette_asset="wassette_${WASSETTE_VERSION}_linux_amd64.tar.gz" ;; \
      arm64) wasm_asset="wasm-tools-${WASM_TOOLS_VERSION}-aarch64-linux.tar.gz"; wassette_asset="wassette_${WASSETTE_VERSION}_linux_arm64.tar.gz" ;; \
      *) echo "Unsupported container architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac; \
    mkdir -p /tmp/wasm-tools /tmp/wassette; \
    curl --fail --location --silent --show-error "https://github.com/bytecodealliance/wasm-tools/releases/download/v${WASM_TOOLS_VERSION}/${wasm_asset}" -o /tmp/wasm-tools.tar.gz; \
    tar -xzf /tmp/wasm-tools.tar.gz -C /tmp/wasm-tools; \
    install /tmp/wasm-tools/wasm-tools /usr/local/bin/wasm-tools; \
    curl --fail --location --silent --show-error "https://github.com/microsoft/wassette/releases/download/v${WASSETTE_VERSION}/${wassette_asset}" -o /tmp/wassette.tar.gz; \
    tar -xzf /tmp/wassette.tar.gz -C /tmp/wassette; \
    install /tmp/wassette/wassette /usr/local/bin/wassette; \
    rm -rf /tmp/wasm-tools /tmp/wassette /tmp/wasm-tools.tar.gz /tmp/wassette.tar.gz; \
    wasm-tools --version; \
    wassette --version

RUN useradd --create-home --shell /bin/bash --groups sudo vscode \
    && echo "vscode ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/vscode \
    && chmod 0440 /etc/sudoers.d/vscode

USER vscode
WORKDIR /workspaces/wassette-dotnet
