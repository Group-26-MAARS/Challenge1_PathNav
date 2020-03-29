using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

public class AnchorRowOps : MonoBehaviour
{
    #region Unity Inspector Variables
    [SerializeField]
    [Tooltip("The base URL for the sharing service.")]
    private string baseSharingUrl = "https://sharingservice20200308094713.azurewebsites.net";
    #endregion // Unity Inspector Variables

    public GameObject anchorDisplayRow;

    // Start is called before the first frame update
    void Start()
    {
        /*
        HttpClient c = new HttpClient();
        Task<string> t = c.GetStringAsync(baseSharingUrl + "/api/anchors/all");
        string s = t.Result;
                System.Console.WriteLine(s);

        */
        string[] tempStr = {"1 : 1",
          "2 : 6647aab9-119f-47a0-ad9c-e616c9db83ce",
          "3 : fc1537b9-338b-48cc-a1a9-897e54516688",
          "4 : b8bd3801-4863-4123-8c75-5f1276f7e751",
          "5 : 2198531b-616a-407b-8409-cec071c0ac4f",
          "6 : 1a383c14-41bf-4517-8d6e-a0ea12ee7e3c",
          "7 : 7c005465-f89d-4342-92af-7bcbc7c2ce33",
          "8 : 74d090b6-ae3b-4c5e-a703-8a22cbfad966",
          "9 : 336842eb-4e85-4d92-8e06-6f351538a038",
          "10 : 149bf5cf-72f1-43fd-8acb-b802c14252a3",
          "11 : 9b31c622-11fd-4cad-94f3-8b0c8df42225",
          "12 : 2c04bf15-8a27-4b75-a2fc-b1dd4bf34cf4",
          "13 : 608cd8e6-cc6a-4433-990e-cbf12d8de127",
          "14 : 934e2dfb-5a44-451a-ba11-b15bbfd6cc36",
          "15 : b463be31-999f-45f1-8a4a-bf4d51249ce7",
          "16 : 4e540f63-c3ce-434a-8ac5-e6c5f86cf1b0",
          "17 : 39d296b6-a1cc-45eb-8948-29213b5bd8d1",
          "18 : adba96bc-6887-424b-998b-b7fd3b21e968",
          "19 : 6e4d2638-1cf2-4a5f-a22f-1914d9653798" };

        for (int i = 0; i < tempStr.Length; i++)
        {
            GameObject currentRow = Instantiate(anchorDisplayRow);
            currentRow.transform.SetParent(transform);
            currentRow.name = "AnchorDisplayRow" + i;
            currentRow.transform.localScale = new Vector3((float)1, (float)0.94791, (float)1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
