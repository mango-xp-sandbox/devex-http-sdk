namespace Dotnet.Http.Sdk.UnitTests.Core.Errors
{
    using System.Net;
    using FluentAssertions;
    using Polly.Timeout;
    using Sdk.Core;
    using Sdk.Core.Exceptions;

    public class ErrorMapperTests
    {
        #region Status Code Mapping Tests

        [Fact(DisplayName = "Mapping: 400 returns ValidationException with exception message")]
        public void Map_400_with_body_returns_ValidationException_with_body_message()
        {
            var body = """{"title":"Validation failed","detail":"Bad input"}""";
            var ex = ErrorMapper.Map(HttpStatusCode.BadRequest, body);

            ex.Should().BeOfType<ValidationException>();
            ex.Message.Should().Be(body);
            ((SinchException)ex).StatusCode.Should().Be(400);
        }

        [Fact(DisplayName = "Mapping: 400 without body returns ValidationException with default message")]
        public void Map_400_without_body_uses_default_message()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.BadRequest, null);

            ex.Should().BeOfType<ValidationException>();
            ex.Message.Should().Be("Bad request.");
            ((SinchException)ex).StatusCode.Should().Be(400);
        }

        [Fact(DisplayName = "Mapping: 401 returns UnauthorizedException")]
        public void Map_401_without_body_uses_default_message()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.Unauthorized, null);

            ex.Should().BeOfType<UnauthorizedException>();
            ex.Message.Should().Be("Unauthorized.");
            ((SinchException)ex).StatusCode.Should().Be(401);
        }

        [Fact(DisplayName = "Mapping: 403 returns UnauthorizedException")]
        public void Map_403_treated_as_Unauthorized()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.Forbidden, null);

            ex.Should().BeOfType<UnauthorizedException>();
            ex.Message.Should().Be("Forbidden.");
            ((SinchException)ex).StatusCode.Should().Be(403);
        }

        [Fact(DisplayName = "Mapping: 404 without body returns NotFoundException with exception message")]
        public void Map_404_with_body_returns_NotFound_with_body_message()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.NotFound, "missing");

            ex.Should().BeOfType<NotFoundException>();
            ex.Message.Should().Be("missing");
            ((SinchException)ex).StatusCode.Should().Be(404);
        }

        [Fact(DisplayName = "Mapping: 404 without body returns NotFoundException with default message")]
        public void Map_408_maps_to_InternalServer_with_timeout_message()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.RequestTimeout, null);

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Be("Request timed out.");
            ((SinchException)ex).StatusCode.Should().Be(408);
        }

        [Fact(DisplayName = "Mapping: 5XX returns InternalServerException with default message")]
        public void Map_500_family_maps_to_InternalServer_with_default_message()
        {
            var ex = ErrorMapper.Map(HttpStatusCode.BadGateway, null); // 502

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Be("Server error.");
            ((SinchException)ex).StatusCode.Should().Be(502);
        }

        [Fact(DisplayName = "Mapping: unknown status code returns InternalServerException with http code in message")]
        public void Map_unknown_status_falls_back_to_InternalServer_with_http_code_in_message()
        {
            var ex = ErrorMapper.Map((HttpStatusCode)418, null); // I'm a teapot

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Be("HTTP 418.");
            ((SinchException)ex).StatusCode.Should().Be(418);
        }

        [Fact(DisplayName = "Mapping: long body is trimmed to 512 characters plus ellipsis")]
        public void Map_trims_long_body_to_512_plus_ellipsis()
        {
            var longBody = new string('x', 600);
            var ex = ErrorMapper.Map(HttpStatusCode.BadRequest, longBody);

            ex.Message.Length.Should().Be(513); // 512 + '…'
            ex.Message![^1].Should().Be('…');
        }

        #endregion

        #region Transport Exceptions Mapping

        [Fact(DisplayName = "Mapping: Polly TimeoutRejectedException maps to InternalServerException with 504")]
        public void MapTransport_Polly_timeout_rejected_maps_to_InternalServer_504()
        {
            var ex = ErrorMapper.MapTransport(new TimeoutRejectedException());

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Contain("timed out");
            ((SinchException)ex).StatusCode.Should().Be(504);
        }

        [Fact(DisplayName = "Mapping: TaskCanceledException and OperationCanceledException map to InternalServerException with 499")]
        public void MapTransport_TaskCanceled_maps_to_InternalServer_499()
        {
            var ex1 = ErrorMapper.MapTransport(new TaskCanceledException());
            var ex2 = ErrorMapper.MapTransport(new OperationCanceledException());

            ex1.Should().BeOfType<InternalServerException>();
            ex2.Should().BeOfType<InternalServerException>();
            ((SinchException)ex1).StatusCode.Should().Be(499);
            ((SinchException)ex2).StatusCode.Should().Be(499);
            ex1.Message.Should().Contain("canceled").And.Contain("timed out");
        }

        [Fact(DisplayName = "Mapping: HttpRequestException maps to InternalServerException with 503 and preserves message")]
        public void MapTransport_HttpRequestException_maps_to_InternalServer_503_and_preserves_message()
        {
            var ex = ErrorMapper.MapTransport(new HttpRequestException("boom"));

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Be("boom");
            ((SinchException)ex).StatusCode.Should().Be(503);
            ex.InnerException.Should().BeOfType<HttpRequestException>();
        }

        [Fact(DisplayName = "Mapping: unknown exception maps to InternalServerException with 500 and preserves message")]
        public void MapTransport_unknown_exception_maps_to_InternalServer_500_and_preserves_message()
        {
            var ex = ErrorMapper.MapTransport(new InvalidOperationException("kaput"));

            ex.Should().BeOfType<InternalServerException>();
            ex.Message.Should().Be("kaput");
            ((SinchException)ex).StatusCode.Should().Be(500);
            ex.InnerException.Should().BeOfType<InvalidOperationException>();
        }

        #endregion
    }
}