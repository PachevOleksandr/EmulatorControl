using EmulatorControl.AndroidCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmulatorControl.Helpers.Google
{
    class GoogleActionsAPI34 : GoogleActions
    {
        public GoogleActionsAPI34(ADBCommandsSender adbCommandSender) : base(adbCommandSender)
        {
            xmlAttributes["CHECK_SIGN_IN_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/signin_fre_dismiss_button");
            xmlAttributes["SKIP_SIGN_IN_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/signin_fre_dismiss_button");
            xmlAttributes["CHECK_SYNC_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/button_bar");
            xmlAttributes["SKIP_SYNC_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/negative_button");
        }
        protected override void SkipSetupScreens()
        {
            if (adbCommandSender.IsElementExist(xmlAttributes["CHECK_SIGN_IN_SCREEN"]))
            {
                adbCommandSender.TapOn(xmlAttributes["SKIP_SIGN_IN_SCREEN"]);
                Thread.Sleep(500);
            }

            if (adbCommandSender.IsElementExist(xmlAttributes["SKIP_SYNC_SCREEN"]))
            {
                adbCommandSender.TapOn(xmlAttributes["SKIP_SYNC_SCREEN"]);
                Thread.Sleep(500);
            }
        }
    }
}
