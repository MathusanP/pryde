namespace Pryde.Data

open Pryde.Core
open System.Management
open LibreHardwareMonitor.Hardware

module DeviceMetrics =

    let getCpuTemperature () : float option =
        let computer = Computer()
        computer.IsCpuEnabled <- true
        computer.Open()
        
        let mutable result = None
        try 
            for hardware in computer.Hardware do
                if hardware.HardwareType = HardwareType.Cpu then
                    hardware.Update()
                    for sensor in hardware.Sensors do 
                        if sensor.SensorType = SensorType.Temperature && sensor.Name.Contains("Package") then
                            result <- sensor.Value |> Option.ofNullable |> Option.map float
        finally
            computer.Close()
        result
    
    let getUptime () : System.TimeSpan =
        use searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem")
        use results = searcher.Get()
    
        let mutable uptime = System.TimeSpan.Zero
    
        for item in results do
            match item.["LastBootUpTime"] with
            | :? string as bootTimeRaw ->
                let bootTime = ManagementDateTimeConverter.ToDateTime(bootTimeRaw)
                uptime <- System.DateTime.Now - bootTime
            | _ -> ()
    
        uptime

    let getCpuInfo () : CpuMetrics =
        use searcher = new ManagementObjectSearcher("SELECT LoadPercentage, NumberOfLogicalProcessors, Name, Architecture FROM Win32_Processor")
        use results = searcher.Get()

        let mutable totalLoad = 0.0
        let mutable coreCount = 0
        let mutable count = 0
        let mutable name = ""
        let mutable architecture = ""

        for item in results do
            let load = 
                match item.["LoadPercentage"] with 
                | :? uint16 as l -> l 
                | _ -> 0us
            let cores = 
                match item.["NumberOfLogicalProcessors"] with 
                | :? uint32 as c -> c 
                | _ -> 1u
            let itemName = 
                match item.["Name"] with 
                | :? string as s -> s 
                | _ -> "Unknown CPU"
            let architectureCode = 
                match item.["Architecture"] with 
                | :? uint16 as a -> a 
                | _ -> 0us

            let architectureType =
                match architectureCode with
                | 0us -> "x86"
                | 9us -> "x64"
                | 5us -> "ARM"
                | 12us -> "ARM64"
                | _ -> "Unknown"

            totalLoad <- totalLoad + float load
            coreCount <- coreCount + int cores
            count <- count + 1
            name <- itemName
            architecture <- architectureType

        {
            UsagePercent = if count > 0 then totalLoad / float count else 0.0
            CoreCount = coreCount
            Name = name
            Architecture = architecture
            Temperature = getCpuTemperature ()
            Uptime = getUptime ()
        } 
    
    let getDiskInfo () : DiskInfo list =
        use searcher = new ManagementObjectSearcher("SELECT DeviceID, VolumeName, Size, FreeSpace, FileSystem FROM Win32_LogicalDisk WHERE DriveType = 3")
        use results = searcher.Get()
        
        [ for item in results do
              let deviceId = 
                  match item.["DeviceID"] with 
                  | :? string as s -> s 
                  | _ -> ""
              let size = 
                  match item.["Size"] with 
                  | :? uint64 as s -> int64 s 
                  | _ -> 0L
              let freeSpace = 
                  match item.["FreeSpace"] with 
                  | :? uint64 as f -> int64 f 
                  | _ -> 0L
              let fileSystem = 
                  match item.["FileSystem"] with 
                  | :? string as fs -> fs 
                  | _ -> "Unknown"
              let volumeName =
                  match item.["VolumeName"] with 
                  | null -> "Unlabeled" 
                  | :? string as v -> v 
                  | _ -> "Unlabeled"

              yield {
                  DriveLetter = deviceId
                  VolumeName = volumeName
                  TotalBytes = size
                  FreeBytes = freeSpace
                  FileSystem = fileSystem
              } ]
        
    let getGraphicsInfo () : GraphicsInfo list =
        use searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController")
        use results = searcher.Get()
        
        let computer = Computer()
        computer.IsGpuEnabled <- true
        computer.Open()
        
        let list = 
            try
                [ for item in results do
                      let name = 
                          match item.["Name"] with 
                          | :? string as n -> n 
                          | _ -> "Unknown GPU"
                      let driverVersion = item.["DriverVersion"] |> unbox<string> |> Option.ofObj
                      let ram = 
                          match item.["AdapterRAM"] with
                          | :? uint32 as v -> Some (int64 v)
                          | :? int64 as v -> Some v
                          | _ -> None
                      
                      let mutable gpuTemp = None
                      let mutable gpuUsage = None
                      let mutable vramUsed = None

                      for hardware in computer.Hardware do
                          let isGpu = 
                              hardware.HardwareType = HardwareType.GpuNvidia ||
                              hardware.HardwareType = HardwareType.GpuAmd ||
                              hardware.HardwareType = HardwareType.GpuIntel

                          // Fuzzy name matching rather than strict equality
                          if isGpu && (hardware.Name.Contains(name) || name.Contains(hardware.Name)) then
                              hardware.Update()
                              for sensor in hardware.Sensors do
                                  if sensor.SensorType = SensorType.Temperature && sensor.Name.Contains("Core") then
                                      gpuTemp <- sensor.Value |> Option.ofNullable |> Option.map float
                                  if sensor.SensorType = SensorType.Load && sensor.Name.Contains("Core") then
                                      gpuUsage <- sensor.Value |> Option.ofNullable |> Option.map float
                                  if sensor.SensorType = SensorType.SmallData && sensor.Name.Contains("GPU Memory Used") then
                                      vramUsed <- sensor.Value |> Option.ofNullable |> Option.map float

                      yield {
                          name = name
                          DriverVersion = driverVersion
                          AdapterRamBytes = ram
                          Temperature = gpuTemp
                          UsagePercent = gpuUsage
                          VramUsedMB = vramUsed
                      } ]
            finally
                computer.Close()
        list