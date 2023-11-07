namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;

using Microsoft.Extensions.Logging;

public interface IValidatable
{
    public void ValidateEntityCreation(ILogger logger);

    public void ValidateEntityDelta(ILogger logger);
}
