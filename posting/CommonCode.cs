using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;
using ZennoLab.InterfacesLibrary.Enums;
using ZennoLab.InterfacesLibrary.Instance;
using ZennoLab.Macros;
using Global.ZennoExtensions;
using ZennoLab.Emulation;
using System.Diagnostics.Eventing.Reader;

namespace posting
{
    public class CommonCode: IZennoExternalCode
    {
        // go url + examination
        public static void GoUrlExm(Instance instance, IZennoPosterProjectModel project, string url, short delay, List<string> examination)
        {
            bool pageLoad = false;
            Tab tab = instance.ActiveTab;
            if ((tab.IsVoid) || (tab.IsNull)) throw new Exception("error load page");
            if (tab.IsBusy) tab.WaitDownloading();
            tab.Navigate(url, "google.ru");
            if (tab.IsBusy) tab.WaitDownloading();

            // examination
            do
            {
                pageLoad = SearchTextOnPage(instance, examination);
                if (!pageLoad)
                {
                    project.SendWarningToLog(url + " no load");
                    Thread.Sleep(delay * 1000);
                }
            } while (!pageLoad);            
        }
        // go url
        public static void GoUrl(Instance instance, IZennoPosterProjectModel project, string url, short delay, string login)
        {
            Tab tab = instance.ActiveTab;
            if ((tab.IsVoid) || (tab.IsNull)) throw new Exception("error load page");
            if (tab.IsBusy) tab.WaitDownloading();
            tab.Navigate(url, "google.ru");
            if (tab.IsBusy) tab.WaitDownloading();

            project.SendInfoToLog(login + " -> " + url, true);
        }
        // click coordinate
        public static void ClickCoordinate(Instance instance, int indent, int sizeOfType, HtmlElement he)
        {
            // delay
            instance.WaitFieldEmulationDelay();
            if (he.IsVoid || he.IsNull) throw new Exception("error click coordinate");
            // leftInbrowser
            int leftInbrowser = Convert.ToInt32(he.GetAttribute("leftInbrowser"));
            // topInbrowser
            int topInbrowser = Convert.ToInt32(he.GetAttribute("topInbrowser"));
            // width
            int width = Convert.ToInt32(he.GetAttribute("clientwidth"));
            // height
            int height = Convert.ToInt32(he.GetAttribute("clientheight"));
            // focus
            he.RiseEvent("onmouseover", instance.EmulationLevel);
            // pointing
            instance.ActiveTab.FullEmulationMouseMoveAboveHtmlElement(he, sizeOfType);
            // delay
            instance.WaitFieldEmulationDelay();
            // click
            instance.Click(leftInbrowser + indent, leftInbrowser + width - indent, topInbrowser + indent, topInbrowser + height - indent, "Left", "Random");
            // delay
            Thread.Sleep(2000);
        }
        // set parametr
        public static void SetStringParameterOnPage(Instance instance, HtmlElement he, string parametr)
        {
            instance.WaitFieldEmulationDelay();
            if (he.IsVoid || he.IsNull) throw new Exception("error set parametr");
            instance.WaitFieldEmulationDelay();
            he.SetValue(parametr, instance.EmulationLevel, false);
        }
        // random int
        public static int RandomInt(int min, int max)
        {
            int randomSelect = 0;
            if (max != 0)
            {
                Thread.Sleep(500);
                Random rnd = new Random();
                randomSelect = rnd.Next(min, max);
            }
            else
                randomSelect = -1;
            return randomSelect;
        }
        // random str
        public static string RandomStr(int lenght)
        {
            Random rnd = new Random();
            string randomStr = "";
            string symbols = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < lenght; i++)
            {
                char ch = symbols[rnd.Next(0, symbols.Length)];
                randomStr += ch;
            }
            return randomStr;
        }
        // random date
        public static string RandomDate()
        {
            Random rnd = new Random();
            var startDate = new DateTime(rnd.Next(1988, 2010), 1, 1);
            Console.WriteLine(startDate.ToString("yyyy.dd.MM"));
            var newDate = startDate.AddDays(rnd.Next(366));
            return newDate.ToString("yyyy:dd:MM " + rnd.Next(10, 23) + ":" + rnd.Next(10, 59) + ":" + rnd.Next(10, 59));
        }
        // seqrch text on page
        public static bool SearchTextOnPage(Instance instance, List<string> searchTextList)
        {
            bool status = false;
            Tab tab = instance.ActiveTab; 

            for (int i = 0; i < searchTextList.Count; i++)
            {
                string pageText = tab.PageText;
                var pattern = new Regex(searchTextList.ElementAt(i));
                var match = pattern.Match(pageText);
                if ((match.Value) == searchTextList.ElementAt(i))
                {
                    status = true;
                    break;
                }
                else continue;
            }
            return status;
        }

        int IZennoExternalCode.Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }
    }
}
