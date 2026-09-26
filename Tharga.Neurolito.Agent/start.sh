#!/bin/sh
set -e

echo "Starting Ollama..."
ollama serve &

echo "Waiting for Ollama to be ready..."
until curl -s http://localhost:11434/api/tags > /dev/null; do
  sleep 1
done

echo "Starting Tharga.Neurolito.Agent..."
exec dotnet Tharga.Neurolito.Agent.dll
