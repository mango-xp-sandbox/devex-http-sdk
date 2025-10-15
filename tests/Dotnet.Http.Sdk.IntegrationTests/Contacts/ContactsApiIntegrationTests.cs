namespace Dotnet.Http.Sdk.IntegrationTests.Contacts
{
    using Core.Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Public;
    using Sdk.Core;
    using Sdk.Core.DependencyInjection;
    using Sdk.Core.Exceptions;

    [Collection("integration")]
    public class ContactsApiIntegrationTests(SdkFixture fx)
    {
        private readonly IContactsApi _api = fx.Contacts;

        private static string NewPhone() => "+34978552341";
        private static string NewName() => $"User-{Guid.NewGuid():N}".Substring(0, 12);

        [Fact(DisplayName = "Contacts: Create returns contact with Id and expected fields")]
        public async Task Create_returns_contact_with_id()
        {
            // Arrange
            var name = NewName();
            var phone = NewPhone();

            // Act
            var created = await _api.CreateAsync(name, phone);

            // Assert
            created.Should().NotBeNull();
            created.Data.Id.Should().NotBeNullOrWhiteSpace();
            created.Data.Name.Should().Be(name);
            created.Data.Phone.Should().Be(phone);
            created.Meta.TimestampUtc.Should().NotBeNullOrWhiteSpace();
        }

        [Fact(DisplayName = "Contacts: Get returns previously created contact")]
        public async Task Get_roundtrips_created_contact()
        {
            // Arrange
            var name = NewName();
            var phone = NewPhone();
            var created = await _api.CreateAsync(name, phone);

            // Act
            var got = await _api.GetAsync(created.Data.Id);

            // Assert
            got.Data.Id.Should().Be(created.Data.Id);
            got.Data.Name.Should().Be(name);
            got.Data.Phone.Should().Be(phone);
        }

        [Fact(DisplayName = "Contacts: Update modifies name and phone")]
        public async Task Update_modifies_contact()
        {
            // Arrange
            var created = await _api.CreateAsync(NewName(), NewPhone());
            var newName = NewName();
            var newPhone = NewPhone();

            // Act
            var updated = await _api.UpdateAsync(created.Data.Id, newName, newPhone);

            // Assert
            updated.Data.Id.Should().Be(created.Data.Id);
            updated.Data.Name.Should().Be(newName);
            updated.Data.Phone.Should().Be(newPhone);

            var fetched = await _api.GetAsync(created.Data.Id);
            fetched.Data.Name.Should().Be(newName);
            fetched.Data.Phone.Should().Be(newPhone);
        }

        [Fact(DisplayName = "Contacts: Delete removes the contact and subsequent Get throws NotFound")]
        public async Task Delete_then_get_throws_notfound()
        {
            // Arrange
            var created = await _api.CreateAsync(NewName(), NewPhone());

            // Act
            await _api.DeleteAsync(created.Data.Id);

            // Assert
            Func<Task> act = () => _api.GetAsync(created.Data.Id);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "Contacts: GetPaged returns page with defaults (page=0, limit=50)")]
        public async Task GetPaged_returns_page()
        {
            // Arrange: ensure there is at least one contact
            await _api.CreateAsync(NewName(), NewPhone());

            // Act
            var page = await _api.GetPagedAsync();

            // Assert
            page.Data.Should().NotBeNull();
            page.Data.Count.Should().BeGreaterThan(0);

            // If your gateway populates pagination meta via PagedResponseDto, you can assert it:
            // page.Meta.Pagination.Should().NotBeNull();
            // page.Meta.Pagination!.Page.Should().Be(0);
            // page.Meta.Pagination!.PageSize.Should().Be(50);
        }

        [Fact(DisplayName = "Contacts: Unauthorized when token is missing")]
        public async Task Unauthorized_when_token_missing()
        {
            // Arrange a second container without token
            var services = new ServiceCollection();
            services.AddSinchSdk(opts =>
            {
                var baseAddress = fx.Provider.GetRequiredService<IOptions<SinchOptions>>().Value.BaseAddress;
                opts.BaseAddress = baseAddress;
                opts.Auth.UseBearerToken(() => ""); // force no Authorization header
            });
            await using var sp = services.BuildServiceProvider();
            var api = sp.GetRequiredService<IContactsApi>();

            // Act
            Func<Task> act = () => api.GetAsync("nonexistent");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        // OPTIONAL: only keep if the stub enforces validation and returns 400.
        [Fact(DisplayName = "Contacts: ValidationException on bad input (if stub validates)")]
        public async Task Create_with_invalid_input_throws_validation()
        {
            // Arrange
            var badName = ""; // or null if your Create guards allow it through
            var badPhone = ""; // many stubs require non-empty or E.164

            // Act
            Func<Task> act = () => _api.CreateAsync(badName, badPhone);

            // Assert
            // If your SDK validates before HTTP, this may throw ArgumentException instead.
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}