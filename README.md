# AffdexMe

**AffdexMe** is a windows application that demonstrates the use of the Affdex SDK for Windows. It uses the camera on your Windows PC to view, process and analyze live video of your face. Start the app and you will see your own face on the screen, and metrics describing your facial expression of emotion.

[![](http://developer.affectiva.com/images/affdexme_win_emoj_3.0_fb.png)](https://www.facebook.com/ahamino/videos/857058591896/?l=7851546746791640786)


[![Build status](https://ci.appveyor.com/api/projects/status/vidys6dkff0c37dl?svg=true)]
(https://ci.appveyor.com/project/ahamino/affdexme-win)

#### Download: [the built version](http://affdex-sdk-dist.s3-website-us-east-1.amazonaws.com/windows/download_affdexme.html)


#### This app includes the following command buttons:

*   Start - Starts the camera processing
*   Reset - Resets the context of the video frames
*   ShowPoints/HidePoints - toggles the display of facial feature points, which Affdex uses to detect expressions
*   Display metrics - Displays the emotion and expressions
*   Display emojis - Displays the most likely / dominant emoji
*   Display Appearance - Displays appearance metrics (gender / eye glasses)
*   Take a screenshot - Takes a screenshot of the camera feed with the metrics overlayed
*   Stop - Stops the camera processing
*   Exit - exits the application

It runs on Windows 7.0, 8.0, 8.1 and 10

#### To build this project from source, you will need:

*   Visual Studio 2013

*   To [download and install the Windows SDK (64-bit)](http://developer.affectiva.com/downloads) from Affectiva.
    
    By default, the Windows SDK is installed to the following location: ```C:\Program Files (x86)\Affectiva\Affdex SDK```
    
    Modify the ```FilePath.GetClassifierDataFolder``` and ```FilePath.GetAffdexLicense``` in ```FilePath.cs```:

    *   **FilePath.GetClassifierDataFolder**

        The function should return the full path of the Affdex SDK data folder.

        If you installed Affdex SDK in a location other than the default. Please change the return value to match the location of the data folder in your installed location.
    *   **FilePath.GetAffdexLicense**

        The function reads the license file and returns its contents. You can signup for an evaluation license to use with the app [by filling this form](http://www.affectiva.com/45-day-free-trial/)

        Set ```string fileName``` variable to the location of the your license file

*   Build the project

*   Run the app through Visual Studio

**Note** It is important not to mix Release and Debug versions of the DLLs. If you run into issues when switching between the two different build types, check to make sure that your system path points to the matching build type.

Emoji Icons provided free by [Emoji One](http://emojione.com/)

Copyright (c) 2016 Affectiva. All rights reserved.
