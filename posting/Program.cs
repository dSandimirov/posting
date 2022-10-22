using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime;
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
            // randoms
            int randomSelect = 0;
            // counters
            int amountGroup = 0;
            int groupCount = 0;
            // status
            bool userStatus = false;
            bool groupStatus = false;

            try
            {
                // Read and disassemble settings
                Settings.ReadSettings(project, pathSettings, out settingsStr);
                Settings.DisassembleSettings(project, settingsStr, out user, out proxy, out groups);
                Settings.DisassembleUser(project, user, out login, out password, out id);
                Settings.DisassembleGroup(project, groups, out groupsList);

                // Instanse settings
                Settings.InstanceSettings(instance, project, login, proxy);

                // load profile, logining and check user
                do
                {
                    // Profile
                    Profile.LoadProfile(project, enProfile, pathProfile, login, out profileStatus);
                    examinations.Clear(); examinations.Add(@"Зарегистрироваться"); examinations.Add(@"Напишите заметку");
                    CommonCode.GoUrl(instance, project, "https://ok.ru", commonDelay, examinations);
                    OK.CheckLogining(instance, project, login, out logining);
                    if (!logining)
                    {
                        // logining
                        do
                        {
                            examinations.Clear(); examinations.Add(@"Зарегистрироваться");
                            CommonCode.GoUrl(instance, project, "https://ok.ru", commonDelay, examinations);
                            OK.Logininng(instance, project, login, password, commonDelay);
                            OK.CheckLogining(instance, project, login, out logining);
                            userStatus = OK.CheckBadUser(instance, project, login);
                            if (userStatus) OK.WriteBadUser(project, settingsStr, pathSettings, user, login, tematika, proxy);
                        } while (!logining);
                    }
                    Profile.SafeProfile(project, enProfile, pathProfile, login);
                    profileStatus = true;
                } while (!profileStatus);

                // walking
                randomSelect = CommonCode.RandomInt(0, 5);
                Walking(instance, project, randomSelect, login);

                // posting
                amountGroup = groupsList.Count;
                if (amountGroup == 0)
                {
                    throw new Exception(login + " -> no group");
                }
                else
                {
                    string currentGroup = string.Empty;
                    // get group and check
                    do
                    {
                        currentGroup = OK.GetGroup(project, groupsList, login);
                        groupStatus = OK.CheckGroup(instance, project, currentGroup, login);
                        if (groupStatus) OK.WriteBadGroup(project, settingsStr,pathSettings, user, login, tematika, proxy, currentGroup, groupsList);
                    } while (groupStatus);
                    


                }
            } 
            catch (Exception e)
            {
                project.SendErrorToLog(e.ToString(), true);
            }          

            return 0;
        }

        public void Walking(Instance instance, IZennoPosterProjectModel project, int mode, string login)
        {
            switch (mode)
            {
                case 0:
                    OK.Messages(instance, project, login);
                    OK.Friends(instance, project, login);
                    OK.Developments(instance, project, login);
                    OK.Discussions(instance, project, login);
                    OK.Klasser(instance, project, login);
                    break;
                case 1:                    
                    OK.Friends(instance, project, login);                    
                    OK.Messages(instance, project, login);
                    OK.Discussions(instance, project, login);
                    OK.Developments(instance, project, login);
                    OK.Klasser(instance, project, login);
                    break;
                case 2:
                    OK.Messages(instance, project, login);
                    OK.Klasser(instance, project, login);
                    OK.Friends(instance, project, login);                    
                    OK.Discussions(instance, project, login);
                    OK.Developments(instance, project, login);
                    break;
                case 3:                    
                    OK.Friends(instance, project, login);
                    OK.Developments(instance, project, login);
                    OK.Discussions(instance, project, login);
                    OK.Klasser(instance, project, login);
                    OK.Messages(instance, project, login);
                    break;
                case 4:
                    OK.Developments(instance, project, login);
                    OK.Discussions(instance, project, login);
                    OK.Messages(instance, project, login);
                    OK.Friends(instance, project, login);                   
                    OK.Klasser(instance, project, login);
                    break;
                case 5:
                    OK.Messages(instance, project, login);
                    OK.Friends(instance, project, login);                    
                    OK.Klasser(instance, project, login);
                    OK.Developments(instance, project, login);
                    OK.Discussions(instance, project, login);
                    break;
                default:
                    OK.Discussions(instance, project, login);
                    OK.Developments(instance, project, login);
                    OK.Klasser(instance, project, login);
                    OK.Friends(instance, project, login);
                    OK.Messages(instance, project, login);
                    break;
            }            
        }

    }
}