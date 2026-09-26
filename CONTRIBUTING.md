# Contributing

Thank you for helping. Issues and pull requests are both welcome.

## Before you start

For anything larger than a small fix, open an issue first, so we can agree on the approach before you
spend time on it. Security problems go through [SECURITY.md](SECURITY.md), not an issue.

## Making a change

1. Fork the repository and branch from `main`.
2. Build and test:
   ```bash
   dotnet build Tharga.Neurolito.Agent.slnx -c Release
   dotnet test Tharga.Neurolito.Agent.slnx -c Release
   ```
3. Add a test for the change. A bug fix comes with a test that fails without it.
4. Open a pull request against `main`. The build and tests run on it, and a maintainer reviews it.

Commit messages follow [Conventional Commits](https://www.conventionalcommits.org): `feat:`, `fix:`,
`docs:`, `test:`, `chore:`.

## The client contract

`Tharga.Neurolito.Client` is used by applications that upgrade on their own schedule, and agents of
different versions talk to the same server. So changes to anything in `Tharga.Neurolito.Client` are
**additive**:

- Add an optional property or a new type. Do not rename, remove or change the meaning of an existing one.
- A behaviour a caller must opt into is a new property or a new message, not a changed default.

If a change cannot be made that way, say so in the pull request, so it can be planned as a breaking release.

## Code style

- Code should explain itself. Public APIs carry XML documentation; other comments are for what the code
  cannot say.
- Prefer `init` over `set`, and records for data.

## Licence

By contributing you agree that your contribution is licensed under the [Apache-2.0](LICENSE) licence.
