using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniHumanoid;
using VRM;
using UniGLTF;
using MirenDetector;

public class VRMModelLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject vrmModel;

    [SerializeField]
    private GameObject poseSettingPanel;

    void Start()
    {
    }

    void Update()
    {
    }

    public void OnOpenClickedVRM()
    {
        string filePath = FileDialogForWindows.FileDialog("Select VRM Model", "vrm");
        if (filePath != null) LoadModel(filePath);
    }

    private void LoadModel(string path)
    {
        GameObject _vrmModel = null;
        try
        {
            if (!File.Exists(path)) return;

            Debug.LogFormat("{0}", path);
            var bytes = File.ReadAllBytes(path);
            var context = new VRMImporterContext();

            context.ParseGlb(bytes);

            Debug.LogFormat("{0}", context.GLTF);

            context.Load();
            context.ShowMeshes();
            context.EnableUpdateWhenOffscreen();
            context.ShowMeshes();
            _vrmModel = context.Root;
            Debug.LogFormat("loaded {0}", _vrmModel.name);

            Destroy(vrmModel);
            vrmModel = _vrmModel;
            SetVRMModel(vrmModel);
        }
        catch (Exception e)
        {
            //_errorMessagePanel.SetMessage(MultipleLanguageSupport.VrmLoadErrorMessage + "\nError message: " + e.Message);
        }
    }

    private void SetVRMModel(GameObject vrmModel)
    {
        vrmModel.AddComponent<DetectionResultManager>();
        if(poseSettingPanel != null)
        {
            PoseSettingUI ui = poseSettingPanel.GetComponent<PoseSettingUI>();
            ui.vrmModel = vrmModel;
        }
    }
}
