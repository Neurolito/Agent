# Security policy

## Reporting a vulnerability

**Do not open a public issue for a security problem.** Report it privately through GitHub:
[Report a vulnerability](https://github.com/Neurolito/neurolito-agent/security/advisories/new).

Say what you found, how to reproduce it, and which version you tested. You will get an answer within
a few working days, and we will tell you when a fix is released.

## Scope

- The agent (`Tharga.Neurolito.Agent`), including its local HTTP API
- The client package (`Tharga.Neurolito.Client`)
- The install scripts and packaging in this repository

Problems with the Neurolito service itself can be reported the same way.

## Supported versions

Fixes are made in the latest release, so upgrading is the fix: `choco upgrade neurolito-agent`, or pull
the latest container image.

## How the agent is meant to be exposed

The agent's local HTTP API has no authentication. It is meant for diagnostics on the machine itself:

- The endpoints that act on the machine - running a prompt, installing or removing a model - are off
  unless `LocalApi:AllowActions` is set.
- `install.agent.ps1` publishes the container's ports on `127.0.0.1` only.

The agent reaches the server over an outbound connection, so no inbound port has to be opened.
A way to reach the local API from elsewhere, without changing these settings, is a vulnerability.
