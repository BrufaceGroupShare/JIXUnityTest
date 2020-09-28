# JIXUnityTest
 
The APK file is: JIXTest.apk

The project uses Unity 2018.4.22f1, and Google ARCore SDK 1.19.0

The Player settings include custom Gradle template and disabled Auto Graphic API feature.

The submitted scene is "JIXARCoreScene"
The depth is enabled automatically with the session config file.
I used WaterStylize assets from the Unity Asset Store.

The project renders all horizontal and vertical planes while we need only the ground plane to be visible (which has a lowest Y value).
I was also studying the Water demo from "ARCore Realism" but not yet be able to migrate it in a new project without the presence of "ARCore Realism Demos" asset.
During the Github upload, I experienced errors in settings to upload large files (above 100MB) then I omitted three "*.so" files in the Library folder.

Overall, I found ARCore is a cool, advanced, and accurate technology to work with regarding a short timeframe. Still, I need to invest more in-depth understanding to use it properly and extract its full capability.
