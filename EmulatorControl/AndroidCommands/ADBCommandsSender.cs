using EmulatorControl.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace EmulatorControl.AndroidCommands
{
    class ADBCommandsSender
    {
        public static string? Device { get; set; }

        private static string SendCommandWithCmd(string command)
        {
            if(Device != null)
            {
                command = $"-s {Device} {command}";
            }

            using (var cmd = new Process())
            {
                cmd.StartInfo.FileName = "adb.exe";
                cmd.StartInfo.Arguments = command;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardOutput = true;

                cmd.Start();
                var result = cmd.StandardOutput.ReadToEnd();
                cmd.WaitForExit();

                return result;
            }
        }

        public static APILVL GetCurrentDeviceApiLvl()
        {
            string command = "shell getprop ro.build.version.sdk";
            return (APILVL)short.Parse(SendCommandWithCmd(command).Trim());
        }

        public static IEnumerable<string> GetDevices()
        {
            const string pattern = @"^([\-\w\d]+)\s+device";

            const string command = "devices";
            var result = SendCommandWithCmd(command);

            var strings = result.Split('\n');

            foreach (var s in strings)
            {
                Match match = Regex.Match(s, pattern);
                var device = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(device))
                    yield return device;
            }
        }

        #region Apps

        public IEnumerable<string> GetOpenApps()
        {
            const string pattern = @"topActivity=\{([\w.]+)/";

            const string command = "shell \"dumpsys activity | grep topActivity\"";
            var result = SendCommandWithCmd(command);
            var strings = result.Split('\n');

            foreach (var s in strings)
            {
                Match match = Regex.Match(s, pattern);
                var app = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(app))
                    yield return app;
            }
        }

        public string GetTopApp()
        {
            const string pattern = @":([\w.]+)/";

            const string command = "shell \"dumpsys activity | grep top-activity\"";
            var result = SendCommandWithCmd(command);

            Match match = Regex.Match(result, pattern);
            var app = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(app))
                return app;

            return string.Empty;
        }

        public bool KillApp(string appName)
        {
            string command = $"shell pm clear {appName}";
            string result = SendCommandWithCmd(command);
            return result.ToLower().Trim() == "success";
        }

        #endregion

        #region Xml

        public string GetUIHierarchy()
        {
            const string fileName = "view.xml";
            const string command = "exec-out uiautomator dump /dev/tty";
            string xml = SendCommandWithCmd(command);
            xml = xml.Replace("UI hierchary dumped to: /dev/tty", string.Empty);
            File.WriteAllText(fileName, xml);
            return fileName;
        }

        public XElement? GetXmlElement(XmlSearchAttribute xmlSearchAttribute, string xmlFile)
        {
            XDocument xmlDoc = XDocument.Load(xmlFile);

            return xmlDoc.Descendants()
                         .FirstOrDefault(x => x.Attribute(xmlSearchAttribute.Name)?.Value != null &&
                                              Regex.IsMatch(x.Attribute(xmlSearchAttribute.Name)?.Value, xmlSearchAttribute.ValuePattern));
        }

        public string GetXmlChildAttributeValue(XmlSearchAttribute xmlSearchAttribute, XElement parent, string attributeName)
        {
            XElement? child = parent.Elements()
                                    .FirstOrDefault(e => e.Attribute(xmlSearchAttribute.Name)?.Value != null &&
                                                         Regex.IsMatch(e.Attribute(xmlSearchAttribute.Name)?.Value, xmlSearchAttribute.ValuePattern));

            if (child == null)
            {
                throw new Exception($"Can not find any element with attribute {xmlSearchAttribute}");
            }

            var resultAttribute = child.Attribute(attributeName);
            if (resultAttribute == null)
            {
                throw new Exception($"Element with attribute {xmlSearchAttribute} has no property '{attributeName}'");
            }

            return resultAttribute.Value;
        }

        public bool IsElementExist(XmlSearchAttribute xmlSearchAttribute, string uiHierarchyFile)
        {
            var element = GetXmlElement(xmlSearchAttribute, uiHierarchyFile);
            return element != null;
        }

        public bool IsElementExist(XmlSearchAttribute xmlSearchAttribute)
        {
            string uiHierarchyFile = GetUIHierarchy();
            var element = GetXmlElement(xmlSearchAttribute, uiHierarchyFile);
            return element != null;
        }

        #endregion

        #region Input

        public Point GetElementTapPoint(XmlSearchAttribute xmlSearchAttribute, string uiHierarchyFile)
        {
            var element = GetXmlElement(xmlSearchAttribute, uiHierarchyFile);

            if (element == null)
            {
                throw new Exception($"Can not find any element with attribute {xmlSearchAttribute}");
            }

            var boundsAttribute = element.Attribute("bounds");
            if (boundsAttribute == null)
            {
                throw new Exception($"Element with attribute {xmlSearchAttribute} has no property 'bounds'");
            }

            // Parsing bounds attribute to Point
            const string boundPattern = @"\[(\d+),(\d+)\]\[(\d+),(\d+)\]";

            string bounds = boundsAttribute.Value;
            Match match = Regex.Match(bounds, boundPattern);

            if (match.Success && match.Groups.Count == 5)
            {
                int x1 = int.Parse(match.Groups[1].Value);
                int y1 = int.Parse(match.Groups[2].Value);
                int x2 = int.Parse(match.Groups[3].Value);
                int y2 = int.Parse(match.Groups[4].Value);

                // Calculate element center point
                return new Point((x1 + x2) / 2, (y1 + y2) / 2);
            }

            throw new Exception($"Failed to parse property 'bounds'");
        }

        public void Tap(Point bounds)
        {
            string command = $"shell input tap {bounds.X} {bounds.Y}";
            SendCommandWithCmd(command);
        }

        public void TapOn(XmlSearchAttribute xmlSearchAttribute)
        {
            string uiHierarchy = GetUIHierarchy();
            var p = GetElementTapPoint(xmlSearchAttribute, uiHierarchy);
            Tap(p);
        }

        public void TapOn(XmlSearchAttribute xmlSearchAttribute, string uiHierarchy)
        {
            var p = GetElementTapPoint(xmlSearchAttribute, uiHierarchy);
            Tap(p);
        }

        public void InputText(string text)
        {
            text = text.Trim().Replace(" ", "\\ ");
            string command = $"shell input text \"{text}\"";
            SendCommandWithCmd(command);
        }

        public void Keyevent(KeyCode key)
        {
            string command = $"shell input keyevent {key}";
            SendCommandWithCmd(command);
        }

        public void ClearEditField(XmlSearchAttribute xmlSearchAttribute, string uiHierarchyFile)
        {
            var element = GetXmlElement(xmlSearchAttribute, uiHierarchyFile);

            if (element == null)
            {
                throw new Exception($"Can not find any element with attribute {xmlSearchAttribute}");
            }

            var textAttribute = element.Attribute("text");
            if (textAttribute == null)
            {
                throw new Exception($"Element with attribute {xmlSearchAttribute} has no property 'text'");
            }

            string text = textAttribute.Value;

            for (int i = 0; i < text.Length; i++)
            {
                Keyevent(KeyCode.KEYCODE_DEL);
            }
        }

        #endregion

        public string Screenshot()
        {
            const string fileName = "screen.png";

            string command = $"exec-out screencap -p > ./{fileName}";
            SendCommandWithCmd(command);

            return fileName;
        }
    }
}
