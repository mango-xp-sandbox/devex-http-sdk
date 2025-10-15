namespace Dotnet.Http.Sdk.Messages
{
    using System.Net.Http.Json;
    using System.Text.Json;
    using Core;
    using Core.Exceptions;
    using Public;
    using Public.Models;

    /// <inheritdoc />
    internal class MessagesApi(IHttpGateway gateway) : IMessagesApi
    {
        /// <inheritdoc />
        public Task<SinchResponse<MessageResponse>> SendAsync(string to, string from, string content, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "messages")
            {
                Content = JsonContent.Create(new CreateMessageRequest
                (
                    from,
                    content,
                    new CreateMessageReceiverRequest(to)
                ), options: SinchJson.Options)
            };

            return gateway.SendAsync<MessageResponse, MessageDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<MessageDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }

        /// <inheritdoc />
        public async Task<SinchResponse<MessageResponse>> GetAsync(string id, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"messages/{id}");
            return await gateway.SendAsync<MessageResponse, MessageDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<MessageDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }

        /// <inheritdoc />
        public async Task<SinchResponse<MessagePagedResponse>> GetPagedAsync(PaginationOptions? paginationOptions = null, CancellationToken ct = default)
        {
            paginationOptions ??= new PaginationOptions();

            var req = new HttpRequestMessage(HttpMethod.Get, PaginationBuilderHelper.BuildPaginationEndpoint("messages", paginationOptions));

            return await gateway.SendAsync<MessagePagedResponse, MessagePagedDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<MessagePagedDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }
    }
}