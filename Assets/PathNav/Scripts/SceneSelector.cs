// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;


namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class SceneSelector : MonoBehaviour
    {
        public Text SelectedSceneNameText;

        List<int> SceneBuildIndices = new List<int>();
        private int _SceneIndex = -1;
        int SceneIndex
        {
            get
            {
                return _SceneIndex;
            }
            set
            {
                if (_SceneIndex != value)
                {
                    _SceneIndex = value;
                    UpdateSceneText();
                }
            }
        }
        
#pragma warning disable CS1998 // Conditional compile statements are removing await
        async void Start()
#pragma warning restore CS1998
        {
            setupVuforiaStudioLogistics();
            GameObject createFlowBtn = GameObject.Find("CreateFlowButton");
            createFlowBtn.transform.localScale = new Vector3(0, 0, 0);

            GameObject anchorNbrBox = GameObject.Find("AnchorNumberBox");
            anchorNbrBox.transform.localScale = new Vector3(0, 0, 0);

            GameObject mainMenuText = GameObject.Find("CreateAnchorMenuText");
            mainMenuText.transform.localScale = new Vector3(0, 0, 0);

            GameObject exitBtn = GameObject.Find("ExitBtn");
            exitBtn.transform.localScale = new Vector3(0, 0, 0);

            GameObject submitAnchorNameBtn = GameObject.Find("SubmitAnchorNameBtn");
            submitAnchorNameBtn.transform.localScale = new Vector3(0, 0, 0);

            //createAnchorUI.GetComponent<MeshRenderer>().enabled = false;
            /*
            if (SelectedSceneNameText == null)
            {
                Debug.Log("Missing text field");
                return;
            }
            */
            //        Debug.Log("Setting text field to NavigationListAndNavExecution");
            //       SelectedSceneNameText.text = "NavigationListAndNavExecution";



#if !UNITY_EDITOR && (UNITY_WSA || WINDOWS_UWP)
            // Ensure that the device is running a suported build with the spatialperception capability declared.
            bool accessGranted = false;
            try
            {
                Windows.Perception.Spatial.SpatialPerceptionAccessStatus accessStatus = await Windows.Perception.Spatial.SpatialAnchorExporter.RequestAccessAsync();
                accessGranted = (accessStatus == Windows.Perception.Spatial.SpatialPerceptionAccessStatus.Allowed);
            }
            catch {}

            if (!accessGranted)
            {
                Button[] buttons = GetComponentsInChildren<Button>();
                foreach (Button b in buttons)
                {
                    b.gameObject.SetActive(false);
                }

                SelectedSceneNameText.resizeTextForBestFit = true;
                SelectedSceneNameText.verticalOverflow = VerticalWrapMode.Overflow;
                SelectedSceneNameText.text = "Access denied to spatial anchor exporter.  Ensure your OS build is up to date and the spatialperception capability is set.";
                return;
            }
#endif

            GetScenes();

            if (SceneBuildIndices.Count == 0)
            {
                SelectedSceneNameText.text = "No scenes";
                Debug.Log("Not enough scenes in the build");
                return;
            }

            SceneIndex = 0;
        }

        void UpdateSceneText()
        {
            // Unity's scene.name function only works after a scene is loaded
            // so we have to do a little work to get a friendly scene name
            if (SceneIndex >= 0 && SceneIndex < SceneBuildIndices.Count)
            {
                int selected = SceneBuildIndices[SceneIndex];

                // this gets us a string like /Assets/AzureSpatialAnchorsPlugin/Examples/Scenes/AzureSpatialAnchorsSceneName.Unity
                string path = SceneUtility.GetScenePathByBuildIndex(selected);
                // Trim off /Assets/AzureSpatialAnchorsPlugin/Examples/Scenes/AzureSpatialAnchors
                path = path.Substring(path.LastIndexOf('/') + "AzureSpatialAnchors".Length + 1);
                // Trim off .Unity
                path = path.Substring(0, path.LastIndexOf('.'));
                SelectedSceneNameText.text = path;
            }
            else
            {
                SelectedSceneNameText.text = $"Invalid scene id {SceneIndex}";
            }
        }

        void GetScenes()
        {

            Scene currentScene = SceneManager.GetActiveScene();

            for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(index);
                Scene s = SceneManager.GetSceneByPath(path);
                if (s.name == currentScene.name)
                {
                    continue;
                }

                SceneBuildIndices.Add(index);
            }
        }

        public void Next()
        {
            if (SceneBuildIndices.Count == 0)
            {
                return;
            }

            SceneIndex = (SceneIndex + 1) % SceneBuildIndices.Count;
        }

        public void Previous()
        {
            if (SceneBuildIndices.Count == 0)
            {
                return;
            }
            // instead of decrementing and dealing with underflow, 
            // increment by 1 less than the list size, and mod.
            SceneIndex = (SceneIndex + SceneBuildIndices.Count - 1) % SceneBuildIndices.Count;
        }
        public void loadNavigationListAndExecutionScene()
        {
            SceneManager.LoadScene("NavigationListAndNavExecution");
        }

        public void loadCombinedExperience()
        {
            SceneManager.LoadScene("CombinedExperience");
        }

        public void LaunchSelected()
        {

            /*
            if (SceneIndex >= 0 && SceneIndex < SceneBuildIndices.Count)
            {
                SceneManager.LoadScene(SceneBuildIndices[SceneIndex]);
            }
            */
        }

        // @steeve: move this to happen on startup of Unity App
        // handle logistics of Vuforia studio
        /// <summary>
        /// Looks for Vuforia Studio Projects Directory, 
        /// takes every project in there, and makes an object of the projects name, thingworxServer, url, and type (of Assembly)
        /// Saves the list of json to a file called "animations" (looking @ you, Dylan)
        /// </summary>
        public async void setupVuforiaStudioLogistics()
        {
                string username = System.Environment.GetEnvironmentVariable("UserName");
                string docDir = $"C:\\Users\\{username}\\Documents";
                string challenge1Dir = $"{docDir}\\MAARS-C1";

                string c1Dir = $"C:\\Users\\{username}\\Documents\\MAARS-C1\\";
                string expDir = $"C:\\Users\\{username}\\Documents\\MAARS-C1\\Experiences";
                string vuforiaProjectsDir = $"{docDir}\\VuforiaStudio\\Projects";


            // dirs created if not already existing
            System.IO.Directory.CreateDirectory(c1Dir);
                System.IO.Directory.CreateDirectory(expDir);

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

        /// <summary>
        /// An experience can be of type Assembly or of type Route
        /// A full walkthrough will be a list of Experiences (typically as list of JSON)
        /// </summary>
        public class Experience
        {
            public string name;
            public string thingworxServer;
            public string url;
            public string type;

            override public string ToString()
            {
                return $"name: {this.name}\ntype:{this.type}\nurl:{this.url}";
            }
        }
    }
}
