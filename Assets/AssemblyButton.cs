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
using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using UnityEngine.SceneManagement;


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
        public string _type;
        public ExperienceRoute route; // Will be null for this experience if type is "Assembly"
        public ExperienceAnimation animation; // Will be null for this experience if type is "Route"
        public string getType()
        {
            return _type;
        }

        public void setType(string type)
        {
            _type = type;
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

    public class ExperienceAnimation
    {
        public string name;
        public string _thingworxServer; // Will be empty for this experience if type is "Route"
        public string url; // Will be empty for this experience if type is "Route"

        public string getName()
        {
            return name;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public string getThingWorxServer()
        {
            return _thingworxServer;
        }

        public void setThingWorxServer(string thingworxServer)
        {
            _thingworxServer = thingworxServer;
        }
        public string getURL()
        {
            return url;
        }

        public void setURL(string url)
        {
            this.url = url;
        }
        override public string ToString()
        {
            return $"name: {this.name}\ntype:{"Animation"}\nurl:{this.url}";
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
        this.experienceItems = new List<Experience>();
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
                Experience tmpExperience = new Experience();
                ExperienceAnimation animationExperience = JsonUtility.FromJson<ExperienceAnimation>(json);
                animationExperience.
                    setURL(WebUtility.UrlEncode($"https://view.vuforia.com/command/view-experience?url={animationExperience.getThingWorxServer()}/ExperienceService/content/projects/{animationExperience.getName()}/index.html"));
                tmpExperience.setType("Assembly");

                HttpClient client = new HttpClient();
                string body = $"{animationExperience.getName()}:{JsonConvert.SerializeObject(animationExperience)}";
                NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString.Add("name", animationExperience.getName());
                queryString.Add("url", animationExperience.getURL());

                string url = "https://sharingservice20200308094713.azurewebsites.net/api/animations";

                // POST to API
                var response = await client.PostAsync(url, new StringContent(queryString.ToString()));
                //Debug.Log(response.StatusCode);
                // end POST
                tmpExperience.animation = animationExperience;
                assemblyList.Add(tmpExperience);
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
    /// <summary>
    /// Creates object of class ExperienceRoute, populates with data from API and adds it to experienceItems queue
    /// </summary>
    /// <param name="currExperience">A pointer the experience item that is added to the list of experiences</param>
    /// <param name="currRouteStr">The current route string from the API for this route</param>
    public void addRouteExperience(Experience currExperience, string currRouteStr)
    {
        int anchorCount = 0;

        // Parse Route, create Experience Obj
        ExperienceRoute currRoute = new ExperienceRoute();
        currRoute.setRouteName(currRouteStr.Replace(" ", "").Split('`')[0]);
        // Get Comma separated list of anchors
        anchorCount = currRouteStr.Replace(" ", "").Split('`')[1].Replace(" ", "").Split(',').Length;
        List<ExperienceAnchor> tempAnchorList = new List<ExperienceAnchor>();

        for (int j = 0; j < anchorCount; j++)
        {
            string currAnchorStr = currRouteStr.Split('`')[1].Replace(" ", "").Split(',')[j];
            ExperienceAnchor anchor = new ExperienceAnchor();
            anchor.setAnchorName(currAnchorStr.Split(':')[0]);
            anchor.setAnchorData(currAnchorStr.Split(':')[1]);
            tempAnchorList.Add(anchor);
        }
        currRoute.setAnchors(tempAnchorList);
        currExperience.setType("Route");
        currExperience.route = currRoute;
        this.experienceItems.Add(currExperience);
    }

    /// <summary>
    /// Creates object of class ExperienceAnimation, populates with data from API and adds it to experienceItems queue
    /// </summary>
    /// <param name="currExperienceItem">A pointer the experience item that is added to the list of experiences</param>
    /// <param name="currAnimationStr">The current animation string from the API for this animation</param>
    public void addAnimationExperience(Experience currExperienceItem, string currAnimationStr)
    {
        ExperienceAnimation animationExperience = new ExperienceAnimation();
        animationExperience = JsonConvert.DeserializeObject<ExperienceAnimation>(currAnimationStr.Split(new char[] {'~'})[1]);
        currExperienceItem.animation = animationExperience;
        currExperienceItem.setType("Assembly");
        this.experienceItems.Add(currExperienceItem);
    }

    /// <summary>
    /// Initializes list of experience items. Each item is an object of class ExperienceAnimation or
    /// ExperienceRoute.
    /// </summary>
    /// <param name="experienceName">The selected experience from the combobox</param>

    public async Task initializeExperienceItems(string experienceName)
    {
        // Get Experience from API
        HttpClient client = new HttpClient();
        string allExperienceText = await client.GetStringAsync("https://sharingservice20200308094713.azurewebsites.net" + 
            "/api/experiences/allassociated/" + experienceName);
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
            else if (currExpStr[0] == 'A')
            {
                addAnimationExperience(tempExperience, currExpStr);
            }
            // Add Experience Object to list
        }
    }

    /// <summary>
    /// Pulls and runs next experience item from experienceItems
    /// </summary>
    public void pullAndRunNextExpItem()
    {
        Experience currExp = new Experience();
        currExp = experienceItems[0];
        if (currExp != null)
        {
            experienceItems.RemoveAt(0);
            if (currExp.getType() == "Route")
            {
                HandleRoute(currExp.route);
            }
            else if (currExp.getType() == "Assembly")
            {
                HandleAssembly(currExp.animation);
            }
        }

    }

    /// <summary>
    /// Initializes Experience list items and creates a queue to pull from then pulls first item
    /// </summary>
    /// @dylan: Should show some window indicating to the user that the experience is complete
    public async void runSelectedExperience()
    {
        string experienceName = "MyExperienceName"; // Will need to get this from combobox
        await initializeExperienceItems(experienceName);
        int nbrItemsPulled = 0;
         
        // Pull Experience type and run
        if (experienceItems.Count > 0)
        {

            pullAndRunNextExpItem();
            GameObject.Find("RunText").GetComponent<UnityEngine.UI.Text>().text =  (++nbrItemsPulled).ToString();
        }
        else
        {
            // Should show some window indicating to the user that the Experience is complete
            // Launch Main Menu
            SceneManager.LoadScene("Challenge1MainMenu");
        }
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
            switch (exp.getType())
            {
                case "Assembly":
                    HandleAssembly(exp.animation);
                    break;
                case "Route":
                    HandleRoute(exp.route);
                    break;

                default:
                    HandleRoute(exp.route);
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
    public void HandleAssembly(ExperienceAnimation exp)
    {
        //Debug.Log(exp);
        Application.OpenURL(exp.getURL());
    }

    /// <summary>
    /// Ideally fetches the Route for the navigation experience and calls the function that
    /// leads the user through PathNav
    /// </summary>
    /// <param name="exp"></param>
    /// @steeve: Connect w/ Dylan to see where that functionality is and call into it.
    public void HandleRoute(ExperienceRoute exp)
    {
        //    Newtonsoft.Json.Linq.JObject parsed = Newtonsoft.Json.Linq.JObject.Parse(json);

        //    foreach (var pair in parsed)
        //    {
        //        Debug.Log($"{pair.Key}: {pair.Value}");
        //    }
        // Add Anchors for the route
        GameObject.Find("AzureSpatialAnchors").GetComponent<AzureSpatialAnchors_CombinedExperience>().
            initializeAnchorKeyList();

        foreach (ExperienceAnchor anchorExp in exp.getAnchors())
        {
            GameObject.Find("AzureSpatialAnchors").GetComponent<AzureSpatialAnchors_CombinedExperience>().
                addAnchorKeyToFind(anchorExp.getAnchorData());
        }
        GameObject.Find("AzureSpatialAnchors").GetComponent<AzureSpatialAnchors_CombinedExperience>().setNbrOfDestAnchors(exp.getAnchors().Count);

        // Run 
        GameObject.Find("AzureSpatialAnchors").GetComponent<AzureSpatialAnchors_CombinedExperience>().
            searchAndBeginNav();
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