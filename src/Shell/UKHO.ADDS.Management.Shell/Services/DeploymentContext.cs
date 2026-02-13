namespace UKHO.ADDS.Management.Shell.Services;

public sealed class DeploymentContext
{
    public event EventHandler<DeploymentChangedEventArgs>? Changed;

    public string? SelectedDeploymentId { get; private set; }

    public void SetSelectedDeploymentId(string? deploymentId)
    {
        if (string.Equals(SelectedDeploymentId, deploymentId, StringComparison.Ordinal))
        {
            return;
        }

        SelectedDeploymentId = deploymentId;
        Changed?.Invoke(this, new DeploymentChangedEventArgs(deploymentId));
    }
}

public sealed class DeploymentChangedEventArgs : EventArgs
{
    public DeploymentChangedEventArgs(string? deploymentId)
    {
        DeploymentId = deploymentId;
    }

    public string? DeploymentId { get; }
}
