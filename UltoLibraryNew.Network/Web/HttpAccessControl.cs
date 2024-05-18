using System.Net;
using System.Text.RegularExpressions;

namespace UltoLibraryNew.Network.Web; 

public class HttpAccessControl {
    public delegate (TriState process, HttpStatusCode responseCode) HttpAccessRule(HttpNetRequest request, HttpNetResponse response);
    
    internal readonly Dictionary<string, (bool process, HttpStatusCode responseCode)> Rules = new();
    internal readonly List<HttpAccessRule> RulesList = [ ];
    
    /// <summary>
    /// Example: ^/asdasd/scripts/
    /// </summary>
    public void AddRule(string regexLocalPath, bool process, HttpStatusCode responseCode) {
        Rules.Add(regexLocalPath, (process, responseCode));
    }
    
    public void AddRule(HttpAccessRule rule) {
        RulesList.Add(rule);
    }
    
    public (bool process, HttpStatusCode responseCode) Process(HttpNetRequest request, HttpNetResponse response) {
        foreach (var rule in Rules) {
            if (!Regex.IsMatch(request.LocalUrl, rule.Key)) continue;
            
            return (rule.Value.process, rule.Value.responseCode);
        }

        foreach (var rule in RulesList) {
            var result = rule(request, response);
            if (result.process == TriState.USE_DEFAULT) continue;
            
            return (result.process.ToBool(), result.responseCode);
        }
        
        return (true, HttpStatusCode.OK);
    }
}