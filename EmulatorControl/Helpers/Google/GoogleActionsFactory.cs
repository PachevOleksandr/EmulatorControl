using EmulatorControl.AndroidCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmulatorControl.Helpers.Google
{
    class GoogleActionsFactory
    {
        public static GoogleActions? Create(APILVL api, ADBCommandsSender adbCommandsSender)
        {
            switch (api)
            {
                case APILVL.API_30:
                    return new GoogleActionsAPI30(adbCommandsSender);
                case APILVL.API_34:
                    return new GoogleActionsAPI34(adbCommandsSender);
                default:
                    return null;
            }
        }
    }
}
