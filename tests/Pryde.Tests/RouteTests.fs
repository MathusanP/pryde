module Pryde.Tests

open Microsoft.AspNetCore.Mvc.Testing
open Xunit
open System.Net
open Pryde.Api

type RouteTests() =
    let factory = new WebApplicationFactory<Program>()
    let client = factory.CreateClient()
    
    [<Fact>]
    member _.``GET system cpu returns 200`` () = task {
        let! response = client.GetAsync("/system/cpu")
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
    }
    
    [<Fact>]
    member _. ``GET system disk returns 200`` () = task {
        let! response = client.GetAsync("/system/disk")
        Assert.Equal(HttpStatusCode.OK, response.StatusCode)
    }
    
    [<Fact>]
    member _.``Get network port with invalid port returns 400`` () = task {
        let! response = client.GetAsync("/network/port/-1")
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
    }
    
    [<Fact>]
    member _.``Get network port with unused port returns 404`` () = task {
        let! response = client.GetAsync("/network/port/59999")
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode)
    }
    
    interface System.IDisposable with
        member _.Dispose() = factory.Dispose()