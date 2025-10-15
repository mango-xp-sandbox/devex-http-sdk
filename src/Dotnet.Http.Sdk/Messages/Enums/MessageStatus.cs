namespace Dotnet.Http.Sdk.Messages
{
    /// <summary>
    /// Represents the status of a message in the messaging system.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        /// The message has been queued and is awaiting processing.
        /// </summary>
        Queued,

        /// <summary>
        /// The message has been successfully delivered.
        /// </summary>
        Delivered,

        /// <summary>
        /// The message delivery has failed.
        /// </summary>
        Failed
    }
}