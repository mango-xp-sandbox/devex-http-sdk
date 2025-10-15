namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using System.Net;
    using Common;
    using FluentAssertions;
    using Polly.Timeout;
    using Sdk.Core;
    using Sdk.Core.Exceptions;

    public class HttpGatewayTests
    {
        private static HttpRequestMessage NewRequest(HttpMethod m) => new(m, "https://example.test/resource");

        [Fact]
        public async Task NonGeneric_success_populates_meta_with_request_id_and_timestamp()
        {
            // Arrange
            var handler = StubHandler.FromResponse(HttpStatusCode.OK, resp =>
            {
                resp.Headers.TryAddWithoutValidation("x-request-id", "req-123");
                resp.Content = new StringContent(""); // content not required for non-generic
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            // Act
            var result = await gateway.SendAsync(req, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Meta.Should().NotBeNull();
            result.Meta.RequestId.Should().Be("req-123");
            result.Meta.ReceivedAtUtc.Should().NotBeNullOrWhiteSpace();
            result.Meta.Pagination.Should().BeNull();
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task NonGeneric_error_maps_via_ErrorMapper()
        {
            // Arrange: 404 with small JSON body
            var handler = StubHandler.FromResponse(HttpStatusCode.NotFound, resp =>
            {
                resp.Content = new StringContent("""{"message":"nope"}""");
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            // Act
            Func<Task> act = () => gateway.SendAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Generic_success_deserializes_maps_and_sets_meta()
        {
            // Arrange
            var payload = "ignored"; // we’ll return a FooInternal from our delegate anyway
            var handler = StubHandler.FromResponse(HttpStatusCode.OK, resp =>
            {
                resp.Content = new StringContent(payload);
                resp.Headers.TryAddWithoutValidation("x-request-id", "abc-999");
            });

            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            // Our deserialize function: read stream, return FooInternal
            Task<FooInternal> Deserialize(Stream s, CancellationToken ct) =>
                Task.FromResult(new FooInternal("hello"));

            // And a simple mapper FooInternal -> Foo
            Foo Map(FooInternal fi) => new(fi.Value);

            // Act
            var result = await gateway.SendAsync(req, Deserialize, Map, CancellationToken.None);

            // Assert
            result.Data.Value.Should().Be("hello");
            result.Meta.RequestId.Should().Be("abc-999");
            result.Meta.ReceivedAtUtc.Should().NotBeNullOrWhiteSpace();
            result.Meta.Pagination.Should().BeNull();
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Generic_failure_when_deserializer_returns_null()
        {
            // Arrange
            var handler = StubHandler.FromResponse(HttpStatusCode.OK, resp =>
            {
                resp.Content = new StringContent("{}");
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            Task<object?> Deserialize(Stream s, CancellationToken ct) => Task.FromResult<object?>(null);
            object Map(object _) => new();

            // Act
            Func<Task> act = () => gateway.SendAsync<object, object>(req, Deserialize, Map, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InternalServerException>()
                .WithMessage("*Deserialization returned null*");
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Generic_sets_pagination_when_internal_type_is_FooPagedResponse()
        {
            // Arrange
            var handler = StubHandler.FromResponse(HttpStatusCode.OK, resp =>
            {
                resp.Content = new StringContent("ignored");
                resp.Headers.TryAddWithoutValidation("x-request-id", "p-1");
            });

            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            Task<FooPagedResponse> Deserialize(Stream s, CancellationToken ct)
                => Task.FromResult(new FooPagedResponse(2, 25));

            // Map to any outward type; we only assert meta pagination
            string Map(FooPagedResponse _) => "ok";

            // Act
            var result = await gateway.SendAsync(req, Deserialize, Map, CancellationToken.None);

            // Assert
            result.Meta.Pagination.Should().NotBeNull();
            result.Meta.Pagination!.Page.Should().Be(2);
            result.Meta.Pagination!.PageSize.Should().Be(25);
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Transport_HttpRequestException_is_mapped_by_transport_mapper()
        {
            // Arrange
            var handler = StubHandler.FromException(new HttpRequestException("boom"));
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            // Act
            Func<Task> act = () => gateway.SendAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InternalServerException>();
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Transport_timeout_rejected_is_mapped_by_transport_mapper()
        {
            // Arrange
            var handler = StubHandler.FromException(new TimeoutRejectedException());
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Get);

            // Act
            Func<Task> act = () => gateway.SendAsync(req, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InternalServerException>()
                .WithMessage("*timed out*");
            handler.Calls.Should().Be(1);
        }

        [Fact]
        public async Task Error_body_is_read_and_passed_to_error_mapper()
        {
            // Arrange: 400 with ProblemDetails-like body
            var handler = StubHandler.FromResponse(HttpStatusCode.BadRequest, resp =>
            {
                resp.Content = new StringContent("""{"title":"Validation failed","detail":"Bad input"}""");
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new SingleClientFactory(client);
            var gateway = new HttpGateway(factory, new TestOptions<SinchOptions>(new SinchOptions()));

            using var req = NewRequest(HttpMethod.Post);

            // Act
            Func<Task> act = () => gateway.SendAsync(req, CancellationToken.None);

            // Assert: your ErrorMapper should map to ValidationException (400)
            await act.Should().ThrowAsync<ValidationException>();
            handler.Calls.Should().Be(1);
        }
    }
}