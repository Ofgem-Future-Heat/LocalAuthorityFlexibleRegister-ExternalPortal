namespace Ofgem_Web_LAF_ExternalPortal.Extensions
{
    public static class Routes
    {
        public const string Route1 = "Route 1 - Household Income";
        public const string Route2 = "Route 2 - Proxy Targeting";
        public const string Route3 = "Route 3 - NHS Referrals";
        public const string Route4 = "Route 4 - Bespoke Targeting";
    }

    public static class Route2Proxy
    {
        public const string Route2Proxy1Text = "Route 2 Proxy 1";
        public const string Route2Proxy2Text = "Route 2 Proxy 2";
        public const string Route2Proxy3Text = "Route 2 Proxy 3";
        public const string Route2Proxy4Text = "Route 2 Proxy 4";
        public const string Route2Proxy5Text = "Route 2 Proxy 5";
        public const string Route2Proxy6Text = "Route 2 Proxy 6";
        public const string Route2Proxy7PPMText = "Route 2 proxy 7 Pre-Payment Meter (PPM)";
        public const string Route2Proxy7NonPPMText = "Route 2 proxy 7 Non-Pre-Payment Meter (PPM)";
    }


    public static class Route2Validation
    {
        public static bool CanBeShown(string originalSelection, string nextSelection)
        {
            // Scenario 3g 
            if (originalSelection == nextSelection) return false;

            // Scenario 3b
            if (originalSelection == Route2Proxy.Route2Proxy3Text &&
                nextSelection == Route2Proxy.Route2Proxy1Text) return false;

            // Scenario 3a
            if (originalSelection == Route2Proxy.Route2Proxy1Text &&
                nextSelection == Route2Proxy.Route2Proxy3Text) return false;

            // Scenario 3c
            if (originalSelection == Route2Proxy.Route2Proxy6Text &&
                (nextSelection == Route2Proxy.Route2Proxy7PPMText ||
                 nextSelection == Route2Proxy.Route2Proxy7NonPPMText)) return false;

            // Scenario 3d
            if ((originalSelection == Route2Proxy.Route2Proxy7PPMText ||
                 originalSelection == Route2Proxy.Route2Proxy7NonPPMText) &&
                nextSelection == Route2Proxy.Route2Proxy6Text) return false;

            // Scenario 3e
            if (originalSelection == Route2Proxy.Route2Proxy5Text &&
                (nextSelection == Route2Proxy.Route2Proxy7PPMText ||
                 nextSelection == Route2Proxy.Route2Proxy7NonPPMText)) return false;

            // Scenario 3f
            if ((originalSelection == Route2Proxy.Route2Proxy7PPMText ||
                 originalSelection == Route2Proxy.Route2Proxy7NonPPMText) &&
                nextSelection == Route2Proxy.Route2Proxy5Text) return false;

            return true;
        }
    }
}
