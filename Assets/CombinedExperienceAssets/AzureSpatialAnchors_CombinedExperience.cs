// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

using moveTo = MoveTo;
using System.Net.Http;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class AzureSpatialAnchors_CombinedExperience : DemoScriptBase_CombinedExperience
    {
        internal enum AppState
        {
            DemoStepChooseFlow = 0,
            DemoStepInputAnchorNumber,
            DemoStepCreateSession,
            DemoStepConfigSession,
            DemoStepStartSession,
            DemoStepCreateLocalAnchor,
            DemoStepSaveCloudAnchor,
            DemoStepSavingCloudAnchor,
            DemoStepStopSession,
            DemoStepDestroySession,
            DemoStepCreateSessionForQuery,
            DemoStepStartSessionForQuery,
            DemoStepLookForAnchor,
            DemoStepLookingForAnchor,
            DemoStepStopSessionForQuery,
            DemoStepComplete,
        }
        string anchorNameForCreation;

        internal enum DemoFlow
        {
            CreateFlow = 0,
            LocateFlow
        }

        private readonly Dictionary<AppState, DemoStepParams> stateParams = new Dictionary<AppState, DemoStepParams>
        {
            { AppState.DemoStepChooseFlow,new DemoStepParams() { StepMessage = "Next: Choose your Demo Flow", StepColor = Color.clear }},
            { AppState.DemoStepInputAnchorNumber,new DemoStepParams() { StepMessage = "Next: Input anchor number", StepColor = Color.clear }},
            { AppState.DemoStepCreateSession,new DemoStepParams() { StepMessage = "Enter Anchor Name", StepColor = Color.clear }},
            { AppState.DemoStepConfigSession,new DemoStepParams() { StepMessage = "Next: Configure CloudSpatialAnchorSession", StepColor = Color.clear }},
            { AppState.DemoStepStartSession,new DemoStepParams() { StepMessage = "Next: Start CloudSpatialAnchorSession", StepColor = Color.clear }},
            { AppState.DemoStepCreateLocalAnchor,new DemoStepParams() { StepMessage = "Tap a surface to add the local anchor.", StepColor = Color.blue }},
            { AppState.DemoStepSaveCloudAnchor,new DemoStepParams() { StepMessage = "Next: Save local anchor to cloud", StepColor = Color.yellow }},
            { AppState.DemoStepSavingCloudAnchor,new DemoStepParams() { StepMessage = "Saving local anchor to cloud...", StepColor = Color.yellow }},
            { AppState.DemoStepStopSession,new DemoStepParams() { StepMessage = "Next: Stop cloud anchor session", StepColor = Color.green }},
            { AppState.DemoStepDestroySession,new DemoStepParams() { StepMessage = "Next: Destroy Cloud Anchor session", StepColor = Color.clear }},
            { AppState.DemoStepCreateSessionForQuery,new DemoStepParams() { StepMessage = "Next: Create CloudSpatialAnchorSession for query", StepColor = Color.clear }},
            { AppState.DemoStepStartSessionForQuery,new DemoStepParams() { StepMessage = "Next: Start CloudSpatialAnchorSession for query", StepColor = Color.clear }},
            { AppState.DemoStepLookForAnchor,new DemoStepParams() { StepMessage = "Next: Look for anchor", StepColor = Color.clear }},
            { AppState.DemoStepLookingForAnchor,new DemoStepParams() { StepMessage = "Looking for anchor...", StepColor = Color.clear }},
            { AppState.DemoStepStopSessionForQuery,new DemoStepParams() { StepMessage = "Next: Stop CloudSpatialAnchorSession for query", StepColor = Color.yellow }},
            { AppState.DemoStepComplete,new DemoStepParams() { StepMessage = "Next: Restart demo", StepColor = Color.clear }}
        };

#if !UNITY_EDITOR
            public AnchorExchanger_CombinedExperience anchorExchanger = new AnchorExchanger_CombinedExperience();
#endif
        #region Member Variables
        private AppState _currentAppState = AppState.DemoStepChooseFlow;
        private DemoFlow _currentDemoFlow = DemoFlow.CreateFlow;
        private readonly List<GameObject> otherSpawnedObjects = new List<GameObject>();
        private int anchorsLocated = 0;
        private int anchorsExpected = 0;
        private readonly List<string> localAnchorIds = new List<string>();
        //private string _anchorKeyToFind = null;
        private List<string> _anchorKeyToFind = null;
        //private long? _anchorNumberToFind;
        private List<string> _anchorNameToFind = null;
        bool navigationStarted = false;
        bool canEnableBeginNavButton = false;
        int nbrOfDestinationAnchors;

        #endregion // Member Variables

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The base URL for the sharing service.")]
        private string baseSharingUrl = "https://sharingservice20200308094713.azurewebsites.net";
        #endregion // Unity Inspector Variables

        private AppState currentAppState
        {
            get
            {
                return _currentAppState;
            }
            set
            {
                if (_currentAppState != value)
                {
                    Debug.LogFormat("State from {0} to {1}", _currentAppState, value);
                    _currentAppState = value;
                    if (spawnedObjectMat != null)
                    {
                        spawnedObjectMat.color = stateParams[_currentAppState].StepColor;
                    }
                    if (feedbackBox == null)
                        Debug.LogFormat("feedbackbox is null******");
                    else
                    feedbackBox.text = stateParams[_currentAppState].StepMessage;
                        EnableCorrectUIControls();
                }
            }
        }
        public int getNbrDestAnchors()
        {
            return nbrOfDestinationAnchors;
        }

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            base.OnCloudAnchorLocated(args);

            Debug.Log("about to check expiration");
           // if ((args != null) && (args.Anchor != null))
            //    Debug.Log("Expiration Date for this anchor is " + args.Anchor.Expiration);


            if (args.Status == LocateAnchorStatus.Located)
            {
                CloudSpatialAnchor nextCsa = args.Anchor;
                currentCloudAnchor = args.Anchor;
       //         Debug.Log("Expiration Date for this anchor is " + nextCsa.Expiration);


                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    anchorsLocated++;
                    currentCloudAnchor = nextCsa;
                    Pose anchorPose = Pose.identity;

                    #if UNITY_ANDROID || UNITY_IOS
                    anchorPose = currentCloudAnchor.GetPose();
                    #endif
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                    GameObject nextObject = SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation, currentCloudAnchor);
                    //spawnedObjectMat = nextObject.GetComponent<MeshRenderer>().material;
                    Debug.Log("Setting goal in shareddemo");

                    //GameObject.Find("arrow").GetComponent<moveTo>().setGoal(nextObject.transform, nextObject.name);

#if !UNITY_EDITOR

                    //Instead of setting anchor is up as destination, add the game object to the flag list for later use
                    GameObject.Find("listOfFlagsGameObj_CombinedExperience").GetComponent<ListOps>().addFlag(nextObject);
                    Debug.Log("********************************************added next Object: " + nextObject.transform.position + ". Main camera's location is " + Camera.main.transform.position + ". other position is " + GameObject.Find("CameraParent").transform.position);

                    // Only start navigation if there are destination flags

                    //      AttachTextMesh(nextObject, _anchorNumberToFind);

#endif
                    otherSpawnedObjects.Add(nextObject);
                    Debug.Log("Adding " + nextObject);

                    if (anchorsLocated >= nbrOfDestinationAnchors)
                    {
                        currentAppState = AppState.DemoStepStopSessionForQuery;
                    }
                    else
                    {
                        Debug.Log("only " + anchorsLocated + " anchors have so far been located");
                    }
                });
            }
        }

        public void beginNav()
        {
            if ((navigationStarted == false) && (GameObject.Find("listOfFlagsGameObj_CombinedExperience").GetComponent<ListOps>().flags.Count > 0))
            {
                //if (GameObject.Find("listOfFlagsGameObj_CombinedExperience").GetComponent<ListOps>().flags.Count == nbrOfDestinationAnchors)
                GameObject.Find("CameraParent").GetComponent<CaptureDistance_CombinedExperience>().beginNavigation();
                navigationStarted = true;
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        public override void Start()
        {
            base.Start();

            /*
            HttpClient c = new HttpClient();
            Task<string> t = c.GetStringAsync(BaseSharingUrl + "/api/anchors/all");
            string s = t.Result;
            Console.WriteLine(s);
            */
            Debug.LogError("Clicked button. made it to 178");

            if (!SanityCheckAccessConfiguration())
            {
                XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject.SetActive(false);
                XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(false);
                XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject.SetActive(false);
                return;
            }

            SpatialAnchorSamplesConfig samplesConfig = Resources.Load<SpatialAnchorSamplesConfig>("SpatialAnchorSamplesConfig");
            if (string.IsNullOrWhiteSpace(BaseSharingUrl) && samplesConfig != null)
            {
                BaseSharingUrl = samplesConfig.BaseSharingURL;
            }

            if (string.IsNullOrEmpty(BaseSharingUrl))
            {
                feedbackBox.text = $"Need to set {nameof(BaseSharingUrl)}.";
                XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject.SetActive(false);
                XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(false);
                XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject.SetActive(false);
                return;
            }
            else
            {
                Uri result;
                if (!Uri.TryCreate(BaseSharingUrl, UriKind.Absolute, out result))
                {
                    feedbackBox.text = $"{nameof(BaseSharingUrl)} is not a valid url";
                    return;
                }
                else
                {
                    BaseSharingUrl = $"{result.Scheme}://{result.Host}/api/anchors";
                }
            }
            Debug.LogError("Clicked button. made it to 215");

            #if !UNITY_EDITOR
            anchorExchanger.WatchKeys(BaseSharingUrl);
            #endif

            feedbackBox.text = stateParams[currentAppState].StepMessage;

            EnableCorrectUIControls();

            Debug.LogError("Clicked button. made it to 225");

            // Jump right into locating anchors.
        //    InitializeLocateFlowDemo();

        }
        public void searchOrBeginNav()
        {
            // Get status of Button Text, if it is "Search", run search. Otherwise, v
            if (canEnableBeginNavButton == false) // if Search button has been NOT been clicked
            {
                InitializeLocateFlowDemo();
                //feedbackBox = GameObject.Find("CreateFlowText").transform.GetComponent<UnityEngine.UI.Text>();

                canEnableBeginNavButton = true;
                GameObject searchOrBeginNavButton = GameObject.Find("Run");
                searchOrBeginNavButton.transform.Find("RunText").GetComponent<UnityEngine.UI.Text>().text = "Begin Nav";
            }
            else
                beginNav();
        }

        public void submitAnchorName()
        {
            anchorNameForCreation = GameObject.Find("AnchorNumberBox").transform.GetChild(1).transform.GetComponent<UnityEngine.UI.Text>().text;

            if (!anchorNameForCreation.Equals(""))
            {
                // Disable the text field
                GameObject anchorNbrBox = GameObject.Find("AnchorNumberBox");
                anchorNbrBox.transform.localScale = new Vector3(0, 0, 0);
                // Disable the submit button
                GameObject submitAnchorNameBtn = GameObject.Find("SubmitAnchorNameBtn");
                submitAnchorNameBtn.transform.localScale = new Vector3(0, 0, 0);
                // Change the Text to the field to new name
                //GameObject.Find("CreateAnchorMenuText").transform.GetComponent<UnityEngine.UI.Text>().text = "New Anchor Name: " + anchorNameForCreation;
                feedbackBox.text = "New Anchor Name: " + anchorNameForCreation; 
            }
        }

        public async void createAnchorButtonClicked()
        {
            // Disable the Regular UI
            GameObject mainUI = GameObject.Find("MainMenuPanel_Navigation");
            //mainUI.GetComponent<MeshRenderer>().enabled = false;
            mainUI.transform.localScale = new Vector3(0, 0, 0);


            _currentDemoFlow = DemoFlow.CreateFlow;
            currentAppState = AppState.DemoStepCreateSession;

            // Enable the Create Anchor UI
            GameObject createFlowBtn = GameObject.Find("CreateFlowButton");
            createFlowBtn.transform.localScale = new Vector3(1, 1, 1);

            GameObject anchorNbrBox = GameObject.Find("AnchorNumberBox");
            anchorNbrBox.transform.localScale = new Vector3(1, 1, 1);

            GameObject mainMenuText = GameObject.Find("CreateAnchorMenuText");
            mainMenuText.transform.localScale = new Vector3(1, 1, 1);

            GameObject exitBtn = GameObject.Find("ExitBtn");
            exitBtn.transform.localScale = new Vector3(1, 1, 1);

            GameObject submitAnchorNameBtn = GameObject.Find("SubmitAnchorNameBtn");
            submitAnchorNameBtn.transform.localScale = new Vector3(1, 1, 1);


            // Verify input from user
            //    anchorNameForCreation = GameObject.Find("AnchorNumberBox").transform.GetChild(0).transform.GetComponent<UnityEngine.UI.Text>().text;
            /*
                while (anchorNameForCreation.Equals(""))
                {
                    feedbackBox.text = "Provide Name for Spatial Anchor";
                }
                */
            //   ConfigureSession();
            //   currentAppState = AppState.DemoStepStartSession;

            /*
            await CloudManager.StartSessionAsync();
            currentAppState = AppState.DemoStepCreateLocalAnchor;
            */
            await AdvanceCreateFlowDemoAsync();
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (spawnedObjectMat != null)
            {
                float rat = 0.1f;
                float createProgress = 0f;
                if (CloudManager.SessionStatus != null)
                {
                    createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                }
                rat += (Mathf.Min(createProgress, 1) * 0.9f);
                spawnedObjectMat.color = stateParams[currentAppState].StepColor * rat;
            }
        }

        protected override bool IsPlacingObject()
        {
            return currentAppState == AppState.DemoStepCreateLocalAnchor;
        }

        protected override Color GetStepColor()
        {
            if (currentCloudAnchor == null || localAnchorIds.Contains(currentCloudAnchor.Identifier))
            {
                return stateParams[currentAppState].StepColor;
            }

            return Color.magenta;
        }

        private void AttachTextMesh(GameObject parentObject, string anchorName)
        {
            GameObject go = new GameObject();

            TextMesh tm = go.AddComponent<TextMesh>();
            if (anchorName.Equals(""))
            {
                tm.text = string.Format("{0}:{1}", localAnchorIds.Contains(currentCloudAnchor.Identifier) ? "L" : "R", currentCloudAnchor.Identifier);
            }
            else if (!anchorName.Equals(""))
            {
                tm.text = $"Anchor Number:{anchorName}";
            }
            else
            {
                tm.text = $"Failed to store the anchor key using '{BaseSharingUrl}'";
            }
            tm.fontSize = 32;
            go.transform.SetParent(parentObject.transform, false);
            go.transform.localPosition = Vector3.one * 0.25f;
            go.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
            go.transform.localScale = Vector3.one * .1f;

            otherSpawnedObjects.Add(go);
        }

#pragma warning disable CS1998 // Conditional compile statements are removing await
        protected override async Task OnSaveCloudAnchorSuccessfulAsync()
#pragma warning restore CS1998

        {
            await base.OnSaveCloudAnchorSuccessfulAsync();

            string anchorName = anchorNameForCreation
                ;

            localAnchorIds.Add(currentCloudAnchor.Identifier);

#if !UNITY_EDITOR
            anchorName = (await anchorExchanger.StoreAnchorKey(currentCloudAnchor.Identifier, anchorName, currentCloudAnchor.Expiration.ToString()));
            //anchorName = (await anchorExchanger.StoreAnchorKey(currentCloudAnchor.Identifier, anchorName, ""));
#endif

            Pose anchorPose = Pose.identity;

            #if UNITY_ANDROID || UNITY_IOS
            anchorPose = currentCloudAnchor.GetPose();
            #endif
            // HoloLens: The position will be set based on the unityARUserAnchor that was located.

            SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

            AttachTextMesh(spawnedObject, anchorName);

            currentAppState = AppState.DemoStepStopSession;

            feedbackBox.text = $"Created anchor {anchorName}. Next: Stop cloud anchor session";
        }

        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
        }

        public async override Task AdvanceDemoAsync()
        {
            if (currentAppState == AppState.DemoStepChooseFlow || currentAppState == AppState.DemoStepInputAnchorNumber)
            {
                return;
            }

            if (_currentDemoFlow == DemoFlow.CreateFlow)
            {
                await AdvanceCreateFlowDemoAsync();
            }
            else if (_currentDemoFlow == DemoFlow.LocateFlow)
            {
                await AdvanceLocateFlowDemoAsync();
            }
        }

        public async Task InitializeCreateFlowDemoAsync()
        {
            if (currentAppState == AppState.DemoStepChooseFlow)
            {
                _currentDemoFlow = DemoFlow.CreateFlow;
                currentAppState = AppState.DemoStepCreateSession;
            }
            else
            {
                await AdvanceDemoAsync();
            }
        }

        /// <summary>
        /// This version only exists for Unity to wire up a button click to.
        /// If calling from code, please use the Async version above.
        /// </summary>
        public async void InitializeCreateFlowDemo()
        {
            try
            {
                await InitializeCreateFlowDemoAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(AzureSpatialAnchors_CombinedExperience)} - Error in {nameof(InitializeCreateFlowDemo)}: {ex.Message}");
            }
        }


