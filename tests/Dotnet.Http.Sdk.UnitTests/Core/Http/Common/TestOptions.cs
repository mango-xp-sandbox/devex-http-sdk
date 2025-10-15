namespace Dotnet.Http.Sdk.UnitTests.Core.Http
{
    using Microsoft.Extensions.Options;

    internal sealed class TestOptions<T>(T value) : IOptions<T>
        where T : class, new()
    {
        public T Value { get; } = value;
    }
}