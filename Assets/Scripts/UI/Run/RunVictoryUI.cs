using UnityEngine;

public class RunVictoryUI : UIScreen
{
    [SerializeField] TypewriterBox statsBox;

    private RunState run;

    protected override void OnBeforeOpen(object payload)
    {
        run = (RunState)payload;


    }
    protected override void OnOpened()
    {

        statsBox.DisplayText();
    }

    public void ReturnToHub()
    {
        SceneLoader.Instance.LoadHub();
    }
}
