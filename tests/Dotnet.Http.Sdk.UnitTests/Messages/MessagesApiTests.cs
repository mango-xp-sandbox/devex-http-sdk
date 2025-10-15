namespace Dotnet.Http.Sdk.UnitTests.Messages
{
    using Core;
    using FluentAssertions;
    using Sdk.Messages;

    public class MessagesApiTests
    {
        [Fact(DisplayName = "Messages: Get operation builds correct request and maps response accordingly")]
        public async Task Messages_Get_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new MessageDto(
                    Id: "m1", From: "a", To: "b",
                    Content: "hi", Status: "queued",
                    CreatedAt: DateTime.UtcNow, DeliveredAt: null)
            };
            var api = new MessagesApi(gw);

            // Act
            var res = await api.GetAsync("m1");

            // Assert
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Get);
            req.RequestUri!.AbsoluteUri.Should().Contain("/messages/m1");

            res.Data.Id.Should().Be("m1");
        }

        [Fact(DisplayName = "Messages: GetPaged operation builds correct request and maps response accordingly")]
        public async Task Messages_GetPaged_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new MessagePagedDto(
                    new List<MessageDto>
                    {
                        new(Id: "m1", From: "a", To: "b",
                            Content: "hi", Status: "ack",
                            CreatedAt: DateTime.UtcNow, DeliveredAt: DateTime.UtcNow)
                    },
                    null,
                    0, // default
                    50 // default
                )
            };
            var api = new MessagesApi(gw);

            // Act
            var res = await api.GetPagedAsync();

            // Assert
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Get);
            req.RequestUri!.AbsoluteUri.Should().Contain("messages?page=0&limit=50");

            res.Data.Messages.Should().HaveCount(1);
            res.Data.Messages.ElementAt(0).Id.Should().Be("m1");
            res.Data.Messages.ElementAt(0).Status.Should().Be(MessageStatus.Delivered);
            res.Data.ContactsInfo.Should().BeNull();
        }

        [Fact(DisplayName = "Messages: Send operation builds correct request and maps response accordingly")]
        public async Task Messages_Send_builds_correct_request_and_maps_response()
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new MessageDto(
                    Id: "m1", From: "a", To: "b",
                    Content: "hi", Status: "queued",
                    CreatedAt: DateTime.UtcNow, DeliveredAt: null)
            };
            var api = new MessagesApi(gw);

            // Act
            var res = await api.SendAsync("a", "b", "hi");

            // Assert
            var req = gw.LastRequest!;
            req.Method.Should().Be(HttpMethod.Post);
            req.RequestUri!.AbsoluteUri.Should().Contain("/messages");
            res.Data.Id.Should().Be("m1");
        }

        [Theory(DisplayName = "Messages: Map message status correctly")]
        [InlineData("queued", MessageStatus.Queued)]
        [InlineData("ack", MessageStatus.Delivered)]
        [InlineData("acknowledged", MessageStatus.Delivered)]
        [InlineData("failed", MessageStatus.Failed)]
        [InlineData("???", MessageStatus.Failed)] // fallback
        public async Task Messages_Get_maps_status(string wire, MessageStatus expected)
        {
            // Arrange
            var gw = new FakeGateway
            {
                NextInternal = new MessageDto(
                    Id: "m1", From: "a", To: "b",
                    Content: "hi", Status: wire,
                    CreatedAt: DateTime.UtcNow, DeliveredAt: null)
            };
            var api = new MessagesApi(gw);

            // Act
            var res = await api.GetAsync("m1");

            // Assert
            res.Data.Status.Should().Be(expected);
        }
    }
}