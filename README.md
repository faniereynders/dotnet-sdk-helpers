# .NET Core SDK CLI Helpers

## Usage

```bash
dotnet sdk [command]
dotnet sdk [version]
dotnet sdk get [version] [platform]
```

| Command | Description |
|-|-|
| `dotnet sdk [version]` | Switches to the [version] of .NET Core SDK version |
| `dotnet sdk latest` | Switches to the latest .NET Core SDK version |
| `dotnet sdk list` | Lists all installed .NET Core SDKs |
| `dotnet sdk releases` | Lists all available releases of .NET Core SDKs |
| `dotnet sdk get [version] [platform]` | Downloads the provided release version & platform. ('' or 'latest' for the latest release. Default platform is `win-x64`) |
| `dotnet sdk help` | Display help |

## Installing the helpers

## Windows

1. Clone or download the repo on a desired location
2. From within Administrator CMD prompt, run **`dotnet .net`**

## Mac

1. Download `dotnet-sdk` script and set its executable bit with **`chmod +x dotnet-sdk`**.
2. Create a symbolic link to it in <code>/usr/local/bin</code> using the command **`ln -s \<full_path\>/dotnet-sdk /usr/local/bin/`**.

This will make it possible to invoke the command using the <code>dotnet sdk</code> syntax.

## Troubleshooting installation of helpers

## Windows

If the following message appears when running dotnet-.net.cmd from an administrator command prompt

    ```
    WARNING: The data being saved is truncated to 1024 characters. 
                                                                
    SUCCESS: Specified value was saved.                            
    ```
    
1. Edit the Path system variable manually and add the path that contains the cloned repository to it. E.g. if this repo was cloned at `C:\Projects\dotnet-sdk-helpers`, then this should be added to Path
2. Relaunch any open command shells. Alternatively, if chocolatey is installed, type `refreshenv` and press enter.