#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
//using UnityEditor.VSAttribution.SubstrateGaming;
using UnityEditor.SceneManagement;

namespace Assets.Scripts
{
    public class StaticWelcomeWindow : EditorWindow
    {
        static readonly Vector2 s_WindowSize = new Vector2(450, 710);

        private Texture2D bannerTexture; // Texture for the banner

        public string actionName;
        public string partnerName;
        public string customerUid;

        [MenuItem("Polkadot SDK/Welcome")]
        public static void Initialize()
        {
            var window = GetWindow<StaticWelcomeWindow>();

            window.titleContent = new GUIContent(".: Polkadot SDK for Unity :.");
            window.minSize = s_WindowSize;
            window.maxSize = s_WindowSize;
        }

        private void OnEnable()
        {
            // Load your banner texture. Make sure the texture is in your Assets folder
            bannerTexture = Resources.Load<Texture2D>("substrate_gaming"); // Replace with your banner image path
        }

        public void OnGUI()
        {

            // Draw the banner
            if (bannerTexture != null)
            {

                GUILayout.Box(bannerTexture, GUILayout.ExpandWidth(true), GUILayout.Height(225));
            }

            // Implementation information
            DrawLine(Color.gray);

            DrawHelpBox("<b><size=14>Welcome to the Polkadot SDK for Unity</size></b>\n\n" +
                "<size=12>Explore the frontier of blockchain in gaming with the Polkadot SDK for Unity. This " +
                "toolkit empowers your Unity projects with seamless blockchain integration, enabling " +
                "innovative gameplay and secure transactions. Get ready to transform your gaming ideas " +
                "into decentralized reality!</size>", 7);


            DrawLine(Color.gray);

            GUILayout.Label("Demo & Tutorials", EditorStyles.boldLabel);

            // Define button size
            float buttonWidth = 100; // Adjust the width as needed
            float buttonHeight = 50; // Adjust the height as needed
                                     // Custom GUIStyle for smaller label text
            GUIStyle smallLabelStyle = new GUIStyle(GUI.skin.label);
            smallLabelStyle.fontSize = 12;
            smallLabelStyle.wordWrap = true;
            smallLabelStyle.richText = true;

            float descriptionWidth = 320; // Adjust the width as needed

            // Demo Explorer Section
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Demo Explorer", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                LoadScene("DemoExplorer"); // Replace with your scene name
            }
            GUILayout.Label("A <b>basic explorer to connect</b> and access some basic functions, if setup can show-case a <b>transfer on a local blockchain</b>!", smallLabelStyle, GUILayout.Width(descriptionWidth));
            GUILayout.EndHorizontal();

            // Demo Wallet Section
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Demo Wallet", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                LoadScene("DemoWallet"); // Replace with your scene name
            }
            GUILayout.Label("An <b>onboarding workflow</b> demo showcasing the <b>account login, creation, import and deletion proccess</b> for a web3 game or app!", smallLabelStyle, GUILayout.Width(descriptionWidth)); // Adjust width as needed
            GUILayout.EndHorizontal();

            // Demo Game Section
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Demo Game", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                LoadScene("DemoGame"); // Replace with your scene name
            }
            GUILayout.Label("Hexalem is an <b>on-chain multiplayer mobile game</b> that showcases the integration of blockchain technology into a gaming experience.", smallLabelStyle, GUILayout.Width(descriptionWidth)); // Adjust width as needed
            GUILayout.EndHorizontal();

            DrawLine(Color.gray);

            // Documentation and Support Section
            GUILayout.Label("Documentation and Support", EditorStyles.boldLabel);

            // Demo Explorer Section
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Documentation", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                Application.OpenURL("https://github.com/SubstrateGaming/Polkadot.Unity.SDK/wiki");
            }
            GUILayout.Label("Our <b>Polkadot SDK for Unity Wiki</b> is maintained on Github, please find <b>informations, code examples, setup guides & tutorials</b>!", smallLabelStyle, GUILayout.Width(descriptionWidth));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Support", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            {
                Application.OpenURL("https://github.com/SubstrateGaming/Polkadot.Unity.SDK/wiki/Support-and-Contact");
            }
            GUILayout.Label("For <b>any questions, technical issues, inquiries, or suggestions</b>, please check out our support process!", smallLabelStyle, GUILayout.Width(descriptionWidth));
            GUILayout.EndHorizontal();

            DrawLine(Color.gray);

            actionName = ""; // Action Name
            partnerName = ""; // VS Partner Name
            customerUid = ""; // VS Customer UID

            //GUILayout.Space(20f);

            //if (GUILayout.Button("Send Attribution", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
            //{
            //    var result = VSAttribution.SendAttributionEvent(actionName, partnerName, customerUid);
            //    Debug.Log($"[VS Attribution] Attribution Event returned status: {result}!");
            //}

        }

        private void LoadScene(string sceneName)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Polkadot Unity SDK/" + sceneName + "/" + sceneName + ".unity");
            }
        }

        #region Utilities
        private void DrawHelpBox(string text, int linesCount)
        {
            GUIStyle helpBox = new GUIStyle(EditorStyles.helpBox) { richText = true };
            EditorGUILayout.SelectableLabel(text, helpBox, GUILayout.Height(linesCount * 14f));
        }

        private void DrawLine(Color color, int thickness = 2, int padding = 7)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding * 2 + thickness));
            r.height = thickness;
            r.y += padding;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        #endregion
    }
}
#endif