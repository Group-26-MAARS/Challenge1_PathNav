using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// for filebrowser
using SFB;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;

public class AssemblyButton : MonoBehaviour
{
    /// <summary>
    /// An experience can be of type Assembly or of type Route
    /// A full walkthrough will be a list of Experiences (typically as list of JSON)
    /// </summary>
    /// 
    private List<Experience> experienceItems;



    public class Experience
    {
        public string name;
        public string thingworxServer; // Will be empty for this experience if type is "Route"
        public string url; // Will be empty for this experience if type is "Route"
        public string type;
        public ExperienceRoute route; // Will be null for this experience if type is "Assembly"

        override public string ToString()
        {
            return $"name: {this.name}\ntype:{this.type}\nurl:{this.url}";
        }
    }

    public class ExperienceAnchor
    {
        private string _anchorName;
        private string _anchorData;

        public string getAnchorName()
        {
            return _anchorName;
        }

        public void setAnchorName(string  anchorName)
        {
            _anchorName = anchorName;
        }

        public string getAnchorData()
        {
            return _anchorData;
        }

        public void setAnchorData(string anchorData)
        {
            _anchorData = anchorData;
        }
    }


    public class ExperienceRoute
    {
        private string _routeName;
        private List<ExperienceAnchor> _anchors;

        public List<ExperienceAnchor> getAnchors()
        {
            return _anchors;
        }

        public void setAnchors(List<ExperienceAnchor> anchors)
        {
            _anchors = anchors;
        }

        public string getRouteName()
        {
            return _routeName;
        }

