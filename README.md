# dotnet-sdk-helpers

On macOS, download <code>dotnet-sdk</code> script and set its executable bit with **<code>chmod +x dotnet-sdk</code>**. Now you can create a symbolic link to it in <code>/usr/local/bin</code> using the command **<code>ln -s dotnet-sdk /usr/local/bin/</code>**. This will make it possible to invoke the command using the <code>dotnet sdk</code> syntax.

If you get error *"Too many levels of symbolic links..."*, delete <code>dotnet-sdk</code> from <code>/usr/local/bin/</code> and use **<code>ln -s \<full path\>/dotnet-sdk /usr/local/bin/</code>** instead.
