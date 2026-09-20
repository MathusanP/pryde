module Pryde.Api.Routes

open Giraffe
open Pryde.Api.Handlers
let webApp : HttpHandler =
    choose [
        GET >=> choose [
            route "/system/cpu" >=> getCpuHandler
            route "/system/disk" >=> getDiskHandler
            route "/system/graphics" >=> getGraphicsHandler
            route "/system/uptime" >=> getUptimeHandler
            route "/network/throughput" >=> getNetworkThroughputHandler
            routef "/network/port/%i" getPortHandler
        ]
    ]