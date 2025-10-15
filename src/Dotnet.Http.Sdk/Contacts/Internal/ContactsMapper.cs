namespace Dotnet.Http.Sdk.Contacts
{
    internal static class ContactsMapper
    {
        internal static ContactResponse ToCanonical(this ContactDto dto) => new(dto.Name, dto.Phone, dto.Id);

        internal static IReadOnlyList<ContactResponse> ToCanonical(this ContactPagedDto dto) =>
            dto.Contacts.Select(ToCanonical).ToList();
    }
}