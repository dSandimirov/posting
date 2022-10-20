using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.IO;
using System.Diagnostics.Eventing.Reader;

namespace posting
{
    internal class Settings : IZennoExternalCode
    {
        public Settings(Instance instance, IZennoPosterProjectModel project)
        { 

        }

        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }

        public void ReadSettings(string pathSettings, out string settings)
        {
            if (File.Exists(pathSettings))
            {
                List<string> list = new List<string>();
                list = File.ReadLines(pathSettings).ToList();
                settings = list[0].ToString();
                list.Add(list[0]);
                list.RemoveAt(0);
                File.WriteAllLines(pathSettings, list);
            }
            else throw new Exception("error read file settings");
        }

        public void InstanceSettings(Instance instance, IZennoPosterProjectModel project, string nameInstance, string proxy)
        {
            instance.ClearCookie();
            instance.ClearCache();

            instance.DownloadActiveX = false;
            instance.DownloadFrame = true;
            instance.DownloadVideos = false;

            instance.IgnoreAdditionalRequests = true;
            instance.IgnoreAjaxRequests = true;
            instance.IgnoreFlashRequests = true;
            instance.IgnoreFrameRequests = true;

            instance.UseJavaApplets = true;
            instance.UseJavaScripts = true;
            instance.UsePlugins = false;
            instance.UseCSS = true;
            instance.UseMedia = false;
            instance.UseAdds = false;
            instance.UsePluginsForceWmode = false;

            instance.LoadPictures = false;
            instance.AllowPopUp = false;
            instance.RunActiveX = false;
            instance.BackGroundSoundsPlay = false;

            instance.MinimizeMemory();

            Random rnd = new Random();
            instance.SymbolEmulationDelay = rnd.Next(250, 500);
            instance.FieldEmulationDelay = rnd.Next(1000, 3000);
            instance.UseFullMouseEmulation = true;
            instance.EmulationLevel = "SuperEmulation";

            instance.SetWindowSize(1000, 800);

            instance.SetProxy(proxy);

            instance.AddToTitle(nameInstance);

            project.Profile.AcceptLanguage = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3";
        }

        public void disassembledSSettings(string settings, out string user, out string proxy, out string groups)
        {
            var splitters = "|".ToCharArray();
            string[] temp_arr = settings.Split(splitters);
            if (temp_arr[0].Length != 0) user = temp_arr[0];
            else throw new Exception("error user");
            if (temp_arr[1].Length != 0) proxy = temp_arr[1];
            else throw new Exception("error proxy");
            if (temp_arr[2].Length != 0) groups = temp_arr[2];
            else throw new Exception("error group list group");            
        }

        public void disassembledUser(string user, out string login, out string password, out string id)
        {
            var splitters = ":".ToCharArray();
            string[] temp_arr = user.Split(splitters);

            if (temp_arr[0].Length != 0) login = temp_arr[0];
            else throw new Exception("error login");
            if (temp_arr[1].Length != 0) password = temp_arr[1];
            else throw new Exception("error password");
            if (temp_arr[2].Length != 0) id = temp_arr[2];
            else throw new Exception("error id");

            
        }
    }

   
}
