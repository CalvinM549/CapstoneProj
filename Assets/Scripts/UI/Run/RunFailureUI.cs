using UnityEngine;

public class RunFailureUI : UIScreen
{
    [SerializeField] private TerminalDisplay terminal;

    private RunState run;

    protected override void OnBeforeOpen(object payload)
    {
        run = (RunState)payload;


    }
    protected override void OnOpened()
    {
        terminal.Play();

    }

    public void ReturnToHub()
    {
        SceneLoader.Instance.LoadHub();
    }

    public override void HandleCancel()
    {
        //
    }
}
