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

        for hardware in computer.Hardware do
            if hardware.HardwareType = HardwareType.Cpu then
                hardware.Update()
                for sensor in hardware.Sensors do 
                    if sensor.SensorType = SensorType.Temperature && sensor.Name.Contains("Package") then
                        result <- sensor.Value |> Option.ofNullable |> Option.map float

        computer.Close()
        result
    
    let getUptime() : System.TimeSpan =
        use searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem")
        let results = searcher.Get()
    
        let mutable uptime = System.TimeSpan.Zero
    
        for item in results do
            let bootTimeRaw = item.["LastBootUpTime"] :?> string
            let bootTime = ManagementDateTimeConverter.ToDateTime(bootTimeRaw)
            uptime <- System.DateTime.Now - bootTime
    
        uptime
    

    let getCpuUsage () : CpuMetrics =
        use searcher = new ManagementObjectSearcher("SELECT LoadPercentage, NumberOfLogicalProcessors, Name FROM Win32_Processor")
        let results = searcher.Get()

        let mutable totalLoad = 0.0
        let mutable coreCount = 0
        let mutable count = 0
        let mutable name = ""

        for item in results do
            let load = item.["LoadPercentage"] :?> uint16
            let cores = item.["NumberOfLogicalProcessors"] :?> uint32
            let itemName = item.["Name"] :?> string
            totalLoad <- totalLoad + float load
            coreCount <- coreCount + int cores
            count <- count + 1
            name <- itemName

        {
            UsagePercent = if count > 0 then totalLoad / float count else 0.0
            CoreCount = coreCount
            Name = name
            Temperature = getCpuTemperature ()
            Uptime = getUptime()
        } 
    
    let getDiskInfo () : DiskInfo list =
        use searcher = new ManagementObjectSearcher("SELECT DeviceID, VolumeName, Size, FreeSpace, FileSystem FROM Win32_LogicalDisk WHERE DriveType = 3")
        let results = searcher.Get()
        
        [ for item in results do
              let deviceId = item.["DeviceID"] :?> string
              let size = item.["Size"] :?> uint64
              let freeSpace = item.["FreeSpace"] :?> uint64
              let fileSystem = item.["FileSystem"] :?> string
              
              let volumeName =
                  match item.["VolumeName"] with | null -> "Unlabeled" | v -> v :?> string
              yield {
                  DriveLetter = deviceId
                  VolumeName = volumeName
                  TotalBytes = int64 size
                  FreeBytes = int64 freeSpace
                  FileSystem = fileSystem
              }]
        
    let getGraphicsInfo () : GraphicsInfo list =
        use searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController")
        let results = searcher.Get()
        
        let computer = Computer()
        computer.IsGpuEnabled <- true
        computer.Open()
        
        let list = 
            [ for item in results do
                  let name = item.["Name"] :?> string
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
                    if (hardware.HardwareType = HardwareType.GpuNvidia ||
                        hardware.HardwareType = HardwareType.GpuAmd ||
                        hardware.HardwareType = HardwareType.GpuIntel)
                       && hardware.Name = name then

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
             
        computer.Close()
        list
    