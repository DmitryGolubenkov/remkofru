namespace RemkofDataLibrary.BusinessLogic.Authorization.Registration
{
    public enum RegistrationStatus
    {
        Success,
        EmailExists,
        UsernameExists,
        UsernameAndEmailExists,
        Fail
    }
}
