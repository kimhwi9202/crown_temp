using UnityEngine;
using System.Collections;

namespace xLIB
{
    public class FPS : MonoBehaviour
    {
        /*
        public float updateInterval = 1.0F;
        private int frames = 0;
        private int gotIntervals = 0;
        private double timeleft;
        private double fps = 15.0;
        private double lastSample;
        private double accum = 0.0;
        private int fontsize = 20;

        // Use this for initialization
        void Start()
        {
            timeleft = updateInterval;
            lastSample = Time.realtimeSinceStartup;
        }

        double GetFPS() { return fps; }
        bool HasFPS() { return gotIntervals > 2; }

        // Update is called once per frame
        void Update()
        {
            ++frames;
            double newSample = Time.realtimeSinceStartup;// 초단위의 시간을 게임 실행 후 실시간값을 알려준다.
            double deltaTime = newSample - lastSample;  //시간 갭 체크
            lastSample = newSample;

            timeleft -= deltaTime;
            accum += (1.0 / deltaTime);

            if (timeleft <= 0.0)
            {
                fps = accum / frames;
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
                ++gotIntervals;
            }
        }

        void OnGUI()
        {
            //GUI.skin.label.font = GUI.skin.button.font = GUI.skin.box.font = font;
            GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = fontsize;

            GUI.Box(new Rect(Screen.width - 380, 10, 370, 40), "FPS: " + fps.ToString("f2") + " | QualityLevel : " + QualitySettings.GetQualityLevel()); // 그래픽 퀄리시 레벨.
        }
        */
        float deltaTime = 0.0f;

        void Update()
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 3 / 100);
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = h * 3 / 100;
            style.normal.textColor = Color.yellow;// new Color(0.0f, 0.0f, 0.5f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}