We train a mobile robot to reach goal using relative angle in state representation, s=(theta, d1~dn). The good news is: NO MORE relative distance information (DeltaX, DeltaY). As such, the NN trained is easier to be installed on a real robot like Jetbot since computer vision CNN (e.g., FasterRCNN, KeypointsRCNN) can be employed to estimate the relative angle. In a real robot, relative distance between agent and goal is difficult to estimated using vision unless special sensors are used.
The Unity package (v. 2020.1.17f1), the training configuration file (ML Agent Release 10), and instructions ppt are included. Please see ppt for instructions.
The training video is shown at: https://youtu.be/aNPAP3v0gHc
Demonstration video of the results are shown at https://youtu.be/K3mN6CDPRGc
