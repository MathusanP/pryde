open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Pryde.Data
open Pryde.Data.NetworkMetrics

[<EntryPoint>]
let main args =
    let cpu = DeviceMetrics.getCpuUsage ()
    let disk = DeviceMetrics.getDiskInfo ()
    let gpu = DeviceMetrics.getGraphicsInfo()
    let networkThroughput = NetworkMetrics.getNetworkThroughput()
    
    printfn "%A" cpu
    printfn "%A" disk
    printfn "%A" gpu
    printfn "%A" networkThroughput
    
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.Run()

    0 // Exit code
