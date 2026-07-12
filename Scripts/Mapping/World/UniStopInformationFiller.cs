using UnityEngine;
using UnityEngine.UIElements;

public class UniStopInformationFiller : MonoBehaviour
{
    private VisualElement informationScreenBackground;
    private VisualElement root;

    public void Initialise(string title, string description)
    {
        var uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;
        root.Q<Label>("StopTitle").text = title;
        root.Q<Label>("StopDescription").text = description;
        informationScreenBackground = root.Q<VisualElement>("InformationBackground");

        if (informationScreenBackground != null)
            informationScreenBackground.RegisterCallback<ClickEvent>(BackgroundTapped);

        root.style.display = DisplayStyle.None;
    }


    private void BackgroundTapped(ClickEvent evt)
    {
        Close();
    }


    public void Open()
    {
        if (root != null) root.style.display = DisplayStyle.Flex;
    }


    public void Close()
    {
        if (root != null) root.style.display = DisplayStyle.None;
    }

    // Should switch this to just open instead I think but
    // then requires a close button / different way to close it 
    public void Toggle()
    {
        if (root == null) return;

        var isVisible = root.style.display == DisplayStyle.Flex;

        if (isVisible)
            Close();
        else
            Open();

        //root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
    }
}