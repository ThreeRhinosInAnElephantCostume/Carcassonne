#!/bin/sh

# I wouldn't trust this script to work on anything other than Ubuntu 21.04.
# For other distros, check the following sites:
# https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
# https://www.mono-project.com/download/stable/
#
# You still may want to copy that hook though.

echo "Copying hooks to .git/hooks"

ln -s "$(pwd)/Scripts/githooks/pre-commit" "$(pwd)/.git/hooks/pre-commit"

echo "done"

echo "checking for apt"

if type "$apt" > /dev/null; then
    echo "apt could not be found / has failed to update"
    echo "you will have to install mono-complete and dotnet-cli manually"
    exit 0
fi

echo "apt found"

echo "Installing mono"

if [ $(dpkg-query -W -f='${Status}' mono 2>/dev/null | grep -c "ok installed") -eq 0 ];
then
    sudo apt install gnupg ca-certificates
    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
    echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
    sudo apt update

    sudo apt install mono-complete -y
else
    echo "mono already installed, attempting to install mono-complete just in case"
    sudo apt install mono-complete -y
fi

echo "Done"


echo "Installing dotnet-sdk"


if [ $(dpkg-query -W -f='${Status}' mono 2>/dev/null | grep -c "ok installed") -eq 0 ];
then
    wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb


    sudo apt-get update; \
    sudo apt-get install -y apt-transport-https && \
    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-5.0
else
    echo "dotnet already installed already installed."
fi

echo "Done"

echo "Installing the formatter"

dotnet add package format-hook --version 1.0.2

echo "Done

echo "setup complete"