        public void setRouteName(string routeName)
        {
            _routeName = routeName;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        experienceItems = new List<Experience>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    // @steeve: move this to happen on startup of Unity App
    // handle logistics of Vuforia studio
    /// <summary>
    /// Looks for Vuforia Studio Projects Directory, 
    /// takes every project in there, and makes an object of the projects name, thingworxServer, url, and type (of Assembly)
    /// Saves the list of json to a file called "animations" (looking @ you, Dylan)
    /// </summary>
    public async Task setupVuforiaStudioLogistics()
    {
        if (GetOS() == "WIN")
        {
            string username = System.Environment.GetEnvironmentVariable("UserName");
            string docDir = $"C:\\Users\\{username}\\Documents";
            string challenge1Dir = $"{docDir}\\MAARS-C1";
            string expDir = $"{challenge1Dir}\\Experiences";
            string vuforiaProjectsDir = $"{docDir}\\VuforiaStudio\\Projects";

            // for each dir in vuforiaProjectsDir (except node_modules)
            string[] subdirs = Directory.GetDirectories(vuforiaProjectsDir);
            subdirs = subdirs.Where(x => !x.Contains("node_modules")).ToArray();

            string assembliesJsonPath = $"{challenge1Dir}\\animations.json";

            List<Experience> assemblyList = new List<Experience>();

            foreach (string sub in subdirs)
            {
                string json = File.ReadAllText($"{sub}\\appConfig.json");
                Experience tempExp = JsonUtility.FromJson<Experience>(json);
                tempExp.url = WebUtility.UrlEncode($"https://view.vuforia.com/command/view-experience?url={tempExp.thingworxServer}/ExperienceService/content/projects/{tempExp.name}/index.html");
                tempExp.type = "Assembly";

                HttpClient client = new HttpClient();
                string body = $"{tempExp.name}:{JsonConvert.SerializeObject(tempExp)}";
                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("name", tempExp.name);
                queryString.Add("url", tempExp.url);

                string url = "https://sharingservice20200308094713.azurewebsites.net/api/animations";

                // POST to API
                var response = await client.PostAsync(url, new StringContent(queryString.ToString()));
                //Debug.Log(response.StatusCode);
                // end POST

                assemblyList.Add(tempExp);
            }

            File.WriteAllText(assembliesJsonPath, JsonConvert.SerializeObject(assemblyList));
        }
    }

    /// <summary>
    /// Opens up a file browser so user can select the Experience they want
    /// Also calls runExperience()
    /// </summary>
    public async void selectProcedure()
    {
        if (GetOS() == "WIN")
        {
            await setupVuforiaStudioLogistics();
            string username = System.Environment.GetEnvironmentVariable("UserName");

            string c1Dir = $"C:\\Users\\{username}\\Documents\\MAARS-C1\\";
            string expDir = $"C:\\Users\\{username}\\Documents\\MAARS-C1\\Experiences";

            // dirs created if not already existing
            System.IO.Directory.CreateDirectory(c1Dir);
            System.IO.Directory.CreateDirectory(expDir);

            // paths will be string[] of length 1 (at least).
            var paths = StandaloneFileBrowser.OpenFilePanel("Open File", expDir, "", false);
            //Debug.Log(String.Join("\n", paths));
            runExperience(paths[0]);
        }
        else if (Application.platform == RuntimePlatform.Android)
        {

        }

    }
    public void addRouteExperience(Experience currExperience, string currExpStr)
    {
        int anchorCount = 0;

        // Parse Route, create Experience Obj
        ExperienceRoute currRoute = new ExperienceRoute();
        currRoute.setRouteName(currExpStr.Replace(" ", "").Split('`')[0]);
        // Get Comma separated list of anchors
        anchorCount = currExpStr.Replace(" ", "").Split('`')[1].Replace(" ", "").Split(',').Length;
        List<ExperienceAnchor> tempAnchorList = new List<ExperienceAnchor>();

        for (int j = 0; j < anchorCount; j++)
        {
            string currAnchorStr = currExpStr.Split('`')[1].Replace(" ", "").Split(',')[j];
            ExperienceAnchor anchor = new ExperienceAnchor();
            anchor.setAnchorName(currAnchorStr.Split(':')[0]);
            anchor.setAnchorData(currAnchorStr.Split(':')[1]);
            tempAnchorList.Add(anchor);
        }
        currRoute.setAnchors(tempAnchorList);
        currExperience.type = "Route";
        currExperience.route = currRoute;
        experienceItems.Add(currExperience);
    }

    public async void runSelectedExperience()
    {
        string experienceName = "MyExperienceName"; // Will need to get this from combobox
        // Get Experience from API
        HttpClient client = new HttpClient();
        string  allExperienceText = await client.GetStringAsync("https://sharingservice20200308094713.azurewebsites.net" + "/api/experiences/allassociated/" + experienceName);
        Console.Write(allExperienceText);

        int nbrExperienceItems = allExperienceText.Split('&').Length;
        // For Each Experience Item
        for (int i = 0; i < nbrExperienceItems; i++)
        {
            Experience tempExperience = new Experience();
            // Check if Route or Animation
            string currExpStr = allExperienceText.Split('&')[i];
            // If Route
            if (currExpStr[0] == 'R')
                addRouteExperience(tempExperience, currExpStr);
            else if(currExpStr[0] == 'A')
            {

            }
            // Add Experience Object to list
        }

        // For Each Experience, Determine if Route or Animation
        /*
        R_ThisRoute ` test0:40kjl3kht: AtUCF: 12 - 20 - 20:this is the first anchor 
            in the newerer form,myNewAnchor4eight2020: b3c219d0 - de05 - 479d - 88a8 - 
            087d731c7afe::04 / 15 / 2020 22:03,dylan: 6cdcbc70 - 0286 - 4302 - 887ec3e30ab29d0e:::
        `A_someAnimationName=>someAnimationSerializedJSON ` R_FinalRoute=>ghg: 2918d 345 - 3ae3 - 
            407e-beb7 - fefecc354155:::,dylan1: 440b519c - 6aad - 41bc - 96c5 - 2546a7bb9a78::: 
        */
    }



    /// <summary>
    /// Supposed to be a while loop type procedure where step after step is completed till done
    /// </summary>
    /// <param name="expPath">The path to the experience json file</param>
    public void runExperience(string expPath)
    {
        string json = File.ReadAllText(expPath);
        List<Experience> steps = JsonConvert.DeserializeObject<List<Experience>>(json);

        foreach (Experience exp in steps)
        {
            switch (exp.type)
            {
                case "Assembly":
                    HandleAssembly(exp);
                    break;
                case "Route":
                    HandleRoute(exp);
                    break;

                default:
                    HandleRoute(exp);
                    break;
            }
        }
    }

    /// <summary>
    /// Essentially opens Vuforia View to the corresponding Assembly 
    /// </summary>
    /// <param name="exp">The Experience representing the Assembly </param>
    /// @steeve: Need to find some way of determing whether user is "done" w/ Assembly
    /// (possibly when the process ends?)
    public void HandleAssembly(Experience exp)
    {
        //Debug.Log(exp);
        Application.OpenURL(exp.url);
    }

    /// <summary>
    /// Ideally fetches the Route for the navigation experience and calls the function that
    /// leads the user through PathNav
    /// </summary>
    /// <param name="exp"></param>
    /// @steeve: Connect w/ Dylan to see where that functionality is and call into it.
    public void HandleRoute(Experience exp)
    {
        //    Newtonsoft.Json.Linq.JObject parsed = Newtonsoft.Json.Linq.JObject.Parse(json);

        //    foreach (var pair in parsed)
        //    {
        //        Debug.Log($"{pair.Key}: {pair.Value}");
        //    }
    }

    public static string GetOS()
    {
        string os = "Unknow";
#if UNITY_EDITOR_WIN
        os = "WIN";
#endif
#if UNITY_IOS
                    os = "IPHONE";
#endif
#if UNITY_ANDROID
        os = "ANDROID";
#endif
#if UNITY_WEBGL
                    os = "WEBGL";
#endif
        return os;
    }
}