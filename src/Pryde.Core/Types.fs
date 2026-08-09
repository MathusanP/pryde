namespace Pryde.Core

open System.Runtime.InteropServices
open System.Runtime.Serialization

type CpuMetrics = {
    UsagePercent: float
    CoreCount: int
    Name: string
    Temperature: float option // In Celsius
}

type DiskInfo = {
    DriveLetter: string
    VolumeName: string
    TotalBytes: int64
    FreeBytes: int64
    FileSystem: string
}

type MemoryInfo = {
    TotalBytes: int64
    UsedBytes: int64
    AvailableBytes: int64
}

type GraphicsInfo = {
    name: string
    DriverVersion: string option
    AdapterRamBytes: int64 option
    Temperature: float option
    UsagePercent: float option
    VramUsedMB: float option 
}