# Neural Network Designer (Console)

This repository contains legacy and modern C# projects. A minimal runnable entry point is available via `ConsoleApp`.

## Requirements

- .NET SDK (10.0 or later)
- Mono / xbuild for legacy project restoration on Linux

## Build steps

1. Open a terminal in `/workspaces/code-0`
2. Build the legacy Tokenizer dependency:

```bash
xbuild Tokenizer/Tokenizer.csproj /p:Configuration=Debug
```

3. Build the console application:

```bash
dotnet build ConsoleApp/ConsoleApp.csproj -c Debug
```

## Run

```bash
printf '\n' | dotnet run --project ConsoleApp/ConsoleApp.csproj -c Debug
```

You should see output similar to:

- `Neural Network Designer Console`
- `Brain initialized.`
- `Total neurons: 1000`

## Notes

- `ConsoleApp` now references `HAB` as a project dependency.
- `HAB` now references `JaStDev.LogService.dll` from `external libs/JaStDev.LogService.dll`.
- The legacy `Tokenizer` project is built with Mono/xbuild and produces the required `Tokenizer.dll`.
