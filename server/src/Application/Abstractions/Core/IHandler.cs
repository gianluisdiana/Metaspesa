namespace Metaspesa.Application.Abstractions.Core;

public interface IQueryHandler<TRequest, TResponse>
  where TRequest : IQuery<TResponse>
  where TResponse : notnull {
  Task<Result<TResponse>> Handle(
    TRequest query, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TRequest>
  where TRequest : ICommand {
  Task<Result> Handle(
    TRequest command, CancellationToken cancellationToken = default);
}


#pragma warning disable CA1040, S2326
public interface IQuery<TResponse> where TResponse : notnull;
public interface ICommand;
#pragma warning restore CA1040, S2326
