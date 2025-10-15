namespace Dotnet.Http.Sdk.Public
{
    using Contacts;
    using Core.Exceptions;
    using Models;

    /// <summary>
    /// Defines a contract for managing contacts, including operations for retrieving, creating, updating, and deleting
    /// contacts.
    /// </summary>
    public interface IContactsApi
    {
        #region Read Operations

        /// <summary>
        /// Retrieves a contact by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to retrieve.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation, containing a
        /// <see cref="SinchResponse{ContactResponse}" /> with the contact details.
        /// </returns>
        /// <exception cref="ValidationException">Thrown when the provided <paramref name="id" /> is invalid.</exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized.</exception>
        /// <exception cref="NotFoundException">Thrown when the contact with the specified <paramref name="id" /> is not found.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<ContactResponse>> GetAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Retrieves a paged list of contacts.
        /// </summary>
        /// <param name="paginationOptions">
        /// The pagination options to control page size and number. If null, default pagination is
        /// applied.
        /// </param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation, containing a
        /// <see cref="SinchResponse{IReadOnlyList{ContactResponse}}" /> with the paged contacts.
        /// </returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<IReadOnlyList<ContactResponse>>> GetPagedAsync(
            PaginationOptions? paginationOptions = null,
            CancellationToken cancellationToken = default
        );

        #endregion

        #region Write Operations

        /// <summary>
        /// Creates a new contact with the specified name and phone number.
        /// </summary>
        /// <param name="name">The name of the contact to create.</param>
        /// <param name="phone">The phone number of the contact to create. Must be in E.164 format.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation, containing a
        /// <see cref="SinchResponse{ContactResponse}" /> with the created contact details.
        /// </returns>
        /// <exception cref="ValidationException">
        /// Thrown when the provided <paramref name="name" /> or <paramref name="phone" /> is
        /// invalid.
        /// </exception>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<ContactResponse>> CreateAsync(
            string name,
            string phone,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Updates an existing contact with the specified identifier, name, and phone number.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to update.</param>
        /// <param name="name">The new name for the contact.</param>
        /// <param name="phone">The new phone number for the contact. Must be in E.164 format.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation, containing a
        /// <see cref="SinchResponse{ContactResponse}" /> with the updated contact details.
        /// </returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized.</exception>
        /// <exception cref="NotFoundException">Thrown when the contact with the specified <paramref name="id" /> is not found.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse<ContactResponse>> UpdateAsync(
            string id,
            string name,
            string phone,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Deletes a contact by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the contact to delete.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the asynchronous operation, containing a <see cref="SinchResponse" />
        /// indicating the result of the delete operation.
        /// </returns>
        /// <exception cref="UnauthorizedException">Thrown when the request is unauthorized.</exception>
        /// <exception cref="NotFoundException">Thrown when the contact with the specified <paramref name="id" /> is not found.</exception>
        /// <exception cref="InternalServerException">Thrown when an internal server error occurs.</exception>
        Task<SinchResponse> DeleteAsync(
            string id,
            CancellationToken cancellationToken = default
        );

        #endregion
    }
}