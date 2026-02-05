using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Selu383.SP26.Tests.Controllers.Authentication;
using Selu383.SP26.Tests.Dtos;
using Selu383.SP26.Tests.Helpers;

namespace Selu383.SP26.Tests.Controllers.Locations;

[TestClass]
public class LocationsControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task ListAllLocations_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/locations");

        //assert
        await httpResponse.AssertLocationListAllFunctions();
    }

    [TestMethod]
    public async Task GetLocationById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetLocation();
        if (target == null)
        {
            Assert.Fail("Make List All locations work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/locations/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/locations/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<LocationDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get location by id to return the same data as the list all locations endpoint");
    }

    [TestMethod]
    public async Task GetLocationById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/locations/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/locations/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateLocation_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new LocationDto
        {
            Address = "asd",
            ManagerId = context.GetBobUserId(),
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/locations with no name");
    }

    [TestMethod]
    public async Task CreateLocation_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new LocationDto
        {
            Name = "a".PadLeft(121, '0'),
            Address = "asd",
            ManagerId = context.GetBobUserId(),
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/locations with a name that is too long");
    }

    [TestMethod]
    public async Task CreateLocation_NoAddress_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetLocation();
        await webClient.AssertLoggedInAsAdmin();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new LocationDto
        {
            Name = "asd",
            ManagerId = context.GetBobUserId(),
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/locations with no adress");
    }

    [TestMethod]
    public async Task CreateLocation_NoTables_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetLocation();
        await webClient.AssertLoggedInAsAdmin();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new LocationDto
        {
            Name = "asd",
            Address = "asd",
            ManagerId = context.GetBobUserId(),
            TableCount = 0
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/locations with no tables");
    }

    [TestMethod]
    public async Task CreateLocation_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new LocationDto
        {
            Name = "a",
            Address = "asd",
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        await httpResponse.AssertCreateLocationFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateLocation_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "asd",
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/locations when not logged in");
    }

    [TestMethod]
    public async Task CreateLocation_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new LocationDto
        {
            Name = "a",
            Address = "asd",
            TableCount = 5,
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/locations when logged in as bob");
    }

    [TestMethod]
    public async Task UpdateLocation_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/locations/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateLocation_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/locations/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateLocation_NoAddress_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/locations/{id} with a missing address");
    }

    [TestMethod]
    public async Task UpdateLocation_NoTables_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.TableCount = 0;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/locations/{id} with no tables");
    }

    [TestMethod]
    public async Task UpdateLocation_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var sueId = context.GetSueUserId();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = "cool new address";
        request.ManagerId = sueId;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        await httpResponse.AssertLocationUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateLocation_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/locations/{id} without being logged in");
    }

    [TestMethod]
    public async Task UpdateLocation_LoggedInAsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        await httpResponse.AssertLocationUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateLocation_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/locations/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/locations/{id} against a location bob manages while logged in as sue");
    }

    [TestMethod]
    public async Task DeleteLocation_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Address = "asd",
            Name = "asd",
            TableCount = 5,
        };
        await using var itemHandle = await webClient.CreateLocation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/locations/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/locations/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteLocation_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Address = "asd",
            Name = "asd",
            TableCount = 5,
        };
        await using var itemHandle = await webClient.CreateLocation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/locations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/locations/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteLocation_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new LocationDto
        {
            Address = "asd",
            Name = "asd",
            TableCount = 5,
        };
        await using var itemHandle = await webClient.CreateLocation(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        await webClient.DeleteAsync($"/api/locations/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/locations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/locations/{id} on the same item twice");
    }

    [TestMethod]
    public async Task DeleteLocation_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new LocationDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
            TableCount = 5,
        };
        await using var target = await webClient.CreateLocation(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        //act
        await webClient.DeleteAsync($"/api/locations/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/locations/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/locations/{id} against a location bob manages while logged in as sue");
    }
}
