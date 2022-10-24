using System;
using System.Collections.Generic;
using System.IO;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace posting
{
    public class Program : IZennoExternalCode
    {
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            // input
            string settingsStr = string.Empty;
            string user = string.Empty;
            string proxy = string.Empty;
            string groups = string.Empty;
            string tematika = project.Variables["tematika"].Value;
            // user
            string login = string.Empty;
            string password = string.Empty;
            string id = string.Empty;
            // enable setting
            string enProfile = project.Variables["enProfile"].Value;
            string enPastLink = project.Variables["enPasteLinks"].Value;
            string enPastComment = project.Variables["enPasteComments"].Value;
            // path setting
            string pathSettings = project.Variables["pathSettings"].Value;
            string pathLink = project.Variables["pathLink"].Value;
            string pathImage = project.Variables["pathPosts"].Value;
            string pathComment = project.Variables["pathComments"].Value;
            // lists
            List<string> groupsList = new List<string>();
            // flags
            bool logining = false;
            bool profileStatus = false;
            // delays
            short commonDelay = 2;
            short timeOffsetDirectPost = Convert.ToInt16(project.Variables["timeOffsetDirectPost"].Value);
            // randoms
            int randomSelect = 0;
            // counters
            int amountGroup = 0;
            // status
            bool userStatus = false;
            bool groupStatus = false;

            int minDifferedPost = Convert.ToInt32(project.Variables["minDelayedPosts"].Value);
            int kolDifferedPost = 0;
            try
            {
                // read and disassemble settings
                Settings.ReadSettings(project, pathSettings, out settingsStr);
                Settings.DisassembleSettings(project, settingsStr, out user, out proxy, out groups);
                Settings.DisassembleUser(project, user, out login, out password, out id);
                Settings.DisassembleGroup(project, groups, out groupsList);

                string pathProfile = project.Directory + @"\profile\" + login + ".zpprofile";

                // instanse settings
                Settings.InstanceSettings(instance, project, login, proxy);

                // load profile, logining and check user
                do
                {
                    try
                    {
                        // profile
                        Profile.LoadProfile(project, enProfile, pathProfile, login, out profileStatus);
                        CommonCode.GoUrl(instance, project, "https://ok.ru", commonDelay, login);
                        OK.CheckLogining(instance, project, login, out logining);
                        if (!logining)
                        {
                            // logining
                            do
                            {
                                CommonCode.GoUrl(instance, project, "https://ok.ru", commonDelay, login);
                                OK.Logininng(instance, project, login, password, commonDelay);
                                OK.CheckLogining(instance, project, login, out logining);
                                userStatus = OK.CheckBadUser(instance, project, login);
                                if (userStatus) OK.WriteBadUser(project, settingsStr, pathSettings, user, login, tematika, proxy);
                            } while (!logining);
                        }
                        Profile.SafeProfile(project, enProfile, pathProfile, login);
                        if (File.Exists(pathProfile)) profileStatus = true;
                    }
                    catch
                    {
                        continue;
                    }
                } while (!profileStatus);

                // walking
                randomSelect = CommonCode.RandomInt(0, 5);
                Walking(instance, project, randomSelect, login);

                // preparation
                amountGroup = groupsList.Count;
                if (amountGroup == 0)
                {
                    throw new Exception(login + " -> no group");
                }
                else
                {
                    do
                    {
                        try
                        {
                            string currentGroup = string.Empty;
                            // get group and check
                            do
                            {
                                currentGroup = OK.GetGroup(project, groupsList, login);
                                groupStatus = OK.CheckGroup(instance, project, "https://ok.ru/group/" + currentGroup, currentGroup, login);
                                if (groupStatus) OK.WriteBadGroup(project, settingsStr, pathSettings, user, login, tematika, proxy, currentGroup, groupsList);
                            } while (groupStatus);
                            // posting
                            do
                            {
                                try
                                {
                                    CommonCode.GoUrl(instance, project, "https://ok.ru/group/" + currentGroup, commonDelay, login);
                                    string postLastTime = OK.LastPost(instance, project, currentGroup, login);
                                    bool goPost = OK.GoPost(instance, project, postLastTime, timeOffsetDirectPost, currentGroup, login);
                                    if (goPost)
                                    {
                                        // direct post
                                        CommonCode.GoUrl(instance, project, "https://ok.ru/group/" + currentGroup + "/post", commonDelay, login);
                                        OK.DeleteDraft(instance, project, currentGroup, login);
                                        OK.PastLink(instance, project, pathLink, enPastLink, currentGroup, login);
                                        OK.PastImage(instance, project, pathImage, currentGroup, login);
                                        OK.PastComment(instance, project, pathComment, enPastComment, currentGroup, login);
                                        OK.PublishPost(instance, project, currentGroup, login);
                                    }

                                    // differed post
                                    CommonCode.GoUrl(instance, project, "https://ok.ru/group/" + currentGroup + "/topics", commonDelay, login);
                                    kolDifferedPost = OK.AmountDifferadPost(instance, project, currentGroup, login);

                                    project.SendInfoToLog(login + " -> " + currentGroup + " -> " + kolDifferedPost + "/" + minDifferedPost + " post in differed");
                                    if (kolDifferedPost < minDifferedPost)
                                    {
                                        if (kolDifferedPost != 0)
                                        {
                                            CommonCode.GoUrl(instance, project, "https://ok.ru/group/" + currentGroup + "/delayed", commonDelay, login);
                                        }
                                        string timeDifferedPost = OK.TimeDifferedPost(instance, project, currentGroup, login);
                                        CommonCode.GoUrl(instance, project, "https://ok.ru/group/" + currentGroup + "/post", commonDelay, login);
                                        OK.DeleteDraft(instance, project, currentGroup, login);
                                        OK.PastLink(instance, project, pathLink, enPastLink, currentGroup, login);
                                        OK.PastImage(instance, project, pathImage, currentGroup, login);
                                        OK.PastComment(instance, project, pathComment, enPastComment, currentGroup, login);
                                        OK.SetDifferedTime(instance, project, timeDifferedPost, currentGroup, login);
                                        OK.PublishPost(instance, project, currentGroup, login);
                                    }
                                    kolDifferedPost = OK.AmountDifferadPost(instance, project, currentGroup, login);
                                }
                                catch (Exception e)
                                {
                                    project.SendErrorToLog(e.ToString(), true);
                                    continue;
                                }
                            } while (kolDifferedPost < minDifferedPost);
                            groupsList.Remove(currentGroup);
                        }
                        catch (Exception e)
                        {
                            project.SendErrorToLog(e.ToString(), true);
                            continue;
                        }
                    } while (groupsList.Count != 0);
                    Profile.SafeProfile(project, enProfile, pathProfile, login);
                    project.SendInfoToLog(login + " -> FINISH!!!");
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