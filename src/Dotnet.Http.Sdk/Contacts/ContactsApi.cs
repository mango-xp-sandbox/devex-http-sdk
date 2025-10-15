namespace Dotnet.Http.Sdk.Contacts
{
    using System.Net.Http.Json;
    using System.Text.Json;
    using Core;
    using Core.Exceptions;
    using Public;
    using Public.Models;

    /// <inheritdoc />
    internal class ContactsApi(IHttpGateway gateway) : IContactsApi
    {
        /// <inheritdoc />
        public async Task<SinchResponse<ContactResponse>> GetAsync(string id, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"contacts/{id}");
            return await gateway.SendAsync<ContactResponse, ContactDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<ContactDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }

        /// <inheritdoc />
        public async Task<SinchResponse<IReadOnlyList<ContactResponse>>> GetPagedAsync(PaginationOptions? paginationOptions = null, CancellationToken ct = default)
        {
            paginationOptions ??= new PaginationOptions();

            var req = new HttpRequestMessage(HttpMethod.Get, PaginationBuilderHelper.BuildPaginationEndpoint("contacts", paginationOptions));

            return await gateway.SendAsync<IReadOnlyList<ContactResponse>, ContactPagedDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<ContactPagedDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }

        /// <inheritdoc />
        public Task<SinchResponse<ContactResponse>> CreateAsync(string name, string phone, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "contacts")
            {
                Content = JsonContent.Create(new CreateContactRequest(name, phone), options: SinchJson.Options)
            };

            return gateway.SendAsync<ContactResponse, ContactDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<ContactDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }

        /// <inheritdoc />
        public Task<SinchResponse<ContactResponse>> UpdateAsync(string id, string name, string phone, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Patch, $"contacts/{id}")
            {
                Content = JsonContent.Create(new UpdateContactRequest(name, phone), options: SinchJson.Options)
            };

            return gateway.SendAsync<ContactResponse, ContactDto>(
                req,
                async (stream, token) =>
                {
                    var dto = await JsonSerializer.DeserializeAsync<ContactDto>(stream, SinchJson.Options, token);
                    if (dto is null) throw new InternalServerException("Empty body", 500);
                    return dto;
                },
                dto => dto.ToCanonical(),
                ct);
        }


        /// <inheritdoc />
        public Task<SinchResponse> DeleteAsync(string id, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"contacts/{id}");
            return gateway.SendAsync(req, ct);
        }
    }
}