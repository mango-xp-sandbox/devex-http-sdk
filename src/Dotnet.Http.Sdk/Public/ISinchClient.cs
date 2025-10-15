namespace Dotnet.Http.Sdk.Public
{
    /// <summary>
    /// Defines the contract for the Sinch client, providing access to contacts and messages APIs.
    /// </summary>
    public interface ISinchClient
    {
        /// <summary>
        /// Gets the API for managing contacts, including operations for retrieving, creating, updating, and deleting contacts.
        /// </summary>
        IContactsApi Contacts { get; }

        /// <summary>
        /// Gets the API for message-related operations, such as sending messages and retrieving message data.
        /// </summary>
        IMessagesApi Messages { get; }
    }
}