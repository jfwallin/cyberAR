---


---

<h1 id="tips-and-tricks-for-magic-leap-development-in-unity">Tips and Tricks for Magic Leap Development in Unity</h1>
<p>The goal of this document is to combine some helpful tips and tricks when dealing with Unity and Magic Leap. There are tips for C# scripting, project architecture, scripting API documentation (because Magic Leap leaves many things undocumented), and Unity Setup.</p>
<p>This Document was written using markdown and StackEdit linked to google drive for easy collaboration. Markdown is a great tool for writing good-looking documentation quickly. Here is a <a href="https://www.markdowntutorial.com/">markdown tutorial</a> and <a href="https://www.markdownguide.org/cheat-sheet/">markdown cheat-sheet</a> that covers basically everything you need to know. (Stack Edit also has markdown cheat-sheet built in!)</p>
<p><strong>Always remember Google is your friend. There are tons of tutorials and forums for Unity</strong></p>
<h2 id="general-unity-tips">General Unity Tips</h2>
<ul>
<li>
<p><strong>Tracking Freezing/Crash Problems Using Unity Logs:</strong><br>
If when working on your project in Unity the editor freezes or crashes (which it does more frequently than one would hope), it can be hard to tell what caused it. However, after every session Unity runs It generates log files which can be invaluable in tracking down exactly what script is causing the trouble.<br>
Unity has documentation on the <a href="https://docs.unity3d.com/Manual/LogFiles.html#:~:text=Package%20Manager,-To%20view%20the&amp;text=app%20utility%20to%20find%20the,%5CUnity%5CEditor%5CEditor">log locations</a> for different OS installations.</p>
</li>
<li>
<p><strong>Make Sure The Target Platform is Set to Lumin</strong><br>
If the console is full of errors saying script names don’t exist, it is likely because the target platform is not set to Lumin, and all of the Magic Leap scripts only compile when that target is set.</p>
</li>
<li>
<p><strong>Make Editor Tools To Make Your Life Easier</strong><br>
Adding custom tools to Unity is really easy and is often accomplished by adding a simple attribute to a function. You can use them to call a function manually for testing, like to fake network connections, manually spawn objects for testing, or run tests on the project. Editor Scripting is powerful and helpful! Here is one great <a href="https://learn.unity.com/tutorial/editor-scripting">editor scripting course</a></p>
</li>
<li>
<p><strong>Debugging Using Visual Studio</strong><br>
Visual Studio (VS) comes with plenty of great tools that help debug your code and link directly with unity.</p>
<ul>
<li><strong>BreakPoints:</strong> Added by clicking in the far left column in VS. When reached, Unity will freeze execution, and all variable values may be inspected. Right click a breakpoint to disable it.</li>
<li><strong>Executing Steps:</strong> Press
<ul>
<li>F10 to go one step past the break point</li>
<li>F11 to step into a funcion instead of executing it all at once</li>
<li>F5 to continue running until another breakpoint is reached.</li>
<li>Shift + F5 to stop debugging</li>
</ul>
</li>
<li><strong>Watch Variables:</strong> Right click on a variable to “AddWatch” and keep track of its values.</li>
</ul>
</li>
</ul>
<h2 id="scripting-tricks">Scripting Tricks</h2>
<ul>
<li><strong>Common Unity Functions:</strong>
<ul>
<li>Update(): Called every frame by unity. Put code here that changes things over time.</li>
<li>FixedUpdate(): Called before every update of the physics loop, which does not line up with every frame execution.</li>
<li>LateUpdate(): Called after all the other Update functions are called. Put code here that depends on other update code being completed before running, like pointing something at another object that moved that frame.</li>
<li>Awake(): Called once in the lifetime of a script, even if the script is not enabled. commonly used to set up variables and references to other scripts.</li>
<li>Start() Called once in the lifetime of a script, always after awake. It only executes if the script is enabled, and then only once. If a script is enabled, disabled, then enabled again, Start() is still only called once.</li>
</ul>
</li>
<li><strong>Time in Unity:</strong>
<ul>
<li>Time.deltaTime: The amount of time that has passed since the last frame. If something is moving at a constant speed, then scale its movement by multiplying it with this number, so that its speed appears consistent even if the time between frames changes drastically.</li>
<li>Time.timeScale: How fast time passes. It is set to 1 by default, but can be set to lower decimals for bullet-time like effects, or 0 to freeze time for a menu pause.</li>
</ul>
</li>
<li><strong>Common Attributes:</strong> format [AttributeName]
<ul>
<li>[HideInInspector]: Put this on the line above public variables so that they don’t appear in the Unity inspector like they would normally. Used when other scripts need to access a variable but you don’t want it altered from the editor.</li>
<li>[SerializeField]: Put this on the line above a private variable to make it visible in the inspector. This allows variables not accessible outside the class to be changed easily from the editor.</li>
</ul>
</li>
<li><strong>SmoothDamp:</strong>
<ul>
<li>Often when trying to smoothly change a value over time, Mathf.Lerp is used, which can sometimes over-shoot the target value, or take too long. Mathf.SmoothDamp() is a utility function that smoothly changes a value with none of those problems, and is very flexible. Here is the <a href="https://docs.unity3d.com/ScriptReference/Mathf.SmoothDamp.html">SmoothDamp documentation</a>. Note, there is one for float values (Mathf) and Vectors (Vector3).</li>
</ul>
</li>
<li><strong>Software Architecture and the Single Responsibility Principle (SRP):</strong>
<ul>
<li>Extremely large monobehaviours should be avoided. One class thats over 600 lines long is questionable, and consistently going over 300-400 lines is also not great. A single script/class should have a clearly defined and limited responsibility, and its code should only do that one thing.</li>
<li>Large monobehaviours should be broken up into smaller ones, and as many scripts that can be made into Scriptable objects or vanilla classes should be, since that code will run more quickly.</li>
<li>SRP is vague unfortunately, and some classes just kinda have to be large, but it is important to try your best to keep things small and separate.</li>
</ul>
</li>
</ul>
<h2 id="magic-leap-tips">Magic Leap Tips</h2>
<ul>
<li><strong>Turning on the Magic Leap:</strong>
<ul>
<li>Try to make sure that you are standing up and not right next to your desk. When not done, there is an increased risk the Leap will mis-align the stored world mesh for the room and try to mesh over it. Causing loads of problems.</li>
<li>Make sure to walk around a bit and make sure the area around you is meshed well.</li>
</ul>
</li>
<li><strong>Project Setup:</strong>
<ul>
<li>Make sure that the Magic Leap SDK path is set correctly to the location it was placed by The Lab Package Manager. Copying it to another location can cause problems when trying to link Unity and the simulator.</li>
<li>Go through the <a href="https://developer.magicleap.com/en-us/learn/guides/sdk-play-mode-in-unity-with-ml-remote">process to use the simulator with Unity</a> before trying to run the project in play mode. Have the simulator open and running before you click play as well.</li>
</ul>
</li>
<li><strong>Fixing A Leap with Broken World Mesh:</strong><br>
Sometimes the Leap will Inexplicably tilt the world mesh and start re-meshing over it all, leading to slow mesh updates and ugly mesh. To fix it, try either of the following:
<ul>
<li>If you would rather just try to re-mesh what you can, go to the creator settings in the Magic Leap, and enter the mesh curation tool and manually re-mesh the area.</li>
<li>For a complete mesh reset, you can reset the Leap. Follow the <a href="%5Bhttps://forum.magicleap.com/hc/en-us/community/posts/360019076432-Forgot-Password-PIN%5D(https://forum.magicleap.com/hc/en-us/community/posts/360019076432-Forgot-Password-PIN)">Leap Reset Instructions</a> and make sure that when setting it up again,</li>
</ul>
</li>
</ul>
<h2 id="external-references">External References</h2>
<p>Here some pdf scans of some notes I took on Magic Leap systems that either had little to no documentation, or documentation that was hard to find. Note some things may be out of date now that Magic Leap has updated their SDK so much.<br>
<a href="https://drive.google.com/file/d/1SEU_owuxNq6fmC0KKGGWHmCDk4UyKBWg/view?usp=sharing">ML Spatial Mapper Notes</a><br>
<a href="https://drive.google.com/file/d/1W_M6tLggAxl5e6rQiBiXOVgvSpqidDB-/view?usp=sharing">Persistent Coordinate Frames Notes</a></p>

