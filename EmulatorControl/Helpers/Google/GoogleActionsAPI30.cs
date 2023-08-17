using EmulatorControl.AndroidCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EmulatorControl.Helpers.Google
{
    class GoogleActionsAPI30 : GoogleActions
    {
        public GoogleActionsAPI30(ADBCommandsSender adbCommandSender) : base(adbCommandSender)
        {
            xmlAttributes["CHECK_TERMS_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/terms_accept");
            xmlAttributes["SKIP_TERMS_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/terms_accept");
            xmlAttributes["CHECK_SYNC_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/signin_header_image");
            xmlAttributes["SKIP_SYNC_SCREEN"] = new XmlSearchAttribute("resource-id", "com.android.chrome:id/negative_button");
        }

        protected override void SkipSetupScreens()
        {
            if (adbCommandSender.IsElementExist(xmlAttributes["CHECK_TERMS_SCREEN"]))
            {
                adbCommandSender.TapOn(xmlAttributes["SKIP_TERMS_SCREEN"]);
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