#pragma warning disable CS1998 // Conditional compile statements are removing await
        public async Task InitializeLocateFlowDemoAsync()
#pragma warning restore CS1998
        {
            string currentAnchorKey = "";
            Debug.LogError("inside ");

            if (currentAppState == AppState.DemoStepChooseFlow)
            {
                currentAppState = AppState.DemoStepInputAnchorNumber;
            }
            else if (currentAppState == AppState.DemoStepInputAnchorNumber)
            {
                string anchorName;
                // string inputText = XRUXPickerForMainMenu.Instance.GetDemoInputField().text;

                // _anchorNumberToFind = anchorName;
                // This is where I need to change _anchorKeyToFind to a list and cycle through all rowKeys (of interest. Statically set for now) and add them to _anchorKeyToFindList
                // For now it will ignore user's actual input
                _anchorNameToFind = new List<string>();
                _anchorKeyToFind = new List<String>();

                // Add rowkeys
                // Get list of Anchors to add from the Anchor List UI
                if ((GameObject.Find("Content") == null) || (GameObject.Find("Content").transform == null))
                    Debug.Log("Null in initlocateflowasync at (0)");
                Transform anchorListTransform = GameObject.Find("Content").transform;

                nbrOfDestinationAnchors = 0;
                // Now adding directly to the list containing the anchor keys, not the rows. 
                //_anchorNumberToFind.Add(24); // Add first flag
                //_anchorNumberToFind.Add(25); // Add second flag

                Debug.LogError("added anchors to the list that need to be searched");

                // Add anchor keys
#if !UNITY_EDITOR
                    Debug.Log("ONLY ADDING 2 ANCHORS TO SEARCH FOR !!***********************************************************************************!!");

                    // Now adding directly to the list containing the anchor keys, not the rows. 
                    //string currentAnchorKey = await anchorExchanger.RetrieveAnchorKey(_anchorNameToFind[i]);
                    
                int i = 0;
                    foreach (Transform child in anchorListTransform)
                    {
                        if (((child.Find("Toggle" + i).GetComponent<Toggle>().isOn == true) && child.Find("Toggle" + i)) && (child.Find("Toggle" + i).Find("AnchorContainer").gameObject.transform.childCount > 0))
                        {
                                    Debug.Log("In initlocateflowasync, made it to (1)");

                            // First need to check if actually selected (above)
                            try
                            {

                                _anchorKeyToFind.Add(child.Find("Toggle" + i).Find("AnchorContainer").gameObject.transform.GetChild(0).name);
                                                    Debug.Log("In initlocateflowasync, made it to (2)");

                            }
                            catch(Exception)
                            {
                                Debug.Log("bad value was attempted to be added to _anchorKeyToFind. value was " + (currentAnchorKey != "" ? currentAnchorKey : ""));
                            }
                            nbrOfDestinationAnchors++;
                        }
                        i++;
                    }
#endif

                    //_anchorKeyToFind = await anchorExchanger.RetrieveAnchorKey(_anchorNameToFind.Value);
                    if (_anchorKeyToFind == null)
                    {
                        if (feedbackBox == null)
                            Debug.Log("FEEDBACK TEXT IS NULL!! (4)");
                            feedbackBox.text = "Anchor Number Not Found!";
                    }
                    else
                    {
                        _currentDemoFlow = DemoFlow.LocateFlow;
                        //currentAppState = AppState.DemoStepCreateSession;
                        currentAppState = AppState.DemoStepCreateSessionForQuery;
                    //   XRUXPickerForMainMenu.Instance.GetDemoInputField().text = "";


                    // Just added this:
                    Debug.Log("Going into AdvanceLocateFlowDemoAsync()");

                    await AdvanceLocateFlowDemoAsync();

                }
            }
            else
            {
                await AdvanceDemoAsync();
            }
        }

        /// <summary>
        /// This version only exists for Unity to wire up a button click to.
        /// If calling from code, please use the Async version above.
        /// </summary>
        public async void InitializeLocateFlowDemo()
        {
            Debug.LogError("made it inside InitializeLocateFlowDemo()");

            try
            {
                currentAppState = AppState.DemoStepInputAnchorNumber;
                await InitializeLocateFlowDemoAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"{nameof(AzureSpatialAnchors_CombinedExperience)} - Error in {nameof(InitializeLocateFlowDemo)}: {ex.Message}");
            }
        }
        

        private async Task AdvanceCreateFlowDemoAsync()
        {
            Debug.LogError("made it inside AdvanceCreateFlowDemoAsync()");

            switch (currentAppState)
            {
                case AppState.DemoStepCreateSession:
                    Debug.LogError("assigning currentCloudAnchor to null");
                    Debug.LogError("setting currentAppState to AppState.DemoStepConfigSession");

                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepConfigSession;
                    break;
                case AppState.DemoStepConfigSession:
                    Debug.LogError("about to configure session");

                    ConfigureSession(); // Jumping to here.
                    currentAppState = AppState.DemoStepStartSession;
                    break;
                case AppState.DemoStepStartSession: // Jumping to here.
                    await CloudManager.StartSessionAsync();
                    currentAppState = AppState.DemoStepCreateLocalAnchor;
                    break;
                case AppState.DemoStepCreateLocalAnchor: // This is set by "IsPlacingObject()"
                    if (spawnedObject != null)
                    {
                        currentAppState = AppState.DemoStepSaveCloudAnchor;
                    }
                    break;
                case AppState.DemoStepSaveCloudAnchor:
                    currentAppState = AppState.DemoStepSavingCloudAnchor;
                    await SaveCurrentObjectAnchorToCloudAsync();
                    break;
                case AppState.DemoStepStopSession:
                    CloudManager.StopSession();
                    CleanupSpawnedObjects();
                    await CloudManager.ResetSessionAsync();
                    currentAppState = AppState.DemoStepComplete;
                    break;
                case AppState.DemoStepComplete:
                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepChooseFlow;
                    CleanupSpawnedObjects();
                    break;
                default:
                    Debug.Log("Shouldn't get here for app state " + currentAppState);
                    break;
            }
        }

        private async Task AdvanceLocateFlowDemoAsync()
        {
            switch (currentAppState)
            {
                case AppState.DemoStepCreateSession:
                    currentAppState = AppState.DemoStepChooseFlow;
                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepCreateSessionForQuery;
                    break;
                case AppState.DemoStepCreateSessionForQuery:
                    Debug.Log("Creating session for query.. inside the main area that looks for anchors and creates session");
                    anchorsLocated = 0;
                    ConfigureSession();
                    currentAppState = AppState.DemoStepStartSessionForQuery;
                    Debug.Log("starting session for query..");


                    await CloudManager.StartSessionAsync();
                    Debug.Log("starting DemoStepLookForAnchor..");

                    currentAppState = AppState.DemoStepLookForAnchor;
                    Debug.Log("starting DemoStepLookingForAnchor..");


                    currentAppState = AppState.DemoStepLookingForAnchor;
                    Debug.Log("starting DemoStepLookingForAnchor..");

                    currentWatcher = CreateWatcher();

                    // DemoStepStopSessionForQuery is set automatically by the anchor located event handler

                    /////////////////////


                    break;

    /*
                case AppState.DemoStepStartSessionForQuery:
                    await CloudManager.StartSessionAsync();
                    currentAppState = AppState.DemoStepLookForAnchor;
                    break;
                case AppState.DemoStepLookForAnchor:
                    currentAppState = AppState.DemoStepLookingForAnchor;
                    currentWatcher = CreateWatcher();
                    break;
                case AppState.DemoStepLookingForAnchor:
                    // Advancement will take place when anchors have all been located.
                    break;
    *//*
                case AppState.DemoStepStopSessionForQuery:
                    CloudManager.StopSession();
                    currentAppState = AppState.DemoStepComplete;
                    break;
   
                case AppState.DemoStepComplete:
                    currentCloudAnchor = null;
                    currentWatcher = null;
                    currentAppState = AppState.DemoStepChooseFlow;
                    CleanupSpawnedObjects();
                    break;
                default:
                    Debug.Log("Shouldn't get here for app state " + currentAppState);
                    break;
        */
            }
        }

        private void EnableCorrectUIControls()
        {
            /*
            Debug.Log("Buttons labels are the following" + XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].name + ", " + XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].name +
                XRUXPickerForMainMenu.Instance.GetDemoButtons()[2].name);
            if (currentAppState == null)
                Debug.Log("in EnableCorrectUIControls(), currentAppState is null");
            else
                Debug.Log("in EnableCorrectUIControls(), currentAppState is " + currentAppState);
            // if (XRUXPickerForMainMenu == null)
            //    Debug.Log("********************************XRUXPickerForMainMenu is null!!****************************");
            if (XRUXPickerForMainMenu.Instance == null)
              Debug.Log("********************************XRUXPickerForMainMenu.Instance is " + XRUXPickerForMainMenu.Instance);


            if (XRUXPickerForMainMenu.Instance.GetDemoButtons()[0] == null)
            {
                Debug.Log("********************************first button is null!!****************************");
            }
            if (XRUXPickerForMainMenu.Instance.GetDemoButtons()[1] == null)
            {
                Debug.Log("********************************2nd button is null!!****************************");
            }

            switch (currentAppState)
            {
                case AppState.DemoStepChooseFlow:

                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject.SetActive(true);
#if UNITY_WSA
                    XRUXPickerForMainMenu.Instance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 0.1f;
                    XRUXPickerForMainMenu.Instance.transform.LookAt(Camera.main.transform);
                    XRUXPickerForMainMenu.Instance.transform.Rotate(Vector3.up, 180);
                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(true);
#else
              //      XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].transform.Find("Text").GetComponent<Text>().text = "Create & Share Anchor";
#endif
                    XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject.SetActive(false);
                    break;
                case AppState.DemoStepInputAnchorNumber:
                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject.SetActive(true);
                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(false);
                    XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject.SetActive(true);
                    break;
                default:
                    if (XRUXPickerForMainMenu.Instance.GetDemoButtons() == null)
                        Debug.Log("XRUXPickerForMainMenu.Instance.GetDemoButtons() is null");
                    else if (XRUXPickerForMainMenu.Instance.GetDemoButtons()[1] == null)
                        Debug.Log("XRUXPickerForMainMenu.Instance.GetDemoButtons()[1] is null");
                    else if (XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject == null)
                        Debug.Log("XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject is null");
                    else
                        XRUXPickerForMainMenu.Instance.GetDemoButtons()[1].gameObject.SetActive(false);
#if UNITY_WSA
                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(false);
#else
                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].gameObject.SetActive(true);

                    XRUXPickerForMainMenu.Instance.GetDemoButtons()[0].transform.Find("Text").GetComponent<Text>().text = "Next Step";
                    #endif
                    if ((XRUXPickerForMainMenu.Instance.GetDemoInputField() == null) || (XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject == null))
                        Debug.Log("XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject is null");
                    else
                        XRUXPickerForMainMenu.Instance.GetDemoInputField().gameObject.SetActive(false); // "enter cloud anchor session for query.."
                    break;
            }
            */
            
        }

        private void ConfigureSession()
        {
            List<string> anchorsToFind = new List<string>();
            Debug.Log("Configuring session.. with currentAppState being set to " + AppState.DemoStepCreateSessionForQuery);

            if (currentAppState == AppState.DemoStepCreateSessionForQuery)
            {
                Debug.Log("*** at lin 574, _anchorKeyTOFind is " + (_anchorKeyToFind == null ? "null" : "not null"));

                // Should change logic to go through all of the _anchorsToFindList, and set 
                // Remember that _anchorKeyToFind is not the same as rowkey!
                for (int i = 0; i < _anchorKeyToFind.Count; i++)
                    anchorsToFind.Add(_anchorKeyToFind[i]);
            }
            {
                Debug.Log("*********************************** number of expected anchors is " + nbrOfDestinationAnchors);

                //anchorsExpected = anchorsToFind.Count;
                anchorsExpected = nbrOfDestinationAnchors;

                SetAnchorIdsToLocate(anchorsToFind);

            }
        }

        protected override void CleanupSpawnedObjects()
        {
            base.CleanupSpawnedObjects();

            for (int index = 0; index < otherSpawnedObjects.Count; index++)
            {
                Destroy(otherSpawnedObjects[index]);
            }

            otherSpawnedObjects.Clear();
        }

        /// <summary>
        /// Gets or sets the base URL for the example sharing service.
        /// </summary>
        public string BaseSharingUrl { get => baseSharingUrl; set => baseSharingUrl = value; }
    }
}
