namespace PSFR_DMS_CustomApis.Attributes
{
    /// <summary>
    /// Applied to a controller class to optionally hide it from Swagger documentation,
    /// based on configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class HideClassFromSwaggerIfConfiguredAttribute : Attribute
    {
    }
}
