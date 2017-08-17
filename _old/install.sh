#!/bin/bash
if [ -e /usr/local/bin/dotnet-sdk   ]; then
    rm /usr/local/bin/dotnet-sdk
fi
if [ -e /usr/local/bin/.net ]; then
    rm /usr/local/bin/.net
fi

ln -s /$PWD/dotnet-sdk /usr/local/bin
ln -s /$PWD/.net /usr/local/bin