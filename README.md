# Install Termux on Android mobile
Download Termux from `https://f-droid.org/en/packages/com.termux`

NOTE: No need to install F-Droid. You can install termux directly.

# Install OpenSSH to access server remotly

```
pkg install termux-services
pkg install openssh
sv-enable sshd
sv up sshd
```

## Enable login using key instead of password

1. Get Username `whoami`
2. Set Password to login using password `passwd`
3. Phone IP address `ifconfig`
4. Copy local key to remove machine `ssh-copy-id -p 8022  u0_a178@192.168.1.221`. Change username and IP accordingly
5. Login `ssh u0_a178@192.168.1.221 -p 8022`

## Disable password login
1. `nano $PREFIX/etc/ssh/sshd_config` 
2. Change `PasswordAuthentication yes` to `PasswordAuthentication no`
```
PrintMotd yes
PasswordAuthentication no
Subsystem sftp /data/data/com.termux/files/usr/libexec/sftp-server
```
3. Save and exit nano editor
4. `pkill sshd` and reconnect to ssh

# Install Linux Distro
```
pkg install git
pkg install proot-distro
proot-distro install alpine
proot-distro login alpine
```

# Download .Net Core (Inside proot-distro)
## Download .Net 8
https://dotnet.microsoft.com/en-us/download/dotnet/8.0

NOTE: Install .Net baserd on your phone architecture. Below example is for ARM32 (Alpine on Galaxy J7 Prime)

```
wget https://download.visualstudio.microsoft.com/download/pr/67254d85-fcab-415e-be63-15d3a3b26f4b/a593418de05aedaf8278c6c5b9371682/dotnet-sdk-8.0.204-linux-musl-arm.tar.gz

mkdir dotnet
mv dotnet-sdk-8.0.204-linux-musl-arm.tar.gz ./dotnet
cd dotnet
tar -xvf

apk add libstdc++ icu-libs
```

## Update Profile
Add file `/etc/profile.d/dotnet.sh` and below lines
```
export PATH="$PATH:/root/dotnet"
export ASPNETCORE_URLS="http://0.0.0.0:8000"
export DOTNET_GCHeapHardLimit=1C0000000
```

NOTE: `exit` and `proot-distro login alpine` to load profile

# Clone Code(Inside proot-distro)
```
git clone https://github.com/vinscom/mobile-ai-server.git

cd mobile-ai-server

dotnet publish -c Release

cd ~/mobile-ai-server/TestServer2/bin/Release/net8.0/publish
dotnet TestServer2.dll
```


#!/data/data/com.termux/files/usr/bin/sh
exec 'proot-distro login alpine' -D -e 2>&1