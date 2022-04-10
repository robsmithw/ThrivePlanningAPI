namespace ThrivePlanningAPI.Common.Constants
{
    public static class AppSettings
    {
        public static class AWS
        {
            public const string Profile = "AWS:Profile";
            public const string Region = "AWS:Region";
            public const string UserPoolId = "AWS:UserPoolId";
            public const string AppClientId = "AWS:AppClientId";
        }

        public static class ConnectionStrings
        {
            public const string DefaultConnection = "ConnectionStrings:DefaultConnection";
        }

        public static class FeatureFlags
        {
            public const string ShouldUseCognito = "FeatureFlags:ShouldUseCognito";
        }
    }
}
