using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace posting
{
    internal class Profile : IZennoExternalCode
    {
        public static void LoadProfile(IZennoPosterProjectModel project, string enProfile, string pathProfile, string login, out bool profileStatus)
        {
            profileStatus = false;
            if (enProfile == "True")
            {
                if (File.Exists(pathProfile))
                {
                    project.Profile.Load(pathProfile);
                    project.SendInfoToLog(login + " -> load profile", true);
                    profileStatus = true;
                }
                else
                {
                    project.SendWarningToLog(login + " -> unload profile", true);
                }
            }            
        }

        public static void SafeProfile(IZennoPosterProjectModel project, string enProfile, string pathProfile, string login)
        {
            project.Profile.Save(pathProfile, false, false, true, true, true, true, true, true, true);
            project.SendInfoToLog(login + " -> safe profile", true);
        }
 
        int IZennoExternalCode.Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }
    }
}
