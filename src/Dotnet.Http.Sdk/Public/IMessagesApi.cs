namespace Dotnet.Http.Sdk.Public
{
    using Core.Exceptions;
    using Messages;
    using Models;

    /// <summary>
    /// Defines the contract for message-related operations, including sending messages and retrieving message data.
    /// </summary>
    public interface IMessagesApi
    {
        #region Write Operations

        /// <summary>
        /// Sends a message from a specified sender to a recipient with the provided content.
        /// </summary>
        /// <param name="to">The recipient's identifier (the contact's ID).</param>
        /// <param name="from">The sender's identifier.</param>
        /// <param name="content">The content of the message to be sent.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="SinchResponse{T}" /> containing the <see cref="MessageResponse" /> with details of the sent message.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the input parameters fail validation.</exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is not authorized.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<MessageResponse>> SendAsync(
            string to,
            string from,
            string content,
            CancellationToken cancellationToken = default
        );

        #endregion

        #region Read Operations

        /// <summary>
        /// Retrieves the details of a message by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the message to retrieve.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="SinchResponse{T}" /> containing the <see cref="MessageResponse" /> with details of the requested message.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the input parameters fail validation.</exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is not authorized.</exception>
        /// <exception cref="NotFoundException">Thrown when the specified message is not found.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<MessageResponse>> GetAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Retrieves a paged list of messages based on the provided pagination options.
        /// </summary>
        /// <param name="paginationOptions">The options for paginating the message results. If null, default pagination is applied.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="SinchResponse{T}" /> containing a <see cref="MessagePagedResponse" /> with a list of messages and
        /// pagination metadata.
        /// </returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is not authorized.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<MessagePagedResponse>> GetPagedAsync(
            PaginationOptions? paginationOptions = null,
            CancellationToken cancellationToken = default
        );

        #endregion
    }
}