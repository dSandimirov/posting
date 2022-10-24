using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
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

                Thread.Sleep(delay * 1000);

                // project.SendInfoToLog(login + " -> logining");
            }
            catch
            {
                project.SendErrorToLog(login + " -> logining");
                throw new Exception();
            }
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
        public static string GetGroup(IZennoPosterProjectModel project, List<string> list,  string login)
        {
            string currentGroup = string.Empty;
            for (int i = 0; i < list.Count; i++)
            {
                currentGroup = list.ElementAt(0);
                if (currentGroup[0] == '#')
                {
                    project.SendErrorToLog(login + " -> " + currentGroup + " -> bad group", true);
                    list.RemoveAt(0);
                }
                else
                {
                    project.SendInfoToLog(login + " -> " + currentGroup + " -> get group");
                    break;
                }
            }

            if (list.Count == 0)
            {
                throw new Exception(login + " -> all group blocked");
            }

            return currentGroup;
        }
        public static bool CheckGroup(Instance instance, IZennoPosterProjectModel project, string url, string group, string login)
        {
            bool result = false;
            List<string> list = new List<string>();

            // list.Clear(); list.Add(@"О группе");
            CommonCode.GoUrl(instance, project, url, 5, login);

            list.Clear(); list.Add(@"Группа заблокирована"); list.Add(@"Этой страницы нет");
            result = CommonCode.SearchTextOnPage(instance, list);

            project.SendInfoToLog(login + " -> " + group + " -> check group");

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
        public static string LastPost(Instance instance, IZennoPosterProjectModel project, string group, string login)
        {
            try
            {
                string postLastTime = string.Empty;

                HtmlElement he;
                he = instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("div", "class", "feed_date", "regexp", 0);

                if (!he.IsVoid) postLastTime = instance.ActiveTab.GetDocumentByAddress("0").FindElementByAttribute("div", "class", "feed_date", "regexp", 0).GetAttribute("innerhtml").ToString();
                else postLastTime = "noPost";

                project.SendInfoToLog(login + " -> " + group + " -> " + postLastTime + " -> last post", true);
                return postLastTime;
            }
            catch
            {
                project.SendErrorToLog(login + " -> " + group + " -> " + "error" + " -> last post", true);
                return string.Empty;
                throw new Exception();
            }
        }
        public static bool GoPost(Instance instance, IZennoPosterProjectModel project, string postLastTime, short directPostDelay, string group, string login)
        {
            if (postLastTime == "noPost")
            {
                project.SendWarningToLog(login + " -> " + group + " -> no post", true);
                return true;
            }            
            if (postLastTime.Contains("вчера"))
            {
                project.SendInfoToLog(login + " -> " + group +" -> " + " esterday", true);
                return true;
            }            
            if (postLastTime.Contains(":"))
            {
                DateTime time = DateTime.Now;
                DateTime postLastTimeDt = DateTime.Parse(postLastTime);
                if (time < postLastTimeDt.AddMinutes(Convert.ToDouble(directPostDelay)))
                {
                    project.SendInfoToLog(login + " -> " + group + " -> " + postLastTime + " -> early", true);
                    return false;
                }
                else
                {
                    project.SendInfoToLog(login + " -> " + group + " -> " + postLastTime + " -> post time", true);
                    return true;
                }
            }
            project.SendWarningToLog(login + " -> " + group + " -> " + postLastTime + " -> long time", true);
            return true;
        }

        // posting
        public static void DeleteDraft(Instance instance, IZennoPosterProjectModel project,string group, string login)
        {
            HtmlElement he = instance.ActiveTab.FindElementByAttribute("div", "class", @"posting_draft-btn", "regexp", 0);
            if (he.IsVoid || he.IsNull) return;
            else
            {
                CommonCode.ClickCoordinate(instance, 2, -1, he);
                project.SendInfoToLog(login + " -> " + group + " -> delete draft", true);
            }
        }
        public static void PastLink(Instance instance, IZennoPosterProjectModel project, string path, string enPastLink, string group, string login)
        {
            if (enPastLink == "True")
            {
                List<string> listLinks = new List<string>();
                listLinks = File.ReadLines(path).ToList();
                int randomSelect = CommonCode.RandomInt(0, listLinks.Count - 1);

                string currentLink = listLinks[randomSelect];

                HtmlElement he = instance.ActiveTab.FindElementByAttribute("wysiwyg:div", "fulltagname", "wysiwyg:div", "regexp", 1);
                instance.EmulationLevel = "None";
                CommonCode.SetStringParameterOnPage(instance, he, currentLink);
                instance.EmulationLevel = "SuperEmulation";

                project.SendInfoToLog(login + " -> " + group + " -> " + currentLink, true);
            }
        }
        public static void PastImage(Instance instance, IZennoPosterProjectModel project, string path, string group, string login)
        {
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            int randomSelect = CommonCode.RandomInt(0, files.Length - 1);

            instance.SetFileUploadPolicy("ok", "");
            instance.SetFilesForUpload(files[randomSelect]);            

            HtmlElement he = instance.ActiveTab.FindElementByAttribute("div", "title", "Добавить фото", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            he = instance.ActiveTab.FindElementByAttribute("input:file", "class", "input-file-input", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            he = instance.ActiveTab.FindElementByAttribute("wysiwyg:div", "fulltag", "wysiwyg:div", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 2, -1, he);

            Thread.Sleep(10000);

            project.SendInfoToLog(login + " -> " + group + " -> " + files[randomSelect], true);
        }
        public static void PastComment(Instance instance, IZennoPosterProjectModel project, string path, string enPastComment, string group, string login)
        {
            if (enPastComment == "True")
            {
                List<string> list = new List<string>();
                list = File.ReadLines(path).ToList();
                int randomSelect = CommonCode.RandomInt(0, list.Count - 1);

                string current = list[randomSelect];

                HtmlElement he = instance.ActiveTab.FindElementByAttribute("wysiwyg:div", "fulltagname", "wysiwyg:div", "regexp", 0);
                instance.EmulationLevel = "None";
                CommonCode.SetStringParameterOnPage(instance, he, current);
                instance.EmulationLevel = "SuperEmulation";

                project.SendInfoToLog(login + " -> " + group + " -> " + current, true);
            }
        }
        public static void PublishPost(Instance instance, IZennoPosterProjectModel project, string group, string login)
        {
            HtmlElement he = instance.ActiveTab.FindElementByAttribute("div", "class", "posting_submit", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 10, -1, he);

            string currentDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            project.SendInfoToLog(login + " -> " + group + " -> " + "publich post", true);
        }
        public static int AmountDifferadPost(Instance instance, IZennoPosterProjectModel project, string group, string login)
        {
            int kol = 0;
            Tab tab = instance.ActiveTab;
            if (tab.IsBusy) tab.WaitDownloading();
            string domText = tab.DomText;
            if (tab.IsBusy) tab.WaitDownloading();

            var pattern = new Regex(@"(?<=<div\ class=""nav-side_tx\ ellip"">Отложенные\ <span\ class=""lstp-t""><span>).*?(?=</span>)");
            var match = pattern.Match(domText);
            if ((match.Value) != "") kol = Convert.ToInt32(match.Value);

            return kol;
        }
        public static string TimeDifferedPost(Instance instance, IZennoPosterProjectModel project, string group, string login)
        {
            string result = string.Empty;
            HtmlElement he = instance.ActiveTab.FindElementByAttribute("a", "innerhtml", @"По дате создания", "regexp", 0);
            if (!he.IsNull) CommonCode.ClickCoordinate(instance, 1, -1, he);

            Tab tab = instance.ActiveTab;
            if (tab.IsBusy) tab.WaitDownloading();
            string domText = tab.DomText;
            if (tab.IsBusy) tab.WaitDownloading();

            var pattern = new Regex(@"(?<=publicationDate""\ class=""group-delay-toggler_tx"">).*(?=</span>)");
            var match = pattern.Match(domText);
            if ((match.Value) != "") result = match.Value;
            else
            {
                result = DateTime.Now.ToString("HH:mm");
                
            }

            project.SendInfoToLog(login + " -> " + group + " -> " + result + " -> time last post");

            return result;
        }
        public static void SetDifferedTime(Instance instance, IZennoPosterProjectModel project, string timeDifferadPost, string group, string login)
        {
            HtmlElement he = instance.ActiveTab.FindElementByAttribute("span", "innerHtml", "Время публикации", "regexp", 0);
            CommonCode.ClickCoordinate(instance, 1, -1, he);

            instance.EmulationLevel = "None";

            DateTime dtCurrent = DateTime.Now;

            he = instance.ActiveTab.FindElementByAttribute("input:text", "name", "st.layer.date", "regexp", 0);
            CommonCode.SetStringParameterOnPage(instance, he, dtCurrent.ToShortDateString());

            DateTime dtLast = DateTime.Parse(timeDifferadPost);
            DateTime dtNew = dtLast.AddHours(1);

            he = instance.ActiveTab.FindElementByAttribute("select", "name", "st.layer.hours", "regexp", 0);
            switch (dtNew.ToString("HH"))
            {
                case "01":
                    CommonCode.SetStringParameterOnPage(instance, he, "1");
                    break;
                case "02":
                    CommonCode.SetStringParameterOnPage(instance, he, "2");
                    break;
                case "03":
                    CommonCode.SetStringParameterOnPage(instance, he, "3");
                    break;
                case "04":
                    CommonCode.SetStringParameterOnPage(instance, he, "4");
                    break;
                case "05":
                    CommonCode.SetStringParameterOnPage(instance, he, "5");
                    break;
                case "06":
                    CommonCode.SetStringParameterOnPage(instance, he, "6");
                    break;
                case "07":
                    CommonCode.SetStringParameterOnPage(instance, he, "7");
                    break;
                case "08":
                    CommonCode.SetStringParameterOnPage(instance, he, "8");
                    break;
                case "09":
                    CommonCode.SetStringParameterOnPage(instance, he, "9");
                    break;
                default:
                    CommonCode.SetStringParameterOnPage(instance, he, dtNew.ToString("HH"));
                    break;
            }

            instance.EmulationLevel = "SuperEmulation";

            project.SendInfoToLog(login + " -> " + group + " -> " + dtNew.ToString("HH:mm") + " -> set differed time");
        }

        // walking
        public static void Messages(Instance instance, IZennoPosterProjectModel project, string login)
        {
            try
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
            catch
            {
                project.SendErrorToLog(login + " -> message");
                return;
            }
        }
        public static void Friends (Instance instance, IZennoPosterProjectModel project, string login)
        {            
            try
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
            catch
            {
                project.SendErrorToLog(login + " -> frend");
                return;
            }
        }
        public static void Developments(Instance instance, IZennoPosterProjectModel project, string login)
        {
            try
            {
                HtmlElement he;

                he = instance.ActiveTab.FindElementByAttribute("li", "data-l", "t,marks", "regexp", 0);
                CommonCode.ClickCoordinate(instance, 2, -1, he);

                project.SendInfoToLog(login + " -> developments");
            }
            catch
            {
                project.SendErrorToLog(login + " -> developments");
                return;
            }
        }
        public static void Discussions(Instance instance, IZennoPosterProjectModel project, string login)
        {
            try
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
            catch
            {
                project.SendErrorToLog(login + " -> discussions");
                return;
            }
        }
        public static void Klasser(Instance instance, IZennoPosterProjectModel project, string login)
        {
            try
            {
                CommonCode.GoUrl(instance, project, "http://ok.ru", 2, login);

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
            catch
            {
                project.SendErrorToLog(login + " -> class", true);
                return;
            }
        }   

        
        int IZennoExternalCode.Execute(Instance instance, IZennoPosterProjectModel project)
        {
            throw new NotImplementedException();
        }
    }
}
