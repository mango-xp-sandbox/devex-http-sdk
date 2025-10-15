namespace Dotnet.Http.Sdk.UnitTests.Contacts
{
    using System.Web;
    using Core;
    using FluentAssertions;
    using Public;
    using Sdk.Contacts;

    public class ContactsApiTests
    {
        [Fact(DisplayName = "Contacts: Create operation builds correct request and maps response accordingly")]
        public async Task Contacts_Create_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new ContactDto("Alice", "+1555", "c1")
            };

            var api = new ContactsApi(gw);

            // Act
            var res = await api.CreateAsync("Alice", "+1555");

            // Assert request
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Post);
            req.RequestUri!.AbsoluteUri.Should().Contain("/contacts");

            // Assert mapping
            res.Data.Id.Should().Be("c1");
            res.Data.Name.Should().Be("Alice");
            res.Data.Phone.Should().Be("+1555");
        }

        [Fact(DisplayName = "Contacts: Update operation builds correct request and maps response accordingly")]
        public async Task Contacts_Update_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new ContactDto("Alice", "+2666", "c1")
            };
            var api = new ContactsApi(gw);
            // Act
            var res = await api.UpdateAsync("c1", "Alice", "+2666");
            // Assert request
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Patch);
            req.RequestUri!.AbsoluteUri.Should().Contain("/contacts/c1");

            // Assert mapping
            res.Data.Id.Should().Be("c1");
            res.Data.Name.Should().Be("Alice");
            res.Data.Phone.Should().Be("+2666");
        }

        [Fact(DisplayName = "Contacts: Delete operation builds correct request and maps response accordingly")]
        public async Task Contacts_Delete_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway();
            var api = new ContactsApi(gw);

            // Act
            var res = await api.DeleteAsync("c1");

            // Assert request
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Delete);
            req.RequestUri!.AbsoluteUri.Should().Contain("/contacts/c1");

            // Assert mapping
            res.Meta.Should().NotBeNull();
        }

        [Fact(DisplayName = "Contacts: Get operation builds correct request and maps response accordingly")]
        public async Task Contacts_Get_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new ContactDto("Alice", "+1555", "c1")
            };
            var api = new ContactsApi(gw);
            // Act
            var res = await api.GetAsync("c1");
            // Assert request
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Get);
            req.RequestUri!.AbsoluteUri.Should().Contain("/contacts/c1");
            // Assert mapping
            res.Data.Id.Should().Be("c1");
            res.Data.Name.Should().Be("Alice");
            res.Data.Phone.Should().Be("+1555");
        }

        [Fact(DisplayName = "Contacts: GetPaged builds correct request and maps response accordingly")]
        public async Task Contacts_GetPaged_return_meta_pagination_data()
        {
            var gw = new FakeGateway { NextInternal = new ContactPagedDto([], 1, 10) };
            var api = new ContactsApi(gw);

            var res = await api.GetPagedAsync(new PaginationOptions(1, 10));

            var uri = gw.LastRequest!.RequestUri!;
            uri.AbsolutePath.Should().EndWith("/contacts");

            var q = HttpUtility.ParseQueryString(uri.Query);
            q["page"].Should().Be("1");
            q["limit"].Should().Be("10");

            res.Meta.Pagination.Page.Should().Be(1);
            res.Meta.Pagination.PageSize.Should().Be(10);
        }

        [Fact(DisplayName = "Contacts: GetPaged defaults to page=0, limit=50 when null")]
        public async Task Contacts_GetPaged_defaults_to_page0_size50_when_null()
        {
            var gw = new FakeGateway { NextInternal = new ContactPagedDto([], 0, 50) };
            var api = new ContactsApi(gw);

            await api.GetPagedAsync();

            var uri = gw.LastRequest!.RequestUri!;
            uri.AbsolutePath.Should().EndWith("/contacts");
            var q = HttpUtility.ParseQueryString(uri.Query);
            q["page"].Should().Be("0");
            q["limit"].Should().Be("50");
        }
    }
}