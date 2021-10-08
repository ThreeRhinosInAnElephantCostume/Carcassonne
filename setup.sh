#!/bin/sh

echo "Copying hooks to .git/hooks"

ln -s ./Scripts/githooks/pre-commit ./.git/hooks/pre-commit


if ! command -v apt update &> /dev/null
then
    echo "apt could not be found / has failed to update"
    echo "you will have to install mono-devel and dotnet-cli manually"
    exit 0
fi

echo "Installing mono"

sudo apt install gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt update

sudo apt install mono-devel -y

echo "Done"


echo "Installing the dotnet core"

wget https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb


sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-5.0
  
echo "Done"
