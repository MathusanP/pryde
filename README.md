# Pryde

Honestly this was more of a project to practice my Fsharp skills but I reckon there'd be some sort of use case for others so I've left it public.

Pryde is a self-hosted metrics API for your windows devices. It includes CPU, disk, memory, GPU and network information exposed over one local HTTP API.


## Setup

1) Navigate to the [releases page](https://github.com/MathusanP/pryde/releases) and download & extract the zip file to a location of your choice.
2) Run `install.ps1` in powershell.
  - If prompted by user account control, click yes, the installer requires admin privileges to register the windows service (and its also required to read from the temperature sensors)
3) Once finished, Pryde sits in C:/Pryde and will run as a Windows Service. It will now start automatically every time the machine boots so you don't need to keep a terminal open.
4) You can confirm if it works by opening `https://localhost:5000/system/cpu` in a browser, you should be able to see the json output with your CPU usage, name and temperature.

## Managing the service

Pryde runs as a standard Windows Service named Pryde, so it can be managed the same was as any other service.

```powershell
Start-Service Pryde  #start
Stop-Service Pryde  #stop
Restart-Service Pryde #retart
Get-Service Pryde #status
```

**Via the services app:** Open `services.msc`, find Pryde in the list, and use the Start/Stop/Restart buttons or right-click menu.

## Uninstalling
Run `uninstall.ps1` as Administrator (from the original releases folder. This stops the service and removes it from Windows.

## Endpoints

| Endpoint | Description |
|---|---|
| `GET /system/cpu` | CPU usage, core count, name, temperature |
| `GET /system/disk` | Per-drive space and filesystem info |
| `GET /system/graphics` | GPU name, driver, VRAM, temperature, load |
| `GET /system/uptime` | System uptime since last boot |
| `GET /network/throughput` | Per-adapter upload/download rates |
| `GET /network/port/{port}` | Whether a specific TCP port is currently listening |

All endpoints return JSON and require no authentication in this version — Pryde is intended to run on `localhost` or be reached only over a private network (e.g. [Tailscale](https://tailscale.com)). See **Exposing this remotely** below before opening it up any further than that.

## Exposing this remotely

By default, Pryde only answers requests reaching it directly — it does not open any ports on your router or make itself reachable from the internet on its own. If you want to check on your device while away from home, I'd reccomend doing this over a virtual private netowrk (vpn)
or by using a tunnel instead of port forwarding, this can be done using a service like Tailscale or using a Cloudflare tunnel.

## A note on admin privileges
 
Pryde runs under the `LocalSystem` account so it can read CPU temperature sensors, which requires elevated access on Windows. This means the service runs with full system privileges. If you don't need temperature monitoring and would rather run with reduced privileges, you can change the service's logon account via `services.msc` → Pryde → Properties → Log On - CPU (and GPU) temperature will simply come back empty in that case, everything else will keep working normally.

## Building from source
 
```bash
dotnet publish src/Pryde.Api -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
The resulting `Pryde.Api.exe` will be under `src/Pryde.Api/bin/Release/net9.0/win-x64/publish/`. Copy it alongside `install.ps1` and `uninstall.ps1` to reproduce a release package.
