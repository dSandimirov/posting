using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace posting
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        public object SyncObject { get; private set; }

        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            string settings = string.Empty;

            Settings set = new Settings(instance, project);
            set.ReadSettings(project.Variables["pathSettings"].Value, out settings);

            //ReadSettings(project.Variables["pathSettings"].Value, out settings);
            project.SendInfoToLog("read settings", true);

            //InstanceSettings(instance, project);
            //GoUrl(instance, "Http://ok.ru");

            project.SendInfoToLog("disassembyly settings", true);
            project.SendInfoToLog("disassembyly user", true);



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