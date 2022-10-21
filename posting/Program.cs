using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
            string tematika = project.Variables["tematika"].Value;
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
            List<string> examinations = new List<string>();    
            // flags
            bool logining = false;
            bool profileStatus = false;
            // delays
            short commonDelay = 2;
            // Read and disassemble settings
            Settings.ReadSettings(project, pathSettings, out settingsStr);
            Settings.disassembleSettings(project, settingsStr, out user, out proxy, out groups);
            Settings.disassembleUser(project, user, out login, out password, out id);
            Settings.disassembleGroup(project, groups, out groupsList);

            // Instanse settings
            Settings.InstanceSettings(instance, project, login, proxy);

            // load profile and logining
            do
            {
                // Profile
                Profile.LoadProfile(project, enProfile, pathProfile, login, out profileStatus);
                examinations.Clear(); examinations.Add(@"Зарегистрироваться"); examinations.Add(@"Напишите заметку");
                CommonCode.GoUrl(instance, project, "Http://ok.ru", commonDelay, examinations);
                OK.CheckLogining(instance, project, login, out logining);
                if (!logining) {
                    // logining
                    do
                    {
                        examinations.Clear(); examinations.Add(@"Зарегистрироваться");
                        CommonCode.GoUrl(instance, project, "Http://ok.ru", commonDelay, examinations);
                        OK.Logininng(instance, project, login, password, commonDelay);
                        OK.CheckLogining(instance, project, login, out logining);
                        OK.CheckBadUser(instance, project, settingsStr, pathSettings, user, login, tematika, proxy);

                    } while (!logining);
                }
                Profile.SafeProfile(project, enProfile, pathProfile, login);
                profileStatus = true;
            } while (!profileStatus) ;

            return 0;
        }
    }
}