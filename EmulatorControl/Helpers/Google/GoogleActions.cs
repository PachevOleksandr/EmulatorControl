using EmulatorControl.AndroidCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace EmulatorControl.Helpers.Google
{
    abstract class GoogleActions
    {
        const string chromePackageName = "com.android.chrome";

        protected ADBCommandsSender adbCommandSender;

        protected Dictionary<string, XmlSearchAttribute> xmlAttributes = new Dictionary<string, XmlSearchAttribute>
        {
            {  "OPEN_CHROME", new XmlSearchAttribute("text", "Chrome") },
            {  "SEARCH_V1", new XmlSearchAttribute("resource-id", "com.android.chrome:id/search_box") },
            {  "SEARCH_V2", new XmlSearchAttribute("class", "android.widget.EditText") },
            {  "CHECK_AGREEMENT_POPUP", new XmlSearchAttribute("resource-id", "KByQx") },
            {  "SCROLL_AGREEMENT_POPUP", new XmlSearchAttribute("resource-id", "KByQx") },
            {  "CLOSE_AGREEMENT_POPUP", new XmlSearchAttribute("resource-id", "L2AGLb") },
            {  "CHECK_INFOBAR_POPUP", new XmlSearchAttribute("resource-id", "com.android.chrome:id/infobar_close_button") },
            {  "CLOSE_INFOBAR_POPUP", new XmlSearchAttribute("resource-id", "com.android.chrome:id/infobar_close_button") },
            {  "IP_BLOCK", new XmlSearchAttribute("resource-id", @"sa-tpcc_[\w\d_]+_2") },
        };

        public GoogleActions(ADBCommandsSender adbCommandSender)
        {
            this.adbCommandSender = adbCommandSender;
        }

        public bool IsOpen()
        {
            return adbCommandSender.GetTopApp() == chromePackageName;
        }

        public void Open()
        {
            if (!IsOpen())
            {
                string uiHierarchy = adbCommandSender.GetUIHierarchy();
                var p = adbCommandSender.GetElementTapPoint(xmlAttributes["OPEN_CHROME"], uiHierarchy);
                adbCommandSender.Tap(p);
                Thread.Sleep(500);
            }

            SkipSetupScreens();
        }

        protected abstract void SkipSetupScreens();

        public void Search(string search)
        {
            string uiHierarchy = adbCommandSender.GetUIHierarchy();

            var searchAttribute = xmlAttributes["SEARCH_V1"];
            if (adbCommandSender.IsElementExist(searchAttribute, uiHierarchy))
            {
                var p = adbCommandSender.GetElementTapPoint(searchAttribute, uiHierarchy);
                adbCommandSender.Tap(p);
                Thread.Sleep(500);
            }
            else
            {
                searchAttribute = xmlAttributes["SEARCH_V2"];
                var p = adbCommandSender.GetElementTapPoint(searchAttribute, uiHierarchy);
                adbCommandSender.Tap(p);
                Thread.Sleep(500);
            }

            adbCommandSender.ClearEditField(searchAttribute, uiHierarchy);
            adbCommandSender.InputText(search);
            adbCommandSender.Keyevent(KeyCode.KEYCODE_ENTER);
            Thread.Sleep(500);

            if (adbCommandSender.IsElementExist(xmlAttributes["CHECK_INFOBAR_POPUP"]))
            {
                adbCommandSender.TapOn(xmlAttributes["CLOSE_INFOBAR_POPUP"]);
                Thread.Sleep(500);
            }

            while (adbCommandSender.IsElementExist(xmlAttributes["CHECK_AGREEMENT_POPUP"]))
            {
                uiHierarchy = adbCommandSender.GetUIHierarchy();

                adbCommandSender.TapOn(xmlAttributes["SCROLL_AGREEMENT_POPUP"], uiHierarchy);
                Thread.Sleep(500);

                adbCommandSender.TapOn(xmlAttributes["CLOSE_AGREEMENT_POPUP"], uiHierarchy);
                Thread.Sleep(500);
            }
        }

        public string GetIp()
        {
            const string ipXPath = "node[1]/node[1]";

            string file = adbCommandSender.GetUIHierarchy();

            var ipBlock = adbCommandSender.GetXmlElement(xmlAttributes["IP_BLOCK"], file);

            if (ipBlock == null)
            {
                throw new Exception("Did not find ip block. Probably visualization problems");
            }

            var node = ipBlock.XPathSelectElement(ipXPath);

            if (node == null)
            {
                throw new Exception("Incorrect xPath to ip block");
            }

            var attribute = node.Attribute("text");

            if (attribute == null)
            {
                throw new Exception("Incorrect xPath to ip block");
            }

            return attribute.Value;
        }
    }
}
