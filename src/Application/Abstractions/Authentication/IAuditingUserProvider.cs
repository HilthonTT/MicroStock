namespace Application.Abstractions.Authentication;

public interface IAuditingUserProvider
{
    string GetUserId();
}
