using Csharp.Common.Interfaces;

namespace Csharp.Common.EntityFramework.Repositories;

public delegate void TransmitRepositoryError(IErrorMessage<RepositoryError> errorMessage);