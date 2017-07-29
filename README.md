# .NET Core SDK CLI Helpers

## Usage

```
dotnet sdk [command]
dotnet sdk [version]
dotnet sdk get [version]
```

| Command | Description |
|-|-|
| `dotnet sdk [version]` | Swtiches to the [version] of .NET Core SDK version |
| `dotnet sdk latest` | Swtiches to the latest .NET Core SDK version |
| `dotnet sdk list` | Lists all installed .NET Core SDKs |
| `dotnet sdk releases` | Lists all available releases of .NET Core SDKs |
| `dotnet sdk get [version]` | Downloads the provided release version. ('' or 'latest' for the latest release) |
| `dotnet sdk help` | Display help |

    
## Installing the helpers

## Windows
1. Clone or download the repo on a desired location
2. From within Admninistrator CMD prompt, run **`dotnet .net`**

## Mac
1. Download `dotnet-sdk` script and set its executable bit with **`chmod +x dotnet-sdk`**. 
2. Create a symbolic link to it in <code>/usr/local/bin</code> using the command **`ln -s \<full_path\>/dotnet-sdk /usr/local/bin/`**. 

This will make it possible to invoke the command using the <code>dotnet sdk</code> syntax.
