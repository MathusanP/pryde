module Pryde.Data.Tests.DeviceMetricsTests

open Xunit
open Pryde.Data
open Pryde.Core

// Testing if WMI and Librehardware can execute

[<Fact>]
let ``getCpuUsage executes and returns non-empty hardware metadata`` () =
    let metrics = DeviceMetrics.getCpuUsage()
    
    Assert.False(System.String.IsNullOrWhiteSpace(metrics.Name))
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
    