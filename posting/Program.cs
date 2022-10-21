using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace posting
{
    public class Program : IZennoExternalCode
    {
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            // Input
            string settingsStr = string.Empty;
            string user = string.Empty;
            string proxy = string.Empty;
            string groups = string.Empty;
            // User
            string login = string.Empty;
            string password = string.Empty;
            string id = string.Empty;
            // Enable setting
            string enProfile = project.Variables["enProfile"].Value;
            // Path setting
            string pathSettings = project.Variables["pathSettings"].Value;
            string pathProfile = project.Directory + @"\profile\" + login + ".zpprofile";
            // Lists
            List<string> groupsList = new List<string>();
            // Objects
            Settings settings = new Settings(instance, project);
            Profile profile = new Profile(instance, project);
            // Read and disassemble settings
            settings.ReadSettings(project, pathSettings, out settingsStr);
            settings.disassembleSettings(project, settingsStr, out user, out proxy, out groups);
            settings.disassembleUser(project, user, out login, out password, out id);
            settings.disassembleGroup(project, groups, out groupsList);
            // Instanse settings
            settings.InstanceSettings(instance,project, login, proxy);
            // Profile
            profile.LoadProfile(project, enProfile, login);

            //GoUrl(instance, "Http://ok.ru");


            return 0;
        }

        public void GoUrl(Instance instance, string url)
        {
            Tab tab = instance.ActiveTab;
            if ((tab.IsVoid) || (tab.IsNull)) throw new Exception("error open page " + url);
            if (tab.IsBusy) tab.WaitDownloading();
            tab.Navigate(url, "");
            if (tab.IsBusy) tab.WaitDownloading();
        }

    }
}