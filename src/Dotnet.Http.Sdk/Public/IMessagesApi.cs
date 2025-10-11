namespace Dotnet.Http.Sdk.Public
{
    using Core.Exceptions;
    using Messages;
    using Models;

    public interface IMessagesApi
    {
        #region Write Operations

        /// <summary>
        /// Send Message
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="body"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="MessageResponse" /></returns>
        /// <exception cref="ValidationException">Thrown when validation fails </exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<MessageResponse>> SendAsync(
            string to,
            string from,
            string content,
            CancellationToken cancellationToken = default
        );

        #endregion

        #region Read Operations

        /// <summary>
        /// Get Message by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="MessageResponse" /></returns>
        /// <exception cref="ValidationException">Thrown when validation fails </exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="NotFoundException">Thrown when the resource is not found</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<MessageResponse>> GetAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get Messages Paged
        /// </summary>
        /// <param name="paginationOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="IReadOnlyList{MessageResponse}" /></returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<IReadOnlyList<MessageResponse>>> GetPagedAsync(
            PaginationOptions? paginationOptions,
            CancellationToken cancellationToken = default
        );

        #endregion
    }
}