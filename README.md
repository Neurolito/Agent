# Neurolito Agent

The agent that runs language models for [Neurolito](https://neurolito.com). Install it on a machine you
control; it runs models locally with [Ollama](https://ollama.com), connects to Neurolito, and takes jobs
from your team's queue.

> **Installing an agent?** Use the [install guide](https://neurolito.com/install-agent). It asks what the
> machine is - operating system, graphics card, container runtime - and shows the one route that fits,
> with a check that the agent connected.

This repository also holds [`Tharga.Neurolito.Client`](https://www.nuget.org/packages/Tharga.Neurolito.Client),
the .NET package for sending prompts to Neurolito, because the agent and the server speak through its contracts.

## What is here

| Folder | What it is |
|---|---|
| `Tharga.Neurolito.Agent` | The agent: an ASP.NET Core service that connects to the server and drives Ollama |
| `Tharga.Neurolito.Client` | The client package, and the message contracts shared by agent and server |
| `Tharga.Neurolito.Agent.Tests` | Tests for the agent |
| `install.agent.ps1` | Installs and runs the agent as a Docker container on Windows |

## Ways to run the agent

- **Container** - `ollama/ollama` with the agent on top, on Windows (Docker Desktop), Linux or macOS.
  Pass `--gpus all` to use an NVIDIA card.
- **Windows service** - the `neurolito-agent` Chocolatey package. Self-contained, so no .NET runtime is needed.
- **From source** - see below.

Both packaged routes need your team's install key, which the install guide fills in for you.
[`Tharga.Neurolito.Agent/README.md`](Tharga.Neurolito.Agent/README.md) has the details, including the
settings the container reads from the environment.

## Building from source

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download). For running the agent you also need
Ollama, or Docker.

```bash
dotnet build Tharga.Neurolito.Agent.slnx -c Release
dotnet test Tharga.Neurolito.Agent.slnx -c Release
```

To build the container image and run it on Windows:

```powershell
.\install.agent.ps1 -NoPull
```

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md), and report security
problems privately as described in [SECURITY.md](SECURITY.md).

## Licence

[Apache-2.0](LICENSE)
