using OfficeOpenXml;

internal class EPPlusLicenseContext : EPPlusLicense
{
    private LicenseContext nonCommercial;

    public EPPlusLicenseContext(LicenseContext nonCommercial)
    {
        this.nonCommercial = nonCommercial;
    }
}