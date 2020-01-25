using System.IO;
using Kfstorm.LrcParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lrc2Storyboard
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var file = LrcFile.FromText(File.ReadAllText(args[0]));

            var root = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("base.json"));
            var transitionLength = (float) root["transition_length"];
            root.Remove("transition_length");
            var offset = (float) root["offset"];
            root.Remove("offset");
            var texts = new JArray();
            foreach (var oneLineLyric in file.Lyrics)
            {
                var start = oneLineLyric.Timestamp.TotalSeconds - transitionLength + offset;
                var text = new JObject();
                text["template"] = "lyrics";
                text["text"] = oneLineLyric.Content;
                text["time"] = start;
                var states = new JArray();
                var inState = new JObject();
                inState["template"] = "in";
                var outState = new JObject();
                outState["template"] = "out";
                var preOutState = new JObject();
                var nextLyric = file.After(oneLineLyric.Timestamp);
                if (nextLyric != null)
                {
                    preOutState["add_time"] = nextLyric.Timestamp.TotalSeconds - oneLineLyric.Timestamp.TotalSeconds - transitionLength * 2;
                } else {
                    preOutState["add_time"] = 5;
                }
                states.Add(inState);
                states.Add(preOutState);
                states.Add(outState);
                text["states"] = states;
                texts.Add(text);
            }

            root["texts"] = texts;
            File.WriteAllText("storyboard.json", JsonConvert.SerializeObject(root));
        }
    }
}