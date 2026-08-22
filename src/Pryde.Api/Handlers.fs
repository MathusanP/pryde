module Pryde.Api.Handlers

open Giraffe
open Pryde.Data

let getCpuHandler : HttpHandler =
    fun next ctx ->
        json (DeviceMetrics.getCpuInfo ()) next ctx

let getDiskHandler : HttpHandler =
    fun next ctx ->
        json (DeviceMetrics.getDiskInfo ()) next ctx

let getGraphicsHandler : HttpHandler =
    fun next ctx ->
        json (DeviceMetrics.getGraphicsInfo ()) next ctx

let getUptimeHandler : HttpHandler =
    fun next ctx ->
        json (DeviceMetrics.getUptime ()) next ctx

let getNetworkThroughputHandler : HttpHandler =
    fun next ctx ->
        json (NetworkMetrics.getNetworkThroughput ()) next ctx

let getPortHandler (port: int) : HttpHandler =
    fun next ctx ->
        match NetworkMetrics.getPortInfo port with
        | Some info -> json info next ctx
        | None -> RequestErrors.NOT_FOUND (text "Port not listening") next ctx