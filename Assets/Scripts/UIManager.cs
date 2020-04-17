using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
public class UIManager : MonoBehaviour, IUIButtons
{
    public Text DayText;
    public Text TimeText;
    public Text WorkingTimeText;
    IDateManager fileManager;
    DateTime TextTime;
    private void Update()
    {
        if (TextTime.Minute != DateTime.Now.Minute)
        {
            Show();
        }
        if (TextTime.Second != DateTime.Now.Second)
        {
            TimeShow();
            TextTime = DateTime.Now;
        }
    }
    private void Start()
    {
        string path = $"{ Application.dataPath}/勤務データ";
        fileManager = new DateManager(path);
        Screen.SetResolution(540, 960, false, 10);
        Show();
        TimeShow();
        TextTime = DateTime.Now;
        ButtonActiveControl();
    }
    void Show()
    {
        DayTextShow();
        WorkingTimeShow();
    }
    bool flashing;
    private void TimeShow()
    {
        var partition = flashing ? ":" : " ";
        flashing = !flashing;
        TimeText.text = DateTime.Now.ToString($"HH {partition} mm");
    }

    private void WorkingTimeShow()
    {
        WorkingTimeText.text = fileManager.GetToDayWorkLog().GetWorkingTime().ToString(@"hh\ \時\間\ mm\ \分");
    }

    private void DayTextShow()
    {
        var culture = CultureInfo.GetCultureInfo("ja-JP");
        DayText.text = DateTime.Now.ToString("yyyy/MM/dd(ddd)", culture);
    }

    public void StartButton()
    {
        fileManager.SendStatus(Status.出勤);
        ButtonActiveControl();
    }
    public void ExitButton()
    {
        fileManager.SendStatus(Status.退勤);
        ButtonActiveControl();
    }
    public void PauseButton()
    {
        fileManager.SendStatus(Status.休入);
        ButtonActiveControl();
    }
    public void ResumeButton()
    {
        fileManager.SendStatus(Status.休終);
        ButtonActiveControl();
    }

    [SerializeField]
    GameObject[] Buttons;
    private void ButtonActiveControl()
    {
        int index = 0;
        foreach (var item in Buttons)
        {
            Status status = (Status)index;
            if (fileManager.CanPassStatus(status))
            {
                ButtonOn(item);
            }
            else
            {
                ButtonOff(item);
            }
            index++;
        }
    }
    private void ButtonOff(GameObject obj)
    {
        float Color_a = 120f / 255f;
        var image = obj.GetComponent<Image>();
        if (image != null)
        {
            var color = image.color;
            color.a = Color_a;
            image.color = color;
        }
        var Button = obj.GetComponent<Button>();
        if (Button != null)
        {
            Button.enabled = false;
        }
        var text = obj.transform.GetChild(0).GetComponent<Text>();
        if (text != null)
        {
            var color = text.color;
            color.a = Color_a;
            text.color = color;
        }
    }
    private void ButtonOn(GameObject obj)
    {
        float Color_a = 255f / 255f;
        var image = obj.GetComponent<Image>();
        if (image != null)
        {
            var color = image.color;
            color.a = Color_a;
            image.color = color;
        }
        var Button = obj.GetComponent<Button>();
        if (Button != null)
        {
            Button.enabled = true;
        }
        var text = obj.transform.GetChild(0).GetComponent<Text>();
        if (text != null)
        {
            var color = text.color;
            color.a = Color_a;
            text.color = color;
        }
    }
}
