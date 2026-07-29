open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Pryde.Data

[<EntryPoint>]
let main args =
    let cpu = WindowsMetrics.getCpuUsage ()
    let disk = WindowsMetrics.getDiskInfo ()
    let gpu = WindowsMetrics.getGraphicsInfo()
    
    printfn "%A" cpu
    printfn "%A" disk
    printfn "%A" gpu
    
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

    app.Run()

    0 // Exit code
