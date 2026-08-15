module Pryde.Data.Tests.DeviceMetricsTests


open Xunit
open System.Net
open System.Net.Sockets
open Pryde.Data
open Pryde.Core

// Testing if WMI and Librehardware can execute

[<Fact>]
let ``getCpuInfo executes and returns non-empty hardware metadata`` () =
    let metrics = DeviceMetrics.getCpuInfo()
    
    Assert.False(System.String.IsNullOrWhiteSpace(metrics.Name))
    Assert.False(System.String.IsNullOrWhiteSpace(metrics.Architecture))
    Assert.True(metrics.CoreCount > 0 )
    Assert.True(metrics.UsagePercent > 0)
    Assert.True(metrics.UsagePercent >= 0.0 && metrics.UsagePercent <= 100.0)
    
[<Fact>]
let ``getDiskInfo executes and maps logical drive properties correctly`` () =
    let disks = DeviceMetrics.getDiskInfo()
    
    Assert.NotEmpty(disks)
    for disk in disks do
        Assert.False(System.String.IsNullOrWhiteSpace(disk.DriveLetter))
        Assert.True(disk.TotalBytes >= 0L)
        Assert.True(disk.FreeBytes >= 0L)
[<Fact>]
let ``getGraphicsInfo handles system GPUs without throwing casting exceptions`` () =
    let gpus = DeviceMetrics.getGraphicsInfo()
    
    for gpu in gpus do
        Assert.False(System.String.IsNullOrWhiteSpace(gpu.name))

[<Fact>]
let ``getUptime returns a positive TimeSpan`` () =
    let uptime = DeviceMetrics.getUptime()
    
    Assert.True(uptime.TotalSeconds > 0.0)
    
// Opening a port to listen to test
module NetworkMetricsTest =
    [<Fact>]
    let ``isPortListening returns true for a port with an active listener`` () =
        let testPort = 54321
        let listener = TcpListener(IPAddress.Loopback, testPort)
        listener.Start()
        
        try
            let result = NetworkMetrics.isPortListening testPort
            Assert.True(result)
        finally
            listener.Stop()
    
    [<Fact>]
    let ``isPortListening returns false for a port with no listener`` () =
        let testPort = 54322
        let result = NetworkMetrics.isPortListening testPort
        Assert.False(result)
        
    [<Theory>]
    [<InlineData(80)>]
    [<InlineData(443)>]
    [<InlineData(65000)>]
    let ``isPortListening does not throw for various port numbers`` (port: int) =
        let exn = Record.Exception(fun () -> NetworkMetrics.isPortListening port |> ignore)
        Assert.Null(exn)
        
    [<Fact>]
    let ``getNetworkThroughput returns a list without throwing`` () =
        let result = NetworkMetrics.getNetworkThroughput ()
        Assert.NotNull(result)
        
    [<Fact>]
    let ``getNetworkThroughput only returns adapters with non-zero traffic`` () =
        let result = NetworkMetrics.getNetworkThroughput ()
        result |> List.iter (fun adapter ->
            Assert.True(adapter.BytesReceivedPerSec > 0L || adapter.BytesSentPerSec > 0L))