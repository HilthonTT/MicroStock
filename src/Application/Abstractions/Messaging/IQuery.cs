namespace Application.Abstractions.Messaging;

public interface IQuery<in TResponse>
    where TResponse : notnull
{
}
