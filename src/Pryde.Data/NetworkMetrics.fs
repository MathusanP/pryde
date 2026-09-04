namespace Pryde.Data

open Pryde.Core
open System.Net.NetworkInformation
open System.Management

module NetworkMetrics =

    let getNetworkThroughput () : NetworkInfo list =
        use searcher = new ManagementObjectSearcher(
            "SELECT Name, BytesReceivedPersec, BytesSentPersec FROM Win32_PerfFormattedData_Tcpip_NetworkInterface")
        use results = searcher.Get()

        [ for item in results do
              let name = item.["Name"] :?> string
              let bytesReceived = item.["BytesReceivedPersec"] :?> uint64
              let bytesSent = item.["BytesSentPersec"] :?> uint64

              if bytesReceived > 0UL || bytesSent > 0UL then
                  yield {
                      AdapterName = name
                      BytesReceivedPerSec = int64 bytesReceived
                      BytesSentPerSec = int64 bytesSent
                  } ]
    let isPortListening (port: int) : bool =
        let ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()
        let listeners = ipGlobalProperties.GetActiveTcpListeners()

        listeners |> Array.exists (fun l -> l.Port = port)

    let getPortInfo (port: int) : PortInfo option =
        let ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties()
        let listeners = ipGlobalProperties.GetActiveTcpListeners()

        listeners
        |> Array.tryFind (fun l -> l.Port = port)
        |> Option.map (fun l ->
            {
                Port = l.Port
                Address = l.Address.ToString()
                Protocol = "TCP"
            })