# Contributing

Thanks for your interest in improving MicrosoftAgentFramework.

## Getting started

1. Fork and clone the repository.
2. Install .NET 8 SDK.
3. Configure `MicrosoftAgentFramework/appsettings.json` with your Azure OpenAI settings.
4. Build locally:

```bash
dotnet restore
dotnet build
```

## Development guidelines

- Keep pull requests small and focused.
- Prefer clear, maintainable code over clever shortcuts.
- Add concise comments only where behavior is non-obvious.
- Avoid breaking route contracts unless the PR explicitly targets API changes.
- Ensure all changed code compiles before submitting.

## Commit guidance

- Use descriptive, scoped commit messages.
- Split unrelated work into separate commits.
- Do not include secrets or personal credentials in commits.

## Pull request checklist

- [ ] Build passes locally.
- [ ] Changes are scoped to a single concern.
- [ ] Documentation updated when behavior/config changes.
- [ ] No credentials or sensitive data added.

## Reporting issues

When filing issues, include:

- Expected behavior
- Actual behavior
- Repro steps
- Environment details (.NET version, OS, relevant config)
