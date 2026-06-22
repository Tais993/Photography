namespace Application.interfaces.services;

public interface IApplicationResetService
{
    public void ResetApplicationData(bool dryRun = false);
}