using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrackChanges.CodePlayer
{
    public class Options
    {
        public int? pos { get; set; }
        //public int step { get; set; }
        //public string place { get; set; }
        public string text { get; set; }
        public int delay { get; set; }
        //public string location { get; set; }
        public string attachment { get; set; }
        string _locale = "en";
        public string locale { get => _locale; set => _locale = value; }
        //public string timeout { get; set; }
        //public string command { get; set; }
        //public string text2 { get; set; }
        //public bool? success { get; set; }
        //public bool? add { get; set; }
        //public string placement { get; set; }
        //public string hide { get; set; }
        //public bool? hideOthers { get; set; }
        public int beforeDelay { get; set; }
        //public string selector { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public Options options { get; set; }
    }

    public class Step
    {
        public string en { get; set; }
    }

    public class Session
    {
        private readonly string sourceFile;
        private int lastPosition = 0;

        public Session(string lang, string sourceFile)
        {
            this.lang = lang;
            this.sourceFile = sourceFile;
            this.code = File.ReadAllText(sourceFile);

            actions = new List<Action>();
            steps = new List<Step>();

            id = new Regex(@"cp-id:([\w|-]*)").Match(this.code).Groups[1].Value;
            var matches = new Regex(@"cp-step:(.*)").Matches(this.code);
            foreach (Match match in matches)
            {
                steps.Add(new Step { en = match.Groups[1].Value });
            }
        }

        public string id { get; set; }
        public string lang { get; set; }
        public string code { get; set; }
        public List<Action> actions { get; set; }
        public List<Step> steps { get; set; }

        internal void AddActionTyping(int moveTo, string text)
        {
            //   "type":"popover",
            //"options":{
            //       "text":"Let's take a look at <i>Extract Method<\/i> using this function as an example.",
            //   "attachment":"selection",
            //   "locale":"en"
            //}
            AddActionPopover("** EXPLIQUE POR QUE VOCÊ ESTÁ INSERINDO ESTAS LINHAS...");

            this.actions.Add(new CodePlayer.Action
            {
                type = "type",
                options = new Options
                {
                    pos = moveTo,
                    text = text,
                    delay = 20
                }
            });
        }

        private void AddActionPopover(string popover)
        {
            this.actions.Add(new CodePlayer.Action
            {
                type = "popover",
                options = new Options
                {
                    text = popover,
                    locale = "en",
                    pos = lastPosition,
                    attachment = "selection"
                }
            });
        }

        internal void MoveTo(int charCount)
        {
            this.actions.Add(new CodePlayer.Action
            {
                type = "moveTo",
                options = new Options
                {
                    pos = charCount,
                    delay = 20
                }
            });
            lastPosition = charCount;
        }

        internal void AddActionDelete(int charCount, string text)
        {
            AddActionSelect(charCount, text);
            AddActionPopover("** EXPLIQUE POR QUE VOCÊ ESTÁ DELETANDO ESTAS LINHAS...");
            this.actions.Add(new CodePlayer.Action
            {
                type = "type",
                options = new Options
                {
                    //pos = charCount,
                    text = "←",
                    beforeDelay = 500
                }
            });
            lastPosition = charCount;
        }

        private void AddActionSelect(int charCount, string text)
        {
            AddActionPopover("** EXPLIQUE POR QUE VOCÊ ESTÁ SELECIONANDO ESTAS LINHAS...");
            this.actions.Add(new CodePlayer.Action
            {
                type = "select",
                options = new Options
                {
                    pos = charCount,
                    text = text,
                    beforeDelay = 500
                }
            });
            lastPosition = charCount;
        }
    }
}
