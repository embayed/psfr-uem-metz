namespace PSFR_DMS_CustomApis.Attributes
{
    /// <summary>
    /// Applied to a controller action (method) to optionally hide it from Swagger documentation,
    /// based on configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class HideMethodFromSwaggerIfConfiguredAttribute : Attribute
    {
    }
}
