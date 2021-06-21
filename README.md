# MirenStudio

![VRM_1](https://user-images.githubusercontent.com/75804798/122736746-4e576600-d2bb-11eb-864d-0dbd09298dd4.gif)
![Live2D_1](https://user-images.githubusercontent.com/75804798/122736753-4f889300-d2bb-11eb-977b-995969b0ea5e.gif)

MirenStudio is open source Vtuber software.

- The software works on Windows 10 with some installation.
- The software is composed of a client part(Unity) and a server part(face detection), and two parts communicate by using Namedpipe.
- Currently, it only works on Unity and Unity editor with VRM model. We are considering applying Live2D, FBX, or other model formats.
- Everyone can use their client with the server part. If you are considering making your client part, please check communication codes.

### Requirements

- [.NET 5.0 Runtime x64](https://dotnet.microsoft.com/download/dotnet/current/runtime)
- [cuda 10.2](https://developer.nvidia.com/cuda-10.2-download-archive) (not 10.1, 11.0)
- [cuDNN 8.0 for cuda 10.2](https://developer.nvidia.com/rdp/cudnn-archive)
- Import [UniVRM package](https://github.com/vrm-c/UniVRM) in unity project.

### Used Networks in Detection
- Face detection: RetinaFace, Single-stage Dense Face Localisation in the Wild (CVPR 2020)
- Facial landmark detection: PFLD, A Practical Facial Landmark Detector 
- Face pose: SolvePnP in OpenCVSharp
- Gaze estimation: [Laser-Eye](https://github.com/1996scarlet/Laser-Eye), [GazeML](https://github.com/swook/GazeML) (with some modification)

We are looking forward to more stable and high accuracy machine learning components. If there is a more suitable ML network, please contribute!

### Demos

https://user-images.githubusercontent.com/75804798/122732816-996f7a00-d2b7-11eb-949a-9bf24bab09b0.mp4
 
https://user-images.githubusercontent.com/75804798/122732835-9bd1d400-d2b7-11eb-9ea1-486788387604.mp4
