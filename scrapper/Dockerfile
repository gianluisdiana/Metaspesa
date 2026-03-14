FROM python:3.14-slim AS builder

COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/

WORKDIR /build

COPY pyproject.toml uv.lock ./

RUN uv sync --frozen --no-dev --no-install-project

FROM python:3.14-slim

WORKDIR /app

RUN apt-get update && apt-get install -y \
    firefox-esr \
    wget \
    ca-certificates \
    libgtk-3-0 \
    libdbus-glib-1-2 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

RUN wget -O /tmp/geckodriver.tar.gz \
    https://github.com/mozilla/geckodriver/releases/latest/download/geckodriver-linux64.tar.gz \
    && tar -xzf /tmp/geckodriver.tar.gz -C /usr/local/bin \
    && chmod +x /usr/local/bin/geckodriver \
    && rm /tmp/geckodriver.tar.gz

COPY --from=builder /build/.venv /app/.venv

ENV PATH="/app/.venv/bin:$PATH"

COPY . .

ENTRYPOINT [ "python", "./src/main.py" ]