using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Selu383.SP26.Tests.Controllers.Authentication;
using Selu383.SP26.Tests.Dtos;
using Selu383.SP26.Tests.Helpers;

namespace Selu383.SP26.Tests.Controllers.Locations;

internal static class LocationsHelpers
{
    internal static async Task<IAsyncDisposable?> CreateLocation(this HttpClient webClient, LocationDto request)
    {
        try
        {
            await webClient.AssertLoggedInAsAdmin();
            var httpResponse = await webClient.PostAsJsonAsync("/api/locations", request);
            var resultDto = await AssertCreateLocationFunctions(httpResponse, request, webClient);
            await webClient.AssertLoggedOut();
            request.Id = resultDto.Id;
            return new DeleteLocation(resultDto, webClient);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return null;
        }
    }

    internal static async Task<LocationDto?> GetLocation(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/locations");
            var getAllResult = await AssertLocationListAllFunctions(getAllRequest);
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return null;
        }
    }

    internal static async Task AssertLocationUpdateFunctions(this HttpResponseMessage httpResponse, LocationDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling PUT /api/locations/{id} with valid data to update a location");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<LocationDto>();
        resultDto.Should().BeEquivalentTo(request, "We expect the update location endpoint to return the result");

        var getByIdResult = await webClient.GetAsync($"/api/locations/{request.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the updated location by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<LocationDto>();
        dtoById.Should().BeEquivalentTo(request, "we expect the same result to be returned by an update location call as what you'd get from get location by id");

        var getAllRequest = await webClient.GetAsync("/api/locations");
        var listAllData =  await AssertLocationListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/locations");
        listAllData.Should().NotBeEmpty("list all should have something if we just updated a location");
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created location by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated location as what you'd get from get getting all locations");
    }

    internal static async Task<LocationDto> AssertCreateLocationFunctions(this HttpResponseMessage httpResponse, LocationDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/locations with valid data to create a new location");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<LocationDto>();
        Assert.IsNotNull(resultDto, "We expect json data when calling POST /api/locations");

        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created location to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create location endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/locations/{resultDto.Id}", "we expect the location header to point to the get location by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/locations/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created location by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<LocationDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create location as what you'd get from get location by id");

        var getAllRequest = await webClient.GetAsync("/api/locations");
        var listAllData =  await AssertLocationListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/locations");
        listAllData.Should().NotBeEmpty("list all should have something if we just created a location");
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created location by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created location as what you'd get from get getting all locations");

        return resultDto;
    }

    internal static async Task<List<LocationDto>> AssertLocationListAllFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/locations");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<LocationDto>>();
        Assert.IsNotNull(resultDto, "We expect json data when calling GET /api/locations");
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 locations when calling GET /api/locations");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all locations to have names");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all locations to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every location");
        return resultDto;
    }

    private sealed class DeleteLocation : IAsyncDisposable
    {
        private readonly LocationDto request;
        private readonly HttpClient webClient;

        public DeleteLocation(LocationDto request, HttpClient webClient)
        {
            this.request = request;
            this.webClient = webClient;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await webClient.AssertLoggedInAsAdmin();
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                await webClient.DeleteAsync($"/api/locations/{request.Id}");

            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
