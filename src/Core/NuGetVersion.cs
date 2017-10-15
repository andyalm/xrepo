using System.Linq;

namespace XRepo.Core
{
    public static class NuGetVersion
    {
        public static string Normalize(string input)
        {
            var prereleaseIndex = input.IndexOf("-");
            var buildIndex = input.IndexOf("+");
            var mainVersion = input;
            var suffix = "";
            if (prereleaseIndex > 0)
            {
                mainVersion = input.Remove(prereleaseIndex);
                suffix = input.Substring(prereleaseIndex);
            }
            else if(buildIndex > 0)
            {
                mainVersion = input.Remove(buildIndex);
                suffix = input.Substring(buildIndex);
            }
            
            var mainVersionParts = mainVersion.Split('.').ToList();
            if (mainVersionParts.Count == 1)
            {
                mainVersionParts.Add("0");
            }
            if (mainVersionParts.Count == 2)
            {
                mainVersionParts.Add("0");
            }

            var normalizedMainVersion = string.Join(".", mainVersionParts);

            return $"{normalizedMainVersion}{suffix}";
        }
    }
}