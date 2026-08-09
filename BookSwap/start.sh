#!/usr/bin/env sh
set -eu
if [ ! -f .env ]; then
  cp .env.example .env
  echo "Создан .env из шаблона. Для публичного развёртывания замените JWT_KEY."
fi
docker compose up --build
