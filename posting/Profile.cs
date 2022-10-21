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
        public Profile(Instance instance, IZennoPosterProjectModel project)
        {
        }

        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }

        public void LoadProfile(IZennoPosterProjectModel project, string enProfile, string login)
        {
            string path = project.Directory + @"\profile" + login + ".zpprofile";
            if (enProfile == "True")
            {
                if (File.Exists(path))
                {
                    project.Profile.Load(path);
                    project.SendInfoToLog(login + " -> load profile", true);
                }
                else
                {
                    CreateProfile(project, enProfile, login);
                }
            }            
        }

        public void SafeProfile(IZennoPosterProjectModel project, string enProfile, string login)
        {
            string path = project.Directory + @"\profile\" + login + ".zpprofile";
            project.Profile.Save(path, false, false, true, true, true, true, true, true, true);
            project.SendInfoToLog(login + " -> safe profile", true);
        }

        public void CreateProfile(IZennoPosterProjectModel project, string enProfile, string login)
        {
            string path = project.Directory + @"\profile\" + login + ".zpprofile";
            project.Profile.Save(path, false, false, true, true, true, true, true, true, true);
            project.SendInfoToLog(login + " -> create profile", true);
        }
    }
}
