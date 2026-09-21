namespace Pryde.Api

open Giraffe
open Pryde.Api.Routes
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection


type Program() =
    class end

module Program =     
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        builder.Host.UseWindowsService() |> ignore
        builder.Services.AddGiraffe() |> ignore
        
        let app = builder.Build()
        app.UseGiraffe(webApp)
        
        app.Run()
        0  // Exit code