open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Pryde.Data

[<EntryPoint>]
let main args =
    let cpu = DeviceMetrics.getCpuUsage ()
    let disk = DeviceMetrics.getDiskInfo ()
    let gpu = DeviceMetrics.getGraphicsInfo()
    let uptime = DeviceMetrics.getUptime()
    
    printfn "%A" cpu
    printfn "%A" disk
    printfn "%A" gpu
    printfn "%A" uptime
    
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.Run()

    0 // Exit code
