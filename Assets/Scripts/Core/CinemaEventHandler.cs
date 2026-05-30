using UnityEngine;

public class CinemaEventHandler : MonoBehaviour
{
    //UI Animation들의 Event Marker를 제어하는 스크립트

    public void OnFirstCinemaEnterFinished()
    {
        //HubCutsceneDirector.Instance?.CompleteFirstCinemaEnter();
    }

    public void OnNormalCinemaEnterFinished()
    {
        //HubCutsceneDirector.Instance?.CompleteNormalCinemaEnter();
    }
}
