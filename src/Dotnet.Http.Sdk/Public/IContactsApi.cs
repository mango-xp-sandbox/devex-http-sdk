namespace Dotnet.Http.Sdk.Public
{
    using Contacts;
    using Core.Exceptions;
    using Models;

    public interface IContactsApi
    {
        #region Read Operations

        /// <summary>
        /// Get Contact by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="ContactResponse" /></returns>
        /// <exception cref="ValidationException">Thrown when validation fails </exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="NotFoundException">Thrown when the resource is not found</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<ContactResponse>> GetAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Get Contacts Paged
        /// </summary>
        /// <param name="paginationOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="IReadOnlyList{ContactResponse}" /></returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<IReadOnlyList<ContactResponse>>> GetPagedAsync(
            PaginationOptions? paginationOptions,
            CancellationToken cancellationToken = default
        );

        #endregion

        #region Write Operations

        /// <summary>
        /// Create Contact
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="ContactResponse" /></returns>
        /// <exception cref="ValidationException">Thrown when validation fails </exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<ContactResponse>> CreateAsync(
            string name,
            string phone,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Update Contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="ContactResponse" /></returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="NotFoundException">Thrown when the resource is not found</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse<ContactResponse>> UpdateAsync(
            string id,
            string name,
            string phone,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Delete Contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Stub class <see cref="ApiResponse" /></returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized</exception>
        /// <exception cref="NotFoundException">Thrown when the resource is not found</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs</exception>
        Task<ApiResponse> DeleteAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        #endregion
    }
}