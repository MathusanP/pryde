open Giraffe
open Pryde.Api.Routes
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddGiraffe() |> ignore
    
    let app = builder.Build()
    app.UseGiraffe(webApp)
    
    app.Run()
    0  // Exit code