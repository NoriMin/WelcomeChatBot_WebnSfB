using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WelcomeChatBot_WebnSfB.Models
{
    public class ZenkakuConvert
    {
        public static Dictionary<char, char> dictionary =
            new Dictionary<char, char>()
            {
                {'０','0'},{'１','1'},{'２','2'},{'３','3'},
                {'４','4'},{'５','5'},{'６','6'},{'７','7'},
                {'８','8'},{'９','9'},
            };

        public static async Task<string> Convert(string source)
        {
            Regex regex = new Regex("[０-９]+");
            return regex.Replace(source, Replacer);
        }

        public static string Replacer(Match m)
        {
            return new string(m.Value.Select(n => dictionary[n]).ToArray());
        }
    }
}