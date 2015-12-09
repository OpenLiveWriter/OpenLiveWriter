// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using mshtml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Localization;
using OpenLiveWriter.PostEditor.PostHtmlEditing;
using System.Collections;

namespace OpenLiveWriter.PostEditor.Emoticons
{
    public class EmoticonsManager
    {
        private const int MAX_RECENT_EMOTICONS = 10;
        private static Bitmap bitmapStrip = Images.Emoticon_All_Strip;
        private static int nextIndex = 0;

        private static readonly List<Emoticon> _emoticons = new List<Emoticon> {
            // !!IMPORTANT!! - If any emoticons below are added/removed, update the %inetroot%\client\writer\src\managed\OpenLiveWriter.localization\emoticons\emoticonList.txt maintaining the same order as here
            // Also run packemoticons.cmd in %inetroot%\client\writer\src\managed\OpenLiveWriter.localization to repack Emoticon_All_Strip.png and then re-add it into OpenLiveWriter.localization\Images.resx

            // If any of the autoreplace strings change, make sure to update the IsAutoReplaceEndingCharacter function.
            //                             AutoReplaceText                              AltText                                                 Bitmap strip, index       Id                                                  FileExtension
            new Emoticon(new List<string> { ":-)", ":)" },                              Res.Get(StringId.EmoticonSmile),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-smile",                     ".png"),
            new Emoticon(new List<string> { ":-D", ":-d", ":D", ":d", ":>", ":->" },    Res.Get(StringId.EmoticonOpenMouthedSmile),             bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-openmouthedsmile",          ".png"),
            new Emoticon(new List<string> { ";-)", ";)" },                              Res.Get(StringId.EmoticonWinkingSmile),                 bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-winkingsmile",              ".png"),
            new Emoticon(new List<string> { ":-O", ":-o", ":O", ":o" },                 Res.Get(StringId.EmoticonSurprisedSmile),               bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-surprisedsmile",            ".png"),
            new Emoticon(new List<string> { ":-P", ":-p", ":P", ":p" },                 Res.Get(StringId.EmoticonSmileWithTongueOut),           bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-smilewithtongueout",        ".png"),
            new Emoticon(new List<string> { "(H)", "(h)" },                             Res.Get(StringId.EmoticonHotSmile),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-hotsmile",                  ".png"),
            new Emoticon(new List<string> { ":-@", ":@" },                              Res.Get(StringId.EmoticonAngrySmile),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-angrysmile",                ".png"),
            new Emoticon(new List<string> { ":-$", ":$" },                              Res.Get(StringId.EmoticonEmbarrassedSmile),             bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-embarrassedsmile",          ".png"),
            new Emoticon(new List<string> { ":-S", ":-s", ":S", ":s" },                 Res.Get(StringId.EmoticonConfusedSmile),                bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-confusedsmile",             ".png"),
            new Emoticon(new List<string> { ":-(", ":(", ":<", ":-<" },                 Res.Get(StringId.EmoticonSadSmile),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sadsmile",                  ".png"),
            new Emoticon(new List<string> { ":'(", ":’(" },                             Res.Get(StringId.EmoticonCryingFace),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-cryingface",                ".png"),
            new Emoticon(new List<string> { ":-|", ":|" },                              Res.Get(StringId.EmoticonDisappointedSmile),            bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-disappointedsmile",         ".png"),
            new Emoticon(new List<string> { "(6)" },                                    Res.Get(StringId.EmoticonDevil),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-devil",                     ".png"),
            new Emoticon(new List<string> { "(A)", "(a)" },                             Res.Get(StringId.EmoticonAngel),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-angel",                     ".png"),
            new Emoticon(new List<string> { ":-[", ":[" },                              Res.Get(StringId.EmoticonVampireBat),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-vampirebat",                ".png"),
            new Emoticon(new List<string> { ":-#" },                                    Res.Get(StringId.EmoticonDontTellAnyoneSmile),          bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-donttellanyonesmile",       ".png"),
            new Emoticon(new List<string> { "8o|" },                                    Res.Get(StringId.EmoticonBaringTeethSmile),             bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-baringteethsmile",          ".png"),
            new Emoticon(new List<string> { "8-|" },                                    Res.Get(StringId.EmoticonNerdSmile),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-nerdsmile",                 ".png"),
            new Emoticon(new List<string> { "^o)" },                                    Res.Get(StringId.EmoticonSarcasticSmile),               bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sarcasticsmile",            ".png"),
            new Emoticon(new List<string> { ":-*" },                                    Res.Get(StringId.EmoticonSecretTellingSmile),           bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-secrettellingsmile",        ".png"),
            new Emoticon(new List<string> { "+o(" },                                    Res.Get(StringId.EmoticonSickSmile),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sicksmile",                 ".png"),
            new Emoticon(new List<string> { "(brb)" },                                  Res.Get(StringId.EmoticonBeRightBack),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-berightback",               ".png"),
            new Emoticon(new List<string> { ":^)" },                                    Res.Get(StringId.EmoticonIDontKnowSmile),               bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-idontknowsmile",            ".png"),
            new Emoticon(new List<string> { "*-)" },                                    Res.Get(StringId.EmoticonThinkingSmile),                bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-thinkingsmile",             ".png"),
            new Emoticon(new List<string> { "<O)", "<o)" },                             Res.Get(StringId.EmoticonPartySmile),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-partysmile",                ".png"),
            new Emoticon(new List<string> { "8-)" },                                    Res.Get(StringId.EmoticonEyeRollingSmile),              bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-eyerollingsmile",           ".png"),
            new Emoticon(new List<string> { "|-)" },                                    Res.Get(StringId.EmoticonSleepySmile),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sleepysmile",               ".png"),
            new Emoticon(new List<string> { @";-\", @";\" },                            Res.Get(StringId.EmoticonShifty),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-shifty",                    ".png"),
            new Emoticon(new List<string> { @":-\" },                                   Res.Get(StringId.EmoticonAnnoyed),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-annoyed",                   ".png"),
            new Emoticon(new List<string> { "(jk)" },                                   Res.Get(StringId.EmoticonJustKidding),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-justkidding",               ".png"),
            new Emoticon(new List<string> { "(J)", "(j)" },                             Res.Get(StringId.EmoticonNinja),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-ninja",                     ".png"),
            new Emoticon(new List<string> { "(V)", "(v)" },                             Res.Get(StringId.EmoticonGreenWithEnvy),                bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-greenwithenvy",             ".png"),
            new Emoticon(new List<string> { "(lol)" },                                  Res.Get(StringId.EmoticonLaughingOutLoud),              bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-laughingoutloud",           ".png"),
            new Emoticon(new List<string> { "(rotfl)", "(rofl)" },                      Res.Get(StringId.EmoticonRollingOnTheFloorLaughing),    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-rollingonthefloorlaughing", ".png"),
            new Emoticon(new List<string> { ":-B", ":-b", ":B", ":b" },                 Res.Get(StringId.EmoticonNyahNyah),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-nyahnyah",                  ".png"),
            new Emoticon(new List<string> { ":8)" },                                    Res.Get(StringId.EmoticonAlien),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-alien",                     ".png"),
            new Emoticon(new List<string> { "(ff)" },                                   Res.Get(StringId.EmoticonFlirtFemale),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-flirtfemale",               ".png"),
            new Emoticon(new List<string> { "(fm)" },                                   Res.Get(StringId.EmoticonFlirtMale),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-flirtmale",                 ".png"),
            new Emoticon(new List<string> { ":'-|", ":’-|", ":'|", ":’|" },             Res.Get(StringId.EmoticonFreezing),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-freezing",                  ".png"),
            new Emoticon(new List<string> { ":-]", ":]" },                              Res.Get(StringId.EmoticonInLove),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-Inlove",                    ".png"),
            new Emoticon(new List<string> { ":-}", ":}" },                              Res.Get(StringId.EmoticonPrincess),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-princess",                  ".png"),
            new Emoticon(new List<string> { "(BOO)" },                                  Res.Get(StringId.EmoticonGhost),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-ghost",                     ".png"),
            new Emoticon(new List<string> { "*-|", "*|" },                              Res.Get(StringId.EmoticonPunch),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-punch",                     ".png"),
            new Emoticon(new List<string> { @"*\", @"*-\" },                            Res.Get(StringId.EmoticonPunk),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-punk",                      ".png"),
            new Emoticon(new List<string> { ";-@", ";@" },                              Res.Get(StringId.EmoticonSteamingMad),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-steamingmad",               ".png"),
            new Emoticon(new List<string> { "(wm)" },                                   Res.Get(StringId.EmoticonWhoMe),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-whome",                     ".png"),
            new Emoticon(new List<string> { "(xo)" },                                   Res.Get(StringId.EmoticonSendAKiss),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sendakiss",                 ".png"),
            new Emoticon(new List<string> { "(L)", "(l)" },                             Res.Get(StringId.EmoticonRedHeart),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-redheart",                  ".png"),
            new Emoticon(new List<string> { "(U)", "(u)" },                             Res.Get(StringId.EmoticonBrokenHeart),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-brokenheart",               ".png"),
            new Emoticon(new List<string> { "(@)" },                                    Res.Get(StringId.EmoticonCatFace),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-catface",                   ".png"),
            new Emoticon(new List<string> { "(&)" },                                    Res.Get(StringId.EmoticonDogFace),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-dogface",                   ".png"),
            new Emoticon(new List<string> { "(S)", "(s)" },                             Res.Get(StringId.EmoticonSleepingHalfMoon),             bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sleepinghalfmoon",          ".png"),
            new Emoticon(new List<string> { "(*)" },                                    Res.Get(StringId.EmoticonStar),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-star",                      ".png"),
            new Emoticon(new List<string> { "(~)" },                                    Res.Get(StringId.EmoticonFilmstrip),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-filmstrip",                 ".png"),
            new Emoticon(new List<string> { "(8)" },                                    Res.Get(StringId.EmoticonNote),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-note",                      ".png"),
            new Emoticon(new List<string> { "(ee)" },                                   Res.Get(StringId.EmoticonEmail),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-email",                     ".png"),
            new Emoticon(new List<string> { "(F)", "(f)" },                             Res.Get(StringId.EmoticonRedRose),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-redrose",                   ".png"),
            new Emoticon(new List<string> { "(W)", "(w)" },                             Res.Get(StringId.EmoticonWiltedRose),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-wiltedrose",                ".png"),
            new Emoticon(new List<string> { "(O)", "(o)", "(0)" },                      Res.Get(StringId.EmoticonClock),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-clock",                     ".png"),
            new Emoticon(new List<string> { "(K)", "(k)" },                             Res.Get(StringId.EmoticonRedLips),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-redlips",                   ".png"),
            new Emoticon(new List<string> { "(G)", "(g)" },                             Res.Get(StringId.EmoticonGiftWithABow),                 bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-giftwithabow",              ".png"),
            new Emoticon(new List<string> { "(^)" },                                    Res.Get(StringId.EmoticonBirthdayCake),                 bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-birthdaycake",              ".png"),
            new Emoticon(new List<string> { "(P)", "(p)" },                             Res.Get(StringId.EmoticonCamera),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-camera",                    ".png"),
            new Emoticon(new List<string> { "(I)", "(i)" },                             Res.Get(StringId.EmoticonLightBulb),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-lightbulb",                 ".png"),
            new Emoticon(new List<string> { "(cc)" },                                   Res.Get(StringId.EmoticonCoffeeCup),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-coffeecup",                 ".png"),
            new Emoticon(new List<string> { "(T)", "(t)" },                             Res.Get(StringId.EmoticonCallMe),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-callme",                    ".png"),
            new Emoticon(new List<string> { "({)" },                                    Res.Get(StringId.EmoticonLeftHug),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-lefthug",                   ".png"),
            new Emoticon(new List<string> { "(})" },                                    Res.Get(StringId.EmoticonRightHug),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-righthug",                  ".png"),
            new Emoticon(new List<string> { "(B)", "(b)" },                             Res.Get(StringId.EmoticonMug),                          bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-mug",                       ".png"),
            new Emoticon(new List<string> { "(D)", "(d)" },                             Res.Get(StringId.EmoticonMartiniGlass),                 bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-martiniglass",              ".png"),
            new Emoticon(new List<string> { "(Z)", "(z)" },                             Res.Get(StringId.EmoticonBoy),                          bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-boy",                       ".png"),
            new Emoticon(new List<string> { "(X)", "(x)" },                             Res.Get(StringId.EmoticonGirl),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-girl",                      ".png"),
            new Emoticon(new List<string> { "(Y)", "(y)" },                             Res.Get(StringId.EmoticonThumbsUp),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-thumbsup",                  ".png"),
            new Emoticon(new List<string> { "(N)", "(n)" },                             Res.Get(StringId.EmoticonThumbsDown),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-thumbsdown",                ".png"),
            new Emoticon(new List<string> { "(nnh)", "(nah)" },                         Res.Get(StringId.EmoticonGoat),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-goat",                      ".png"),
            new Emoticon(new List<string> { "(#)" },                                    Res.Get(StringId.EmoticonSun),                          bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-sun",                       ".png"),
            new Emoticon(new List<string> { "(rr)" },                                   Res.Get(StringId.EmoticonRainbow),                      bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-rainbow",                   ".png"),
            new Emoticon(new List<string> { "(sn)" },                                   Res.Get(StringId.EmoticonSnail),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-snail",                     ".png"),
            new Emoticon(new List<string> { "(tu)" },                                   Res.Get(StringId.EmoticonTurtle),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-turtle",                    ".png"),
            new Emoticon(new List<string> { "(pl)" },                                   Res.Get(StringId.EmoticonPlate),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-plate",                     ".png"),
            new Emoticon(new List<string> { "(||)" },                                   Res.Get(StringId.EmoticonBowl),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-bowl",                      ".png"),
            new Emoticon(new List<string> { "(pi)" },                                   Res.Get(StringId.EmoticonPizza),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-pizza",                     ".png"),
            new Emoticon(new List<string> { "(so)" },                                   Res.Get(StringId.EmoticonSoccerBall),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-soccerball",                ".png"),
            new Emoticon(new List<string> { "(au)" },                                   Res.Get(StringId.EmoticonAuto),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-auto",                      ".png"),
            new Emoticon(new List<string> { "(ap)" },                                   Res.Get(StringId.EmoticonAirplane),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-airplane",                  ".png"),
            new Emoticon(new List<string> { "(um)" },                                   Res.Get(StringId.EmoticonUmbrella),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-umbrella",                  ".png"),
            new Emoticon(new List<string> { "(ip)" },                                   Res.Get(StringId.EmoticonIslandWithAPalmTree),          bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-islandwithapalmtree",       ".png"),
            new Emoticon(new List<string> { "(co)" },                                   Res.Get(StringId.EmoticonComputer),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-computer",                  ".png"),
            new Emoticon(new List<string> { "(mp)" },                                   Res.Get(StringId.EmoticonMobilePhone),                  bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-mobilephone",               ".png"),
            new Emoticon(new List<string> { "(st)" },                                   Res.Get(StringId.EmoticonStormCloud),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-stormcloud",                ".png"),
            new Emoticon(new List<string> { "(pu)" },                                   Res.Get(StringId.EmoticonPointingUp),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-pointingup",                ".png"),
            new Emoticon(new List<string> { "(h5)" },                                   Res.Get(StringId.EmoticonHighFive),                     bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-highfive",                  ".png"),
            new Emoticon(new List<string> { "(yn)" },                                   Res.Get(StringId.EmoticonFingersCrossed),               bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-fingerscrossed",            ".png"),
            new Emoticon(new List<string> { "(mo)" },                                   Res.Get(StringId.EmoticonMoney),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-money",                     ".png"),
            new Emoticon(new List<string> { "(bah)" },                                  Res.Get(StringId.EmoticonBlackSheep),                   bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-blacksheep",                ".png"),
            new Emoticon(new List<string> { "(li)" },                                   Res.Get(StringId.EmoticonLightning),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-lightning",                 ".png"),
            new Emoticon(new List<string> { "(wo)" },                                   Res.Get(StringId.EmoticonWork),                         bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-work",                      ".png"),
            new Emoticon(new List<string> { "('.')", "(’.’)" },                         Res.Get(StringId.EmoticonBunny),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-bunny",                     ".png"),
            new Emoticon(new List<string> { "(bus)" },                                  Res.Get(StringId.EmoticonSchoolBus),                    bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-schoolbus",                 ".png"),
            new Emoticon(new List<string> { "*p*" },                                    Res.Get(StringId.EmoticonPeace),                        bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-peace",                     ".png"),
            new Emoticon(new List<string> { "*s*" },                                    Res.Get(StringId.EmoticonSchool),                       bitmapStrip, nextIndex++, Emoticon.CLASS_NAME + "-school",                    ".png")
        };

        private List<Emoticon> _recentEmoticons;
        private Dictionary<Emoticon, Uri> _inlineImageUriTable;
        private IBlogPostImageEditingContext _imageEditingContext;
        private IEditingMode _editingModeContext;

        public EmoticonsManager(IBlogPostImageEditingContext imageEditingContext, IEditingMode editingModeContext)
        {
            _imageEditingContext = imageEditingContext;
            _editingModeContext = editingModeContext;
            _recentEmoticons = RetrieveRecentEmoticons();
            _inlineImageUriTable = new Dictionary<Emoticon, Uri>();

            // This is a DEBUG only call.
            AssertEmoticonsManagerSetupCorrectly();
        }

        public List<Emoticon> RecentEmoticons
        {
            get { return _recentEmoticons; }
        }

        public List<Emoticon> PopularEmoticons
        {
            get { return _emoticons; }
        }

        public static bool IsAutoReplaceEndingCharacter(char c)
        {
            char cLower = char.ToLowerInvariant(c);
            return (cLower == 's' ||
                cLower == 'o' ||
                cLower == 'd' ||
                cLower == 'p' ||
                cLower == 'l' ||
                cLower == 'b' ||
                c == ')' ||
                c == '(' ||
                c == '|' ||
                c == '*' ||
                c == '[' ||
                c == '$' ||
                c == '#' ||
                c == '@' ||
                c == '\\' ||
                c == '>' ||
                c == '<' ||
                c == '}' ||
                c == ']');
        }

        public bool CanInsertEmoticonImage
        {
            get { return _editingModeContext.CurrentEditingMode != EditingMode.PlainText; }
        }

        /// <summary>
        /// Adds the given emoticon as the most recently used.
        /// </summary>
        public void AddToRecent(Emoticon emoticon)
        {
            // We don't want duplicates, so always attempt to remove the emoticon first.
            _recentEmoticons.Remove(emoticon);
            _recentEmoticons.Insert(0, emoticon);

            if (_recentEmoticons.Count > MAX_RECENT_EMOTICONS)
            {
                int numOver = _recentEmoticons.Count - MAX_RECENT_EMOTICONS;
                _recentEmoticons.RemoveRange(_recentEmoticons.Count - numOver, numOver);
            }

            // Save to registry.
            SaveRecentEmoticons(_recentEmoticons);
        }

        /// <summary>
        /// Attempts to find the emoticon specified by the given HTML element.
        /// </summary>
        public static Emoticon GetEmoticon(IHTMLElement element)
        {
            if (element == null || (element as IHTMLImgElement) == null || String.IsNullOrEmpty(element.className))
                return null;

            List<string> classNames = new List<string>(element.className.Split(new[] { ' ' }));
            if (classNames.Contains(Emoticon.CLASS_NAME))
            {
                // An emoticon should have two classes, one identifying it as an emoticon and the other identifying which emoticon.
                foreach (string className in classNames)
                    foreach (Emoticon emoticon in _emoticons)
                        if (className == emoticon.Id)
                            return emoticon;
            }

            return null;
        }

        /// <summary>
        /// Returns HTML that can be inserted into the canvas to display an emoticon.
        /// </summary>
        public string GetHtml(Emoticon emoticon)
        {
            if (!CanInsertEmoticonImage)
            {
                // If we can't insert images, just return a plain-text emoticon.
                Debug.Assert(emoticon.AutoReplaceText.Count > 0, "Emoticon is missing autoreplace text.");
                return emoticon.AutoReplaceText[0];
            }

            Uri inlineImageUri;
            if (!_inlineImageUriTable.TryGetValue(emoticon, out inlineImageUri))
            {
                inlineImageUri = CreateInlineImage(emoticon);
            }

            return String.Format(CultureInfo.InvariantCulture, "<img src=\"{0}\" style=\"border-style: none;\" alt=\"{1}\" class=\"{2} {3}\" />",
                UrlHelper.SafeToAbsoluteUri(inlineImageUri), HtmlServices.HtmlEncode(emoticon.AltText), Emoticon.CLASS_NAME, emoticon.Id);
        }

        /// <summary>
        /// Saves the emoticon image to disk and returns the path. Should only be called if the emoticon image has not been saved previously for this blog post.
        /// </summary>
        private Uri CreateInlineImage(Emoticon emoticon)
        {
            Debug.Assert(_imageEditingContext.ImageList != null && _imageEditingContext.SupportingFileService != null, "ImageEditingContext not initalized yet.");

            Stream emoticonStream = StreamHelper.AsStream(ImageHelper.GetBitmapBytes(emoticon.Bitmap, ImageFormat.Png));
            ISupportingFile sourceFile = _imageEditingContext.SupportingFileService.CreateSupportingFile(emoticon.Id + emoticon.FileExtension, emoticonStream);
            ImageFileData sourceFileData = new ImageFileData(sourceFile, emoticon.Bitmap.Width, emoticon.Bitmap.Height, ImageFileRelationship.Source);

            BlogPostImageData imageData = new BlogPostImageData(sourceFileData);

            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.ShadowImageForDrafts))
                imageData.InitShadowFile(_imageEditingContext.SupportingFileService);

            emoticonStream.Seek(0, SeekOrigin.Begin);
            ISupportingFile inlineFile = _imageEditingContext.SupportingFileService.CreateSupportingFile(emoticon.Id + emoticon.FileExtension, emoticonStream);
            imageData.InlineImageFile = new ImageFileData(inlineFile, emoticon.Bitmap.Width, emoticon.Bitmap.Height, ImageFileRelationship.Inline);

            _imageEditingContext.ImageList.AddImage(imageData);

            SetInlineImageUri(emoticon, imageData.InlineImageFile.Uri);

            return imageData.InlineImageFile.Uri;
        }

        /// <summary>
        /// Gets where the emoticon image is saved so that duplicate emoticons can point to the same image.
        /// </summary>
        public Uri GetInlineImageUri(Emoticon emoticon)
        {
            Uri inlineImageUri;
            if (!_inlineImageUriTable.TryGetValue(emoticon, out inlineImageUri))
            {
                inlineImageUri = CreateInlineImage(emoticon);
            }

            return inlineImageUri;
        }

        /// <summary>
        /// Sets where the emoticon image is saved so that duplicate emoticons can point to the same image.
        /// </summary>
        public void SetInlineImageUri(Emoticon emoticon, Uri inlineUri)
        {
            _inlineImageUriTable.Remove(emoticon);
            _inlineImageUriTable.Add(emoticon, inlineUri);
        }

        /// <summary>
        /// Grabs the list of recently used emoticons from the registry.
        /// </summary>
        private List<Emoticon> RetrieveRecentEmoticons()
        {
            List<Emoticon> recentEmoticons = new List<Emoticon>();
            foreach (string name in PostEditorSettings.RecentEmoticonsKey.GetNames())
            {
                string recentEmoticonId = PostEditorSettings.RecentEmoticonsKey.GetString(name, null);
                if (recentEmoticonId != null)
                {
                    try
                    {
                        Emoticon recentEmoticon = PopularEmoticons.Find(emoticon => emoticon.Id == recentEmoticonId);
                        if (recentEmoticon != null)
                            recentEmoticons.Add(recentEmoticon);
                        else
                            PostEditorSettings.RecentEmoticonsKey.SetString(name, null);
                    }
                    catch (Exception ex)
                    {
                        Trace.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected exception getting Recent emoticon info [{0}]:{1}", recentEmoticonId, ex.ToString()));
                    }
                }
            }
            return recentEmoticons;
        }

        /// <summary>
        /// Saves the list of recently used emoticons to the registry.
        /// </summary>
        private void SaveRecentEmoticons(List<Emoticon> recentEmoticons)
        {
            // Wipe out existing recent emoticons.
            foreach (string name in PostEditorSettings.RecentEmoticonsKey.GetNames())
                PostEditorSettings.RecentEmoticonsKey.Unset(name);

            // Save recent emoticons.
            for (int i = 0; i < recentEmoticons.Count; i++)
                PostEditorSettings.RecentEmoticonsKey.SetString(String.Format(CultureInfo.InvariantCulture, "Entry{0:00}", i), recentEmoticons[i].Id);
        }

        /// <summary>
        /// This debug-only function verifies that the emoticons are setup correctly.
        /// </summary>
        [Conditional("DEBUG")]
        private void AssertEmoticonsManagerSetupCorrectly()
        {
            Hashtable autoReplaceHashtable = new Hashtable();
            Hashtable idHashtable = new Hashtable();

            foreach (Emoticon emoticon in _emoticons)
            {
                foreach (string autoReplaceString in emoticon.AutoReplaceText)
                {
                    // Assert all the AutoReplace strings are unique.
                    Debug.Assert(!autoReplaceHashtable.ContainsKey(autoReplaceString), emoticon.AltText + " emoticon auto-replace string is non-unique!");
                    autoReplaceHashtable.Add(autoReplaceString, String.Empty);

                    // Assert IsAutoReplaceEndingCharacter is correct.
                    Debug.Assert(IsAutoReplaceEndingCharacter(autoReplaceString[autoReplaceString.Length - 1]), "EmoticonsManager.IsAutoReplaceEndingCharacter needs to be updated for " + emoticon.AltText + " emoticon!");
                }

                // Assert all the Ids are unique.
                Debug.Assert(!idHashtable.ContainsKey(emoticon.Id), emoticon.AltText + " emoticon id is non-unique!");
                autoReplaceHashtable.Add(emoticon.Id, String.Empty);
            }
        }
    }
}
