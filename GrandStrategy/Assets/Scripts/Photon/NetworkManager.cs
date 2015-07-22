using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
public interface INetworkManager {
    void SendCommand(ViewModel viewModel, ICommand command);
    void AddViewModel(ViewModel viewModel);
} 

// Note its deriving from MonoBehaviour, put this on the same object as your scenemanager
public class NetworkManager : MonoBehaviour, INetworkManager
{

    public SceneManager sceneManager;

    public List<ViewModel> viewModels = new List<ViewModel>();

    public void AddViewModel(ViewModel viewModel){
        viewModels.Add(viewModel);
        Debug.Log(viewModels.Count);
    }

    void OnGUI ()
    {
        GUILayout.Box("VIewModels: " + viewModels.Count);
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.isWriting)
        {
            for (int i = 0; i < viewModels.Count; i++)
            {
                //viewModels[i].WriteToNetworkStream(stream);
            }

        }
        else
        {
            for (int i = 0; i < viewModels.Count; i++)
            {
                //viewModels[i].ReadFromNetworkStream(stream);
            }
        }

    }

    public void AddNetworkCommand()
    {

    }

    public void SendCommand(ViewModel viewModel, ICommand command)
    {
        //etc
        
    }

    [RPC]
    public void ExecuteComsmand(ViewModel viewModel, ICommand command)
    {
        //ExecuteCommand(viewModel, );
    }

}
*/