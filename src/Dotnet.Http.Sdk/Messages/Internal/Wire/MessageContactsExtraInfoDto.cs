namespace Dotnet.Http.Sdk.Messages
{
    internal sealed record MessageContactsExtraInfoDto(
        MessageContactDataDto? Additional1,
        MessageContactDataDto? Additional2,
        MessageContactDataDto? Additional3
    );
}