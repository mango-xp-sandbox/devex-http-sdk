namespace Dotnet.Http.Sdk.IntegrationTests.Messages
{
    using Core.Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Public;
    using Sdk.Core;
    using Sdk.Core.DependencyInjection;
    using Sdk.Core.Exceptions;
    using Sdk.Messages;

    [Collection("integration")]
    public class MessagesApiIntegrationTests(SdkFixture fx)
    {
        private readonly IContactsApi _contactsApi = fx.Contacts;
        private readonly IMessagesApi _api = fx.Messages;

        [Fact(DisplayName = "Messages: Send returns a message with Id and expected fields")]
        public async Task Send_returns_message_with_id()
        {
            // Arrange
            var name = $"Test User {Guid.NewGuid()}"; // unique name to avoid conflicts during creation
            var to = "+34978552341";
            var from = "service";
            var content = "hello integration";

            var response = await _contactsApi.CreateAsync(name, to);
            var mockUser = response.Data;

            // Act
            var res = await _api.SendAsync(mockUser.Id, from, content, CancellationToken.None);

            // Assert
            res.Should().NotBeNull();
            res.Data.Should().NotBeNull();
            res.Data.Id.Should().NotBeNullOrWhiteSpace();
            res.Data.To.Should().Be(mockUser.Id);
            res.Data.From.Should().Be(from);
            res.Data.Content.Should().Be(content);
            res.Meta.RequestId.Should().NotBeNull(); // best-effort, may be empty in stub
            res.Meta.ReceivedAtUtc.Should().NotBeNullOrWhiteSpace();
        }

        [Fact(DisplayName = "Messages: Get returns a previously created message")]
        public async Task Get_roundtrips_created_message()
        {
            // Arrange
            var name = $"Test User {Guid.NewGuid()}"; // unique name to avoid conflicts during creation
            var to = "+34978552341";
            var from = "service";
            var content = "hello integration";

            var response = await _contactsApi.CreateAsync(name, to);
            var mockUser = response.Data;

            var created = await _api.SendAsync(mockUser.Id, from, content, CancellationToken.None);

            // Act
            var fetched = await _api.GetAsync(created.Data.Id);

            // Assert
            fetched.Data.Id.Should().Be(created.Data.Id);
            fetched.Data.To.Should().Be(mockUser.Id);
            fetched.Data.From.Should().Be(from);
            fetched.Data.Content.Should().Be(content);
            fetched.Data.Status.Should().BeOneOf(MessageStatus.Queued, MessageStatus.Delivered, MessageStatus.Failed);
        }

        [Fact(DisplayName = "Messages: GetPaged returns a page and populates meta pagination when available")]
        public async Task GetPaged_returns_page()
        {
            // Arrange
            var name = $"Test User {Guid.NewGuid()}"; // unique name to avoid conflicts during creation
            var to = "+34978552341";
            var from = "service";
            var content = "hello integration";

            var response = await _contactsApi.CreateAsync(name, to);
            var mockUser = response.Data;

            var created = await _api.SendAsync(mockUser.Id, from, content, CancellationToken.None);

            // Act
            var page = await _api.GetPagedAsync(new PaginationOptions(0, 100)); // defaults (page=0, limit/size=50 depending on your API)

            // Assert
            page.Should().NotBeNull();
            page.Data.Should().NotBeNull();
            page.Data.Messages.Should().NotBeNull();

            // There should be at least the one we just created (stub-dependent; don’t assert exact count)
            page.Data.Messages.Any(m => m.Id == created.Data.Id).Should().BeTrue();

            page.Meta.Pagination.Should().NotBeNull();
            page.Meta.Pagination!.Page.Should().Be(0); // default sanes in the stub
            page.Meta.Pagination.PageSize.Should().Be(100); // default sane in the stub
        }

        [Fact(DisplayName = "Messages: Unauthorized when token is missing")]
        public async Task Unauthorized_when_token_missing()
        {
            // Arrange a second container without token
            var services = new ServiceCollection();
            services.AddSinchSdk(opts =>
            {
                // reuse base URL from fixture
                opts.BaseAddress = fx.Provider.GetRequiredService<IOptions<SinchOptions>>().Value.BaseAddress;
                opts.Auth.UseBearerToken(() => ""); // empty -> no header
            });
            using var sp = services.BuildServiceProvider();
            var api = sp.GetRequiredService<IMessagesApi>();

            // Act
            Func<Task> act = async () => await api.GetAsync("nonexistent", CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>();
        }
    }
}