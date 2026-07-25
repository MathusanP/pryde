namespace Pryde.Data

open Pryde.Core
open System.Management

module WindowsMetrics =
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
        }