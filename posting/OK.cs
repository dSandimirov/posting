using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace posting
{
    internal class OK : IZennoExternalCode
    { 
        // common
        public static void Logininng(Instance instance, IZennoPosterProjectModel project, string login, string password, int delay)
        {
            HtmlElement he;

            // login
            he = instance.ActiveTab.FindElementByAttribute("input:text", "id", "field_email", "regexp", 0);
            CommonCode.SetStringParameterOnPage(instance, he, login);
            // password
            he = instance.ActiveTab.FindElementByAttribute("input:password", "id", "field_password", "regexp", 0);
            CommonCode.SetStringParameterOnPage(instance, he, password);
            // click
            he = instance.ActiveTab.FindElementByAttribute("input:submit", "value", "Войти в Одноклассники", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            Thread.Sleep(delay*1000);
        }
        public static void CheckLogining(Instance instance, IZennoPosterProjectModel project, string login, out bool logining)
        {
            logining = false;
            Tab tab = instance.ActiveTab;
            if (tab.IsBusy) tab.WaitDownloading();
            string pageText = tab.PageText;

            string[] searchTextUnlogin =
            {
                @"Не получается войти?",
                @"Can't log in?"
            };

            for (int i = 0; i < searchTextUnlogin.Length; i++)
            {
                var pattern = new Regex(searchTextUnlogin[i]);
                var match = pattern.Match(pageText);
                if ((match.Value) != "")
                {
                    logining = false;
                }
            }

            string[] searchTextLogin =
{
                @"Напишите заметку",
                @"лента"
            };

            for (int i = 0; i < searchTextLogin.Length; i++)
            {
                var pattern = new Regex(searchTextLogin[i]);
                var match = pattern.Match(pageText);
                if ((match.Value) != "")
                {
                    logining = true;
                }
            }

            if (logining) project.SendInfoToLog(login + " -> logining", true);
            else project.SendWarningToLog(login + " -> unlogining", true);
        }
        public static void Capcha(Instance instance, IZennoPosterProjectModel project, string login, string password, int delay)
        {

            instance.LoadPictures = true;
            Thread.Sleep(delay * 1000);

            HtmlElement he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildById("captcha");
            if (he.IsVoid)
            {
                he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("img", "src", "https://ok.ru/captcha\\?st.cmd=captcha&1641594215345&1641594217362&1641594217933", "regexp", 0);
            }
            if (he.IsVoid)
            {
                he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("img", "fulltag", "img", "text", 0);
            }
            if (he.IsVoid)
            {
                instance.LoadPictures = false;
                project.SendInfoToLog(login + " -> no capcha");
                return;
            }
            // send capcha
            string recognition = ZennoPoster.CaptchaRecognition("Anti-Captcha.dll", he.DrawToBitmap(false), "");
            // disassemble capcha
            string captcha;
            var splitters = "-|-".ToCharArray();
            string[] temp_arr = recognition.Split(splitters);
            if (temp_arr[0].Length != 0)
            {
                captcha = temp_arr[0];
                project.SendWarningToLog(login + " -> captcha -> " + captcha, true);

                // inter capcha
                he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildById("field_code");
                if (he.IsVoid)
                {
                    he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByName("st.ccode");
                }
                if (he.IsVoid)
                {
                    he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("input:text", "class", "it", "regexp", 0);
                }
                if (he.IsVoid)
                {
                    he = instance.GetTabByAddress("page").GetDocumentByAddress("0").FindElementByTag("form", 0).FindChildByAttribute("input:text", "fulltag", "input:text", "text", 0);
                }
                if (he.IsVoid) return;

                instance.WaitFieldEmulationDelay();

                he.SetValue(captcha, instance.EmulationLevel, false);

                Logininng(instance, project, login, password, 1);
            }
            else
            {
                project.SendInfoToLog(login + " -> no capcha");
                return;
            }

        }
        
        // user
        public static bool CheckBadUser(Instance instance, IZennoPosterProjectModel project, string login)
        {
            bool result = false;
            Tab tab = instance.ActiveTab;
            if (tab.IsBusy) tab.WaitDownloading();
            string pageText = tab.PageText;

            string[] searchText = { @"Неправильно указан", @"профиль заблокирован", @"Доступ к профилю ограничен", @"Профиль удалён по просьбе" };

            for (int i = 0; i < searchText.Length; i++)
            {
                var pattern = new Regex(searchText[i]);
                var match = pattern.Match(pageText);
                if ((match.Value) == searchText[i])
                {
                    result = true;
                    break;
                }
            }

            project.SendInfoToLog(login + " -> check user");
            return result;
        }
        public static void WriteBadUser(IZennoPosterProjectModel project, string settings, string pathSettings, string user, string login, string tematika, string proxy)
        {
            if (settings[0] != '#')
            {
                // write settings
                string path = pathSettings;
                StreamReader reader = new StreamReader(path);
                string content = reader.ReadToEnd();
                reader.Close();

                int index = content.IndexOf(user);
                content = content.Remove(index, user.Length);
                content = content.Insert(index, "#" + user);

                StreamWriter writer = new StreamWriter(path);
                writer.Write(content);
                writer.Close();
                // write bad_user.txt
                string dateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                File.AppendAllText(project.Directory + @"\bad_user.txt", dateTime + " -> " + tematika + " -> " + user + " -> " + proxy + Environment.NewLine);
            }

            project.SendErrorToLog(login + " -> new bad user", true);

            // max thread - 1
            var id = Guid.Parse(project.TaskId);
            ZennoPoster.SetMaxThreads(id, ZennoPoster.GetThreadsCount(id) - 1);

            // stop
            if (ZennoPoster.GetThreadsCount(id) == 1)
            {
                ZennoPoster.StopTask(id);
            }
        }
        
        // group
        public static string GetGroup(IZennoPosterProjectModel project, List<string> list, string login)
        {
            string element = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                element = list.ElementAt(0);
                if (element[0] == '#')
                {
                    project.SendErrorToLog(login + " -> " + element + " -> bad group", true);
                    list.RemoveAt(0);
                }
                else
                {
                    list.Add(list.ElementAt(0));
                    list.Remove(list.ElementAt(0));
                    project.SendInfoToLog(login + " -> " + element + " -> get group");
                    break;
                }
            }

            if (list.Count == 0)
            {
                throw new Exception(login + " -> all group blocked");
            }

            return element;
        }
        public static bool CheckGroup(Instance instance, IZennoPosterProjectModel project, string url, string group)
        {
            bool result = false;
            List<string> list = new List<string>();

            list.Clear(); list.Add(@"О группе");
            CommonCode.GoUrl(instance, project, url, 5, list);

            list.Clear(); list.Add(@"Группа заблокирована"); list.Add(@"Этой страницы нет");
            result = CommonCode.SearchTextOnPage(instance, list);

            return result;
        }
        public static void WriteBadGroup(IZennoPosterProjectModel project, string settings, string pathSettings, string user, string login, string tematika, string proxy, string group, List<string> list)
        {
            if (settings[0] != '#')
            {
                StreamReader reader = new StreamReader(pathSettings);
                string content = reader.ReadToEnd();
                reader.Close();

                int index = content.IndexOf(group);
                content = content.Remove(index, group.Length);
                content = content.Insert(index, "#" + group);

                StreamWriter writer = new StreamWriter(pathSettings);
                writer.Write(content);
                writer.Close();

                list.Remove(group);

                // write bad_user.txt
                string dateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                File.AppendAllText(project.Directory + @"\bad_groups.txt", dateTime + " -> " + tematika + " -> " + user + " -> " + proxy + group + Environment.NewLine);

                project.SendErrorToLog(login + " -> " + group + " -> new bad group", true);
            }
        }
        
        // walking
        public static void Messages(Instance instance, IZennoPosterProjectModel project, string login)
        {
            HtmlElement he;

            he = instance.ActiveTab.FindElementByAttribute("li", "data-l", "t,messages", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            int kol = instance.ActiveTab.FindElementsByAttribute("msg-chat-notification-bubble", "chat", @"{}", "regexp").Count;
            int randomSelect = CommonCode.RandomInt(0, kol);

            he = instance.ActiveTab.FindElementByAttribute("msg-chat-notification-bubble", "chat", @"{}", "regexp", randomSelect);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            project.SendInfoToLog(login + " -> message");
        }
        public static void Friends (Instance instance, IZennoPosterProjectModel project, string login)
        {
            HtmlElement he;

            he = instance.ActiveTab.FindElementByAttribute("li", "data-l", "t,guests", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            he = instance.ActiveTab.FindElementByAttribute("a", "href", @"friendRequests", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            int kol = instance.ActiveTab.FindElementsByAttribute("span", "innertext", "Принять", "regexp").Count;
            int randomSelect = CommonCode.RandomInt(0, kol);
            he = instance.ActiveTab.FindElementByAttribute("span", "innertext", "Принять", "regexp", randomSelect);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            project.SendInfoToLog(login + " -> frend");
        }
        public static void Developments(Instance instance, IZennoPosterProjectModel project, string login)
        {
            HtmlElement he;

            he = instance.ActiveTab.FindElementByAttribute("li", "data-l", "t,marks", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            project.SendInfoToLog(login + " -> developments");
        }
        public static void Discussions(Instance instance, IZennoPosterProjectModel project, string login)
        {
            HtmlElement he;

            he = instance.ActiveTab.FindElementByAttribute("li", "data-l", "t,discussions", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            int kol = instance.ActiveTab.FindElementsByAttribute("div", "class", @"counterText", "regexp").Count;
            int random_select = CommonCode.RandomInt(0, kol);

            he = instance.ActiveTab.FindElementByAttribute("div", "class", @"counterText", "regexp", random_select);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            project.SendInfoToLog(login + " -> discussions");
        }
        public static void Klasser(Instance instance, IZennoPosterProjectModel project, string login)
        {
            List<string> examinations = new List<string>();
            examinations.Clear(); examinations.Add(@"Напишите заметку");
            CommonCode.GoUrl(instance, project, "Http://ok.ru", 2, examinations);

            int randomSelect = CommonCode.RandomInt(1, 5);

            HtmlElement he;

            for (int i = 0; i < randomSelect; i++)
            {
                int kolKlass = instance.ActiveTab.FindElementsByAttribute("span", "data-unlike-tx", "Класс", "regexp").Count;

                int currentSelect = CommonCode.RandomInt(1, kolKlass);
                he = instance.ActiveTab.FindElementByAttribute("span", "data-unlike-tx", "Класс", "regexp", currentSelect);

                if (!he.IsVoid)
                {
                    instance.WaitFieldEmulationDelay();

                    he.RiseEvent("click", instance.EmulationLevel);

                    project.SendInfoToLog(login + " -> class", true);
                }
                else
                {
                    project.SendWarningToLog(login + " -> class", true);
                }
            }
        }       

        
        int IZennoExternalCode.Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }
    }
}
