This is document to show how to run dotnet server on your Arm based android mobile. 

| No | Phone                   | CPU Type (Phone Year) |
|----|-------------------------|-----------------------|
| 1  | Samsung Galaxy J7 Prime | Arm 32 (2016)         |
| 2  | Samsung S24 Ultra       | Arm 64  (2024)        |

# Install Termux on android mobile
Download Termux from `https://f-droid.org/en/packages/com.termux`

NOTE: No need to install F-Droid. You can install termux directly.

# Install OpenSSH to access server remotely

```
pkg update && pkg upgrade
pkg install termux-services
pkg install openssh
sv-enable sshd
sv up sshd
```

## Enable login using key instead of password

1. Get Username `whoami`
2. Set Password `passwd`
3. Phone IP address `ifconfig`
4. Copy your public key to remotely machine `ssh-copy-id -p 8022  u0_a178@192.168.1.221`. Change username and IP accordingly
5. Login `ssh u0_a178@192.168.1.221 -p 8022`

## Disable password based ssh login
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

NOTE: Install .Net based on your phone architecture (32 or 64). Below example is for ARM32

```
wget https://download.visualstudio.microsoft.com/download/pr/67254d85-fcab-415e-be63-15d3a3b26f4b/a593418de05aedaf8278c6c5b9371682/dotnet-sdk-8.0.204-linux-musl-arm.tar.gz

mkdir dotnet
mv dotnet-sdk-8.0.204-linux-musl-arm.tar.gz ./dotnet
cd dotnet
tar -xvf dotnet-sdk-8.0.204-linux-musl-arm.tar.gz
rm dotnet-sdk-8.0.204-linux-musl-arm.tar.gz

apk add libstdc++ icu-libs
```

## Add Env Variable
Add file `/etc/profile.d/dotnet.sh` and below lines
```
export PATH="$PATH:/root/dotnet"
export ASPNETCORE_URLS="http://0.0.0.0:8000"
export DOTNET_GCHeapHardLimit=1C0000000
```

NOTE: `exit` and `proot-distro login alpine` to load profile


# Run Code(Inside proot-distro)
```
git clone https://github.com/vinscom/android-dotnet-server.git

cd android-dotnet-server
dotnet publish -c Release

dotnet /root/android-dotnet-server/android-dotnet-server/bin/Release/net8.0/publish/android-dotnet-server.dll
```

Test URL: `http://192.168.1.221:8000/WeatherForecast`


# Install Nginx (Outside proot-distro)

1. Install nginx `pkg install nginx`
2. Edit Nginx config `nano $PREFIX/etc/nginx/nginx.conf` and add below config
```
http {
    upstream backend {
        server localhost:8000;
    }

    server {
        listen 8080;
        server_name example.com;

        location / {
            proxy_pass http://backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        }
    }
}
```
3. Enable service  `sv-enable nginx`
4. Port forward on your public IP. `Public IP:80 -> Android Phone IP:8080`
5. DNS map your domain to your public IP
6. Check Nginx is accessible `http://192.168.1.221:8080`
7. Check Nginx is publicly available `http://exmaple.com/WeatherForecast`


# Enable HTTPS (Inside proot-distro)
```
apk update && apk upgrade
apk add certbot
certbot certonly --http-01-port 8080
```

Certificates will be generated at location 
```
Successfully received certificate.
Certificate is saved at: /etc/letsencrypt/live/example.com/fullchain.pem
Key is saved at:         /etc/letsencrypt/live/example.com/privkey.pem
```

Copy cert to Nginx
```
mkdir -p /data/data/com.termux/files/usr/etc/letsencrypt/live
cp -r /etc/letsencrypt/archive/example.com  /data/data/com.termux/files/usr/etc/letsencrypt/live/
```

Update nginx config
```
http {
    upstream backend {
        server localhost:8000;
    }

    server {
        listen 8443 ssl;
        server_name example.com;

        location / {
            proxy_pass http://backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        }

        ssl_certificate /data/data/com.termux/files/usr/etc/letsencrypt/live/example.com/fullchain1.pem;
        ssl_certificate_key /data/data/com.termux/files/usr/etc/letsencrypt/live/example.com/privkey1.pem;
    }
}
```

Port forward on your public IP. `Public IP:443 -> Android Phone IP:8443`

HTTPS Working:  https://exmaple.com/WeatherForecast

TODO: Add auto renew certificate

# Start dotnet server on Termux Boot (Outside proot-distro)

Create Termux Service

```
mkdir -p $PREFIX/var/service/dotnetserver/log/supervise
mkdir -p $PREFIX/var/service/dotnetserver/supervise
nano $PREFIX/var/service/dotnetserver/run
nano $PREFIX/var/service/dotnetserver/log/run
chmod +x $PREFIX/var/service/dotnetserver/run
chmod +x $PREFIX/var/service/dotnetserver/log/run
```

Service file `$PREFIX/var/service/dotnetserver/run`
```
#!/data/data/com.termux/files/usr/bin/sh
exec proot-distro login alpine -- /root/android-dotnet-server/run.sh 2>&1
```

Log file `$PREFIX/var/service/dotnetserver/log/run`
```
#!/data/data/com.termux/files/usr/bin/sh
svlogger="/data/data/com.termux/files/usr/share/termux-services/svlogger"
exec "${svlogger}" "$@"
```

Commands:
1. Tail logs `tail -f $PREFIX/var/log/sv/dotnetserver/current`
2. Start Service `sv up dotnetserver`
3. Service status `sv status dotnetserver`
4. Stop service may not work correctly. Below is method
    - Stop service `sv down dotnetserver`
    - Check if service is still running `sv status dotnetserver`
    - Kill Service and Service log processes using ids provided in status.e.g. `kill -9 123456`

# Stop Mobile from Sleeping
You will have to install https://wiki.termux.com/wiki/Termux:Boot

NOTE: No need to install F-Droid. You can install termux boot directly.

```
mkdir ~/.termux/boot
nano ~/.termux/boot/stop-from-sleep
chmod +x ~/.termux/boot/stop-from-sleep
```

File `stop-from-sleep`
```
#!/data/data/com.termux/files/usr/bin/sh
termux-wake-lock
. $PREFIX/etc/profile
```

# Install Redis Service
Create Termux Service

```
mkdir -p $PREFIX/var/service/redis/log/supervise
mkdir -p $PREFIX/var/service/redis/supervise
nano $PREFIX/var/service/redis/run
nano $PREFIX/var/service/redis/log/run
chmod +x $PREFIX/var/service/redis/run
chmod +x $PREFIX/var/service/redis/log/run
```

Service file `$PREFIX/var/service/redis/run`
```
#!/data/data/com.termux/files/usr/bin/sh
exec redis-server redis-server $PREFIX/etc/redis.conf 2>&1
```

Log file `$PREFIX/var/service/redis/log/run`
```
#!/data/data/com.termux/files/usr/bin/sh
svlogger="/data/data/com.termux/files/usr/share/termux-services/svlogger"
exec "${svlogger}" "$@"
```
