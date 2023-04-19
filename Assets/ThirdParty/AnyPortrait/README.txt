------------------------------------------------------------
		AnyPortrait (Version 1.4.3)
------------------------------------------------------------


Thank you for using AnyPortrait.
AnyPortrait is an extension that helps you create 2D characters in Unity.
When you create a game, I hope that AnyPortrait will be a great help.

Here are some things to know before using AnyPortrait:


1. How to start

To use AnyPortrait, go to "Window > AnyPortrait > 2D Editor".
The work is done in the unit called Portrait.
You can create a new portrait or open an existing one.
For more information, please refer to the User Guide.



2. User Guide

The User's Guide is "AnyPortrait User Guide.pdf" in the Documentation folder.
This file contains two basic tutorials.

AnyPortrait has more features than that, so we recommend that you refer to the homepage.

Homepage with guides : https://www.rainyrizzle.com/



3. Languages

AnyPortrait supports 10 languages.
(English, Korean, French, German, Spanish, Danish, Japanese, Chinese (Traditional / Simplified), Italian, Polish)

It is recommended to select the appropriate language from the Setting menu of AnyPortrait.

The homepage supports English, Korean and Japanese.



4. Support

If you have any problems or problems with using AnyPortrait, please contact us.
You can also report the editor's typographical errors.
If you have the functionality you need, we will try to implement as much as possible.

You can contact us by using the web page or email us.

Report Page : 
https://www.rainyrizzle.com/anyportrait-report-eng (English)
https://www.rainyrizzle.com/anyportrait-report-kor (Korean)

EMail : contactrainyrizzle@gmail.com


Note: I would appreciate it if you acknowledge that it may take some time 
because there are not many developers on our team.



5. License

The license is written in the file "license.txt".
You can also check in "Setting > About" of AnyPortrait.



6. Target device and platform

AnyPortrait has been developed to support PC, mobile, web, and console.
Much has been optimized to be able to run in real games.
We have also made great efforts to ensure compatibility with graphical problems.


However, for practical reasons we can not actually test in all environments, there may be potential problems.
There may also be performance differences depending on your results.

Since we aim to run on any device in any case, 
please contact us for any issues that may be causing the problem.



7. Update Notes

1.0.1 (March 18, 2018)
- Added Italian and Polish.
- Supports Linear Color Space.
- You can change the texture asset setting in the editor.

1.0.2 (March 27, 2018)
- Fixed an issue where the bake could no longer be done with an error message if the mesh was modified after bake.
- Fixed an issue where the backup file could not be opened.
- Fixed a problem where rendering can not be done if Scale has negative value.
- Improved Modifier Lock function.
- Fixed an issue that the modifier is unlocked and multi-processing does not work properly.
- Added Sorting Layer / Order function. You can set it in the Bake dialog, Inspector.
- Sorting Layer / Order values ​​can be changed by script.
- If the target GameObject is Prefab, it is changed to apply automatically when Bake is done. This applies even if it is not Prefab Root.
- Fixed a bug in the number of error messages that users informed. Thank you.
- Fixed an error when importing a PSD file and a process failure.
- Fixed a problem where the shape of a character is distorted if Bake is continued.

1.0.3 (April 14, 2018)
- Significant improvements in Screen Capture
- Transparent color can be specified as background color (Except GIF animation)
- Added ability to save Sprite Sheet
- Screen capture Dialog is deleted and moved to the right screen to improve
- Support screen capture on Mac OSX
- Improved Physics Effects
- Corrected incorrectly calculated inertia when moving from outside
- Modify the gizmo to be inverted if the scale of the object is negative
- When replacing the texture of the mesh, Script Functions that can be replaced with an image registered in AnyPortrait has been added
- Fixed an issue that caused data errors to occur when undoing after creating or deleting objects
- Fixed a problem that when importing animation pose, data is missing while generating timeline automatically
- Fixed an issue where other vertices were selected when using the FFD tool
- Fixed an issue where vertex positions would be strange when undoing when using FFD tool
- Fixed an issue where the modifier did not recognize that the mesh was deleted, resulting in error code output
- Fixed an issue where the clipping mesh would not render properly if the Important option was turned off
- Fixed an issue where sub-mesh groups could not generate clipping meshes
- Fixed a problem where deleted mesh and mesh groups appeared as GameObjects
- Fixed a problem where the script does not change the texture of the mesh

1.0.4 (June 10, 2018)
- Animation can be controlled by Unity Mecanim.
- Bone IK became more natural.
- IK can be controlled by an external Bone.
- Weight can be set when IK is controlled by external Bone, and this weight can be linked to a Control Parameter.
- Mirror copying is possible when creating bones, and you can paste them in reversed poses when copying poses.
- 2 functions for Bones have been added.
- Added Auto-Key function to automatically generate keyframes when making an animation.
- Onion Skin has been improved to change color, rendering method, rendering order, and to render continuous frames during animation making.
- Ctrl + Alt (Command + Alt in OSX) and mouse drag to move or zoom in and out.
- After the mesh is added, it automatically switches to the Setting tab.
- A button has been added at the top of the screen to change "whether mesh output".
- Two-sided rendering can be set in the mesh setting.
- When the new version of AnyPortrait is updated, the first screen informs you.
- Press the Ctrl key (Command key in OSX) to change the color of the buttons that have customized settings.
- The title image of AnyPortrait has been added to the Demo folder.
- The 7th demo scene with new features in version 1.0.4 has been added.
- Fixed an issue where vertex colors were not rendered properly when setting the Physics Modifier of the clipped mesh.
- Fixed an issue where mesh without rigging information could not be processed properly after bake.
- Fixed an issue where some text was not translated in the Bake dialog.
- Fixed an issue where Depth would bake strangely when creating a nested mesh group when importing PSD files.
- Fixed an issue where meshes were generated strangely when creating Atlas of 4096 resolution in PSD files.
- Fixed an issue where the Morph (Animation) modifier was not processed correctly when running animations.

1.0.5 (June 16, 2018)
- Fixed script errors in apEditorUtil.cs in Mac OSX.

1.0.6 (July 14, 2018)
- Re-importing a PSD File is added
- Change the texture asset settings of Atlas created with PSD file to be high quality
- Added ability to collapse tool group in upper UI
- Setting whether to check the latest version in the editor setting dialog
- Fixed intermittent script error when editing animation
- Fixed an issue where when rigging in scene, rigging weights are not normalized and invalid values ​​are passed
- Fixed an error when checking the latest version

1.0.7 (August 6, 2018)
- Fixed an issue where Bake does not work or error occurs when Rigging Weight value is 0 or Bone is not specified
- Fixed iOS missing in default settings in DLL of AnyPortrait

1.1.0 (October 7, 2018)
- Generating Meshes Automatically is added.
- Mirror tool for editing meshes added.
- Added the ability to edit mesh vertices.
- Perspective camera is supported, and a Billboard option for this function is added.
- "Pirate Game 3D" demo scene, which is the 3D version of "Pirate Game", is added.
- When controlling animations as scripts, "SetAnimationSpeed" functions have been added to set the speed of animation.
- When creating meshes, if you press the Ctrl key (Command key on Mac OSX), the cursor snaps to the nearest vertex.
- When creating a mesh, if you press the Shift key and click a edge, a vertex is added on the edge.
- Make Mesh UI changed.
- You can change the Shadow setting (Receive Shadow, Cast Shadow) in the Bake setting.
- Bake dialog UI changed.
- Inspector UI changed.
- You can open the editor directly from the Inspector, and you can also bake it right away.
- Modifiers and Bones can be added to Child Mesh Groups, and the Parent Mesh Group can control them.
- A menu to open "Q & A Web page" is added.
- Fixed an issue where polygons are not generated properly when making a mesh.
- Fixed an issue where animations set to low speed or low FPS would not play smoothly.
- Fixed an issue where Hierarchy was not updated when deleting animations.
- Fixed an issue where Clipping Mask did not work intermittently when playing game.
- Fixed an issue where the IK setting of the first bone was disabled when creating a sequence in succession.
- Fixed an issue where data was intermittently missing when manually saving backups.
- Fixed an error when controlling the control parameters in the Inspector.
- Fixed an issue where thumbnails are output abnormally in the iOS development environment.
- Fixed an issue where animated clips were continuously created unnecessarily when using Optimized Bake while using Mecanim.

1.1.1 (October 11, 2018)
- Fixed a problem where the positions and angles of child bones changed when linking bones.

1.1.2 (November 11, 2018)
- MP4 video export function is added. (available from Unity 2017.4)
- GIF animation quality option is changed to easily set to four levels.
- Maximum Quality of GIF animation is slightly better than it used to be.
- UI is changed to allow stop during animation capture.
- "Lightweight Render Pipeline" is supported. (available from Unity 2018.2)
- The ability to change the Ambient Light to black for AnyPortrait in the Bake dialog is added.
- Script functions to use apPlayData related to animation playback are added.
- If language is set to Japanese, Japanese website will be opened when selecting a menu
- It is changed that Editing mode is started automatically during a process of adding objects to Modifier.
- Fixed an issue where the clipping mask was not correctly calculated depending on the angle of the camera when rendering from a perspective camera.
- Fixed an issue where Blend function calculates weights strangely when doing rigging.
- Changed the editor to terminate automatically when artificially modifying the resource path of AnyPortrait.
- Fixed a problem where Bone was moved to the mouse position as soon as you clicked the Bone's default position.

1.1.3 (November 15, 2018)
- Added the ability to access the Asset Store directly if there is a new update
- Improved screen capture speed and quality
- No limitation on maximum size of screen capture resolution
- Fixed an issue that  transparency was not applied properly when capturing a screen
- Fixed the problem that the update log dialog does not connect to the Japanese homepage

1.1.4 (December 18, 2018)
- Added "Extra Option" to change rendering order and image in real time.
- Draw calls have been further optimized.
- Added "Refresh Meshes" button to refresh mesh in Inspector UI.
- Three scripting functions have been added to control the material of the mesh.
- The unnecessary object information UI is not displayed at the top of the screen.
- Fixed an issue where the parent mesh group appeared in the dialog to add a mesh group.

1.1.5 (December 24, 2018)
- Fixed an issue where the default depth of mesh does not change on Unity 2018.

1.1.6 (April 19, 2019)
- Unity's Timeline is now available, making it possible to create cinematic scenes
- Option to limit the performance of the editor to prevent laptop overheating is added
- Option to decide whether or not the "Selection Lock" be turned on when "Edit Mode" is turned on
- Drawcall is not increased even when the Scale of the Transform is inverted by a negative value
- When "Important option" is turned off, CPU optimization is improved more effectively
- The ability to change the order of items in the Hierarchy UI is added
- Functions to play animation from a specific point are added
- Functions to change "Sorting Layer/Order" targeting an optTransform are added
- The speed of the animation can be adjusted according to the "Speed Multiplier" property of "Animator"
- The design of Inspector UI is better than before
- A button is added to register the Control Parameter to the modifier without pressing the "Record key button"
- The path where Animation clips are saved in Mecanim setting is changed to "Relative path"
- "Edit Mode" turned on when adding a mesh to the Physics modifier
- If the signs of the scale interpolated by modifiers are different from each other, the value is changed discontinuously to not be through 0
- When editing a modifier, the "Edit mode" is not forcibly turned off even if the value of a Control Parameter is changed
- A button to duplicate an Animation event is added
- A warning message appears when a child Mesh Group is associated with an Animation clip
- You can select all vertices with "Ctrl + A"
- You can copy animation keyframes with "Ctrl + C, V"
- Animation curves can be modified in batch when multiple keyframes are selected
- An animation curve can be copied to keyframes of all timeline layers
- When you bake a character, a dialog appears asking you to change the scene's Ambient Color automatically
- "Length of a bone" is added in bone setting UI
- An issue where the logs are continuously output to the console when using some functions is fixed
- An error which is occurred on Hierarchy UI is fixed
- An issue where Unity stopped when docking the AnyPortrait editor to the Unity editor and turning Maximize on and off is fixed
- An issue where an animation with "Once" type could not play normally from Animator is fixed
- An issue where a portrait could no longer be opened when "Extra Option" targets bones is fixed
- A problem where multiple keys could be created with the same value of Control Parameters is fixed
- An error, which is occurred when a scene is switched or Unity editor is restarted with the AnyPortrait editor open, is fixed
- An issue where Extra Option value is missing when copying and pasting the modifier key is fixed
- An issue where FPS of animations are not apply properly when executing "Optimized Bake" is fixed
- An issue where vertices are moved to the mouse position when the mesh was created by pressing the Ctrl key and selecting the nearest vertex is fixed
- An issue where temporarily hidden meshes were forcibly shown when changing the value of a Control Parameter while "Edit Mode" was turned on is fixed
- A problem that can not import a PSD file with invalid channel information is fixed

1.1.7 (July 10, 2019)
- New features, "Material Library" and "Material Set" are added to manage various materials and shaders and apply various rendering techniques.
- Data of modifiers and animations are optimized to reduce the size of the prefab file
- Improved "AsyncInitialize" function to reduce CPU load is added
- Processing speed for some additions and deletions in animation work is improved
- Coping keyframes to other animation clips with "Ctrl+C,V" is available
- It is available to select and add multiple meshes and mesh groups to a mesh group at once
- User can set whether "Controller tab" is switched automatically when animation or modifier is selected
- User can set whether the "Temporary rendering" of the mesh will be reset for reasons such as Undo or key-value change during the task
- The button to reset the mesh's "Temporary rendering" is added
- An issue where "Do Not Show this message" worked reversely in the Ambient Color correction dialog is fixed
- An issue where existing keyframes could not be overwritten when copying animation keyframes with "Ctrl+C,V" key is fixed
- A problem that clipping settings of layers were not applied when importing PSD file is fixed
- A problem that the shadow and normal vector of 2-sided mesh are calculated abnormally is fixed
- An issue where the animation clip was not opened in the editor when the Start Frame and the End Frame are the same is fixed

1.1.8 (September 8, 2019)
- Rigging Modifier is improved
- The Slider UI to modify weights is added
- "Lock function" to restrict weight edits by other bones is added
- "Brush Mode" to edit weights using the mouse is added
- The function to select vertices rigged by the current bone
- The function to copy the rigging weights of multiple vertices based on position
- "Auto-Rig" is greatly improved
- The rigging weight of vertices is renderable as a shape of a pie-chart
- The option to render clipping meshes in LWRP (Lightweight Render Pipeline) is added
- The LWRP 2D material package is added in Material Library
- You can set whether the color of newly added bone is similar to the color of the parent bone in the Setting dialog
- The function to Snap the end of a bone to a child bone is added
- The function to duplicate a bone is added
- "LookAt" method of IK Controller works even for one bone or bone for which IK chain is not set
- You can hide the left and right UI, and the UI design that can fold the right UI up and down has been improved
- Sorting Group is supported
- An option for setting "Sorting Order" is added, and a related script function("SetSortingOrderChangedAutomatically") is added
- When changing the mesh group that is the target of an animation clip, if the parent mesh group is selected, the animation data is migrated without initializing
- A bug that the Modifier's Extra option and Animation Events would not be duplicated when cloning animations is fixed

1.2.0 (October 28, 2019)
- VR is supported
- Target Texture property of Cameras is supported
- It is changed that Billboard option does not apply in Bake processing, only apply while the game is running
- A bug where animation clips for Mecanim would not be saved properly if there is a white space in the save path is fixed.
- A bug that Custom FFD does not work properly when the size is 2 is fixed
- A bug that the bone is not be controlled right after it is detached is fixed
- A bug that functions which control meshes in a batch such as SetMeshImageAll and SetMeshColorAll do not work properly is fixed (Please execute Bake to apply)

1.2.1 (November 25, 2019)
- The performance of the AnyPortrait Editor has been slightly improved and stabilized
- Auto-Scrolling in the animation timeline UI has been improved so that it also works when adding a keyframe or selecting and editing objects
- Improved keyboard shortcuts in the animation timeline UI to better recognize them
- The process method of FPS in the AnyPortrait Editor has been changed to be easier to see
- A bug that Bake is failed when changing the Depth of a Child Mesh Group with the modifier's Extra option is fixed
- A bug that the rendering order changed by the modifier's Extra option is not applied during screen capture is fixed
- A bug that the Auto-Scrolling movement of animation timeline UI does not work properly is fixed
- A bug that the "Temporary Show/Hide" buttons are displayed incorrectly in the Object List UI is fixed

1.2.2 (January 31, 2020)
- Duplicating a Mesh is added
- Duplicating a Mesh Group is added
- Duplicating objects (Meshes and sub Mesh Groups) in mesh groups is added
- Migrating a Mesh in a Mesh Group to another Mesh Group
- Improved editing of curves separately by "Previous / Middle / Next" when editing curves in batches with multiple keyframes selected
- When you press Ctrl+Shift and Click at the top of the Timeline UI, the Time Slider moves directly to that location
- The feature to change whether or not to limit the rotation value of keyframes within 180 degrees
- Performance of the editor is slightly improved
- When running on Mac OS for the first time, a message related to Metal appears
- The count of bones is added in Statistics
- Pressing the Auto-Rig button of the Rigging modifier while holding down the Ctrl (or Command) key allows you to select the bones that will be the target of automatic rigging
- When opening a dialog to select a texture for an image, it is changed that texture assets are loaded sequentially
- Added material presets to support Universal Rendering Pipeline (URP) added in Unity 2019.3
- The feature is added that pressing the Arrow keys (or Shift+Arrow keys) can change the position, rotation and scale of the selected object
- The feature is added that the FFD tool is applied or canceled by pressing Enter or Escape key
- The function is added to change the Color Space of Images in batches when the value of Color Space is different between the Baking option and the images property
- The internal process is improved to handle "Undo" or "Redo" when adding or deleting objects
- The menu is added to go to the Advanced Manual page (Window > AnyPortrait > Advanced Manual in Unity Editor)
- A bug is fixed that intermittent error log when scrolling Hierarchy
- A bug is fixed that "Quick Bake" and "Open Editor and Select" do not work properly in the Inspector UI
- A bug is fixed that text is intermittently aligned to center in the Update Log dialog
- A bug is fixed that the order of clipping meshes or sub-mesh groups cannot be changed

1.2.3 (April 25, 2020)
- Smooth transition between 2 animation clips is improved
- Significantly improved overall handling related to transitions and layering of animation clips
- An issue is fixed that caused the control parameters are switched too quickly when the animation clips are switched
- An issue is fixed that the negative value of an integer control parameter in animation is not calculated normally
- An issue is fixed that blending is not worked properly when clips are played repeatedly in Unity's Timeline
- When using "Timeline Simulator", which previews Unity's Timeline, it is improved to preview even when the game is not running in the editor.
- It is prevented to scroll horizontally in the Hierarchy UI of the mesh group while editing the modifier
- It is improved that the order in the Hierarchy UI to also apply to the Controller tab
- Top area and margins of the modifier UI is improved to be more intuitive
- A function works normally even if the cursor moves outside the workspace while dragging
- If the options in the setting dialog are different from the default values, the colors are displayed differently
- Warning message appears when restoring settings to default
- Backspace and Delete are not distinguished in Mac OSX, so two keys are recognized as the same shortcut
- "Trash icon" is added to all Detach/Delete buttons
- Improved UI on the right side of the animation clip
- The animation timeline UI can be zoomed by pressing Ctrl Key and scrolling the mouse wheel.
- The problem is fixed that caused unnecessary memory usage and poor performance when the Hierarchy UI was refreshed
- The performance in many processes, such as when starting modifiers, selecting objects in Hierarchy, and adding and deleting meshes in modifiers is improved
- The issue is improved that the processing time of the editor increases when there are many animation clips
- When loading a Portrait for the first time in the editor, a pop-up to see the loading process is displayed (except when opening the editor in the Inspector)
- Processing speed is improved by optimizing unnecessary processing of non-rendered meshes
- In the setting dialog, you can set the size of bones and whether the size increase according to the screen zoom
- In the workspace, the color of the bone's outline is changed to be different from the bone color
- When a bone is selected in the workspace, the outline of the bone is slightly shiny to be easy to distinguish
- "Needle shape", which is the new shape of bones is added
- It is easier to select bones when clicking with the mouse than before
- In the bone setting UI of the mesh group, preset buttons to set the color of the bone easily are added
- If the option of bones is set that new bone's color is similar to the parent, its color is not too much similar to the parent than before
- "Vivid" option is added to show rigging gradation composed of different colors
- The option is added to make the selected area of ​​the vertex's circular rigging weights easy to distinguish
- The option is added to set the size of vertex's circular rigging weights
- If the value of the weight in the circular rigging weights of the vertex is small, it is difficult to see, so it is displayed separately in the center of the circle
- Vertex's circular rigging weight's click area is larger than before
- The feature is added to show translucent or hide the non-rigging bones
- Shortcut keys are added to adjust rigging weight (Z,X: change by 0.02, Shift+Z,X: change by 0.05)
- While holding Ctrl, Shift or Alt key in editing rigging, bones are not selected
- A shortcut key is added for deleting bones in the setting screen (Delete key)
- When hiding or showing objects with the Color option of the modifier, an option to switch immediately without being translucent is added
- An option is added to change AnyPortrait package installation path <color=red>(For more information, please visit our homepage)</color>
- Performance in game is improved when placing and running multiple characters with Fixed FPS by turning off the Important option
- It is improved so that the temporary visibility is maintained even if you do any action, if the option of keeping temporarily visibility of meshes and bones is selected
- Since Unity 2019.3, it becomes difficult to press the button due to the location of the tooltip, so the tooltip does not appear from that version
- The text is changed about non-Bake in the Inspector UI
- A problem is fixed that the mapping to sub-mesh groups and theirs child meshes was released when opening a PSD file
- A bug is fixed that the visibility info (eye icon) of objects in the sub-mesh group are not refreshed properly
- An issue is fixed that GUI Control error log is occurred while editing modifier
- A bug is fixed that the editor does not work properly, when there is no registered Root Unit and the editor is opened directly in the Inspector
- A bug is fixed that undoing caused all data to be corrupted when changing the target mesh group of the animation to the parent mesh group, initializing the data
- A bug is fixed that the Bake is failed due to the data when the sub-mesh group was removed after being rigged from the parent's meshes
- A problem is fixed that selecting a bone, a mesh, or a sub mesh group at the same time through Hierarchy of a mesh group is possible when editing the modifier
- A bug is fixed that the FFD tool is not released when another object is selected while using the FFD tool
- An issue is fixed that GUI error log is occurred when selecting another object during Physic modifier editing
- An issue is fixed that the Top UI was displayed strangely when selecting an object while editing a Physic modifier
- An issue is fixed that it was difficult to distinguish from the parent item because the front margin of the 2nd level child item was strange in Hierarchy UI
- A bug is fixed that the vertical scroll bar of the animation timeline UI is not work normally when using the mouse
- An issue is fixed that "Auto-loop keyframes" are not refreshed when changing the animation length or loop option
- An issue is fixed that "Undo" is not worked when changing the animation length or loop option
- A typo is fixed where the vector properties of the Transform item are output as (X, X) to be output as (X, Y) in the animation's keyframe property UI
- An issue is fixed that the "Bake" is not processed properly when the custom property as the texture type is created in the Material Library and "Texture Per Image" is selected.

1.2.4 (July 20, 2020)
- Multiple Selection is added
- Jiggle Bone is added
- The purple-colored bone's "selected outline color" is changed to be more visible.
- The process of selecting meshes in the work area is optimized
- The name and information of the "selected key and object" in the bottom UI are deleted
- The click range of points of the animation curve is expanded
- A dialog asking if you want to initialize vertices immediately after first selecting the mesh after importing from the PSD file appears (You can set it in the setting dialog)
- "Enter Play Mode Option", a new feature of Unity 2019.3, is supported
- "Forum" menu is added
- A bug is fixed that if you edit an object using the arrow keys on the keyboard and then press the arrow keys again immediately after undoing, it will not undo
- A bug is fixed where animation was not played and edited when undo was repeated more than once during animation editing
- An issue is fixed where GUI error log occurred intermittently when editing animation
- A bug is fixed where "Pose Test" no longer works in the Rigging modifier after undoing
- An issue is fixed where a bone could be registered as a timeline layer using the "All Bones to Layers" button after registering the Morph modifier of the animation as a timeline
- A bug is fixed where bones were not selected intermittently when clicked immediately after turning on animation editing mode
- An issue is fixed where clicking the gizmo while "child mesh group" was selected immediately after turning on the edit mode will deselect it
- A bug is fixed where the current rendering status was not reflected as an icon in the Hierarchy UI immediately after copying/pasting in the modifier
- A bug is fixed where the initial value of the control parameter was set to "0 or a value close to 0" instead of default value when starting the game
- A bug is fixed where all edges disappeared when undoing on the mesh editing
- An issue is fixed that the focus is not reflected in the selected item immediately after the Hierarchy UI is updated
- An issue that tool conversion was not fast with shortcut keys immediately after pressing Ctrl and Shift keys is fixed
- An issue in which object output was initialized when undoing while editing a modifier is fixed
- An issue where the bone disappeared when undoing immediately after the keyframe of the parent bone with IK set is automatically generated by the move tool is fixed

1.2.5 (September 24, 2020)
- "Non-Uniform Scale" option is added
- Depending on the option, the Rigged Meshes are flipped when is rendered according to the reversed scale of bones
- In the legacy prefab system and the new prefab system introduced in Unity 2018.3, it has been improved to be able to run "Apply" to characters created with AnyPortrait
- Added functions to control integration with prefabs in the Inspector UI
- When recovering from an abnormally stored backup file due to an unknown cause, it has been improved to recover at least to the extent that editing is possible.
- The maximum value of the brush size of the Soft Selection range, Blur brush, and Rigging modifier's brush is increased by 3 times
- A button and shortcut key to delete mesh group's meshes, child mesh groups, and bones at once are added
- Some words that were awkward in the Japanese UI are fixed
- An issue is fixed that rigged meshes moved strangely when the child mesh group moved
- An issue is fixed that bones could not be controlled immediately after undoing when using the Rigging modifier's Pose Test function
- An issue is fixed that the length of the "Animation Event Dialog" was short in the editor of Unity 2019.3 or later
- An issue is fixed that editing would not work normally when selecting all vertices by pressing Ctrl+A after selecting some vertices when Soft Selection is on
- An issue is fixed that Jiggle Bones behave abnormally depending on the angle to the camera when placing a character in a 3D world and rendering it in a billboard method
- An issue is fixed that an object was not selected when clicking on the last location where the gizmo has been shown
- An issue is fixed that the gizmo was not activated when a mesh was duplicated or a different mesh was selected while the Edit tab of Make Mesh was turned on
- An issue is fixed that the gizmo did not disappear when selecting a vertex and selecting another mesh immediately while the Edit tab of Make Mesh was turned on

1.2.6 (November 7, 2020)
- When opening a character with the editor, the default value ​​of the control parameters is changed to be automatically corrected if it is out of range.
- When rotating an object in the editor or changing the size of an object, the position value is corrected so that it does not move.
- When doing Bake, a message indicating that there is a "Mesh without image" is added.
- An issue is fixed that meshes with a negative default size could not be edited by the gizmo.
- An issue is fixed that meshes, which is applied Rigging or Transform modifiers, with a negative default size did not work properly.

1.3.0 (April 10, 2021)
- Rigging modifier and animation processing performance improvements during game playing
- A menu that allows users to assign most of the shortcut keys in the editor has been added to the setting dialog box.
- The content of the upper message that appears when you press the shortcut key is changed in more detail, and it is improved so that unnecessary cases are not displayed.
- (Added Shortcut) Page Up, Page Down : Scroll the animation timeline UI up and down
- (Added Shortcut) Ctrl + <,> : Move to the previous or next keyframe
- (Added Shortcut) N : Turn the animation's “Auto-Key” function on or off
- (Added Shortcut) ~ : Switch between the Hierarchy tab and the Controller tab on the left side of the editor
- (Added Shortcut) Enter : Execute the Make Polygon function when making mesh
- (Added Shortcut) 1-6 Number keys : Switch the mesh edit menu
- (Added Shortcut) F2 : The focus moves to the UI that modifies the name of the current object
- (Added Shortcut) I : Toggle Visibility Preset
- (Added Shortcut) 1-5 Number keys : Switch Visibility Preset Rules
- (Added Shortcut) Alt + O : Toggle Rotoscoping
- (Added Shortcut) 9 Number key : Previous Rotoscoping Image File
- (Added Shortcut) 0 Number key : Next Rotoscoping Image File
- (Added Shortcut) D : Switch whether to make multiple modifiers to run in edit mode
- (Added Shortcut) Alt + D : Set the modifier's target not to be selected even if the selection lock is released in edit mode
- (Added Shortcut) Alt + G : Display objects that are not subject to modifiers in edit mode as gray color
- (Added Shortcut) Alt + B : Preview calculation result for bones in edit mode
- (Added Shortcut) Alt + C : Preview calculation result for color in edit mode
- "View menu" has been added to replace the role of the existing view icons
- Onion Skin, mesh, bone display option buttons have been deleted (These can be set to reappear in the Setting dialog)
- Status icons indicating how objects can be displayed and edited in the workspace appear in the upper right corner.
- A new "Visibility Preset" function has been added to the "View Menu" that allows the user to pre-define which bones and meshes to display in the workspace.
- Modifier Lock is removed and is replaced with "Edit Mode Options" (added to View menu)
- The previous automatic mesh generation function has been removed, and a completely new and improved automatic mesh generation function is provided
- Mesh can be created directly from the mesh settings menu
- Modifying the area for mesh generation is improved be easily edited with the mouse.
- When selecting a mesh created from a PSD file, an option to automatically generate a mesh is added to this message when selecting the option that prompts you to remove vertices.
- The "Rotoscoping" feature, which shows external image files in the background of the workspace, is added to the View menu
- The function to paste two or more keys in combination in the modifier by control parameter is added.
- After coping values ​​in up to 4 slots, you can paste them in the form of sum values or average values.
- It is improved so that copied values ​​can be pasted even if they are not the same object as long as the minimum conditions are satisfied (except for copy & paste in animation)
- The script function to get the Sorting Order of target mesh is added
- Physics functions are improved such as AddForce to be applied to "Jiggle Bones"
- The script functions are added that returns the currently playing apOptRootUnit or index
- The size of the top UI of the editor and the first UI on the right has been reduced
- Control parameter UI has been reduced and improved to show more Control Parameter UI on the screen. (Edit shortcut button has been deleted)
- When selecting keyframes by dragging in the animation timeline UI, it has been improved to automatically scroll vertically and horizontally.
- When selecting a mesh from the menu, the screen is automatically scrolled to fit the area or pivot of the mesh.
- "Add all control parameters to the timeline" function is added
- The order of sub-mesh groups in the Bone tab of the Hierarchy UI of the mesh group has been improved to be sorted by the depth
- It is improved so that objects are sorted in the same order as the hierarchy when adding objects to the timeline.
- It is improved so that the timeline layers are sorted in the same order as in the hierarchy when the "button to sort timeline layers by depth" is pressed.
- When using the "Depth To Order" method in the Bake dialog, an option to assign the sorting order at intervals other than 1 has been added.
- If there is only one image, the image is automatically set when creating a mesh
- When opening or saving an external file, the last path is recorded and is improved to be used as the default path the next time the dialog is opened.
- The option to keep the "Auto-Key" function even after the edit mode exits has been added.
- When renaming a mesh group, if it is registered as a child of another mesh group, the name of the connected object is synchronized so that it is changed together (the vice versa is not synchronized)
- An issue is fixed that occurred in the internal functions of apOptTransform, GetMeshTransform, and GetMeshGroupTransform.
- An issue is fixed that the Transparency Sort Mode was forcibly fixed to Orthographic even when the billboard option was turned off.
- An issue is fixed that the interpolation of the animation timeline of the "control parameter" type was not properly processed while the game was running.
- An issue is fixed that a "ghost input" that seemed to keep typing these keys for a while after releasing other shortcut keys after pressing Ctrl, Shift, Alt keys occurs
- An issue is fixed that the control parameter was not selected correctly when clicking another control parameter UI immediately after moving the cursor out of the editor while being selected in certain versions of the Unity editor.
- An issue is fixed that too many registry values ​​were created when the editor options were saved.
- An issue is fixed that the backup file was incorrectly saved differently from the specification depending on the user's device environment. (Backup files from previous versions can be opened in this version, but on the contrary, backup files created in this version cannot be opened in previous versions. )
- An issue is fixed that the preview was displayed abnormally because the "default position, rotation, and size of the mesh group" was not applied in the "Reimport PSD" function.
- An issue is fixed that the Jiggle Bone setting was missing when exporting the bone structure to a file.
- An issue is fixed that some buttons appeared to be clickable in the UI of animation clips with no mesh groups connected
- "Physic" of the modifier name has been modified to "Physics"
- An issue is fixed that layer opacity was not applied when importing a PSD file
- An issue is fixed that Bake could not be executed when the Physics modifier was applied to the double-sided mesh.
- An issue is fixed that when Bake was performed while the size of the GameObject of apPortrait or GameObjects above it was inverted, it would not render normally even if the scale was restored to its original value.
- An issue is fixed that "Bones with no rigging weight applied to them appear gray" is not updated immediately after performing the "Pos-Paste" function.
- An issue is fixed that the dialog was forcibly closed during the process of opening a PSD file due to a Unity Editor error in the latest macOS (however, the error log may continue to occur until updated in macOS or Unity)
- An issue is fixed that editing the modifier of a child mesh group would become uneditable when undoing it
- An issue is fixed that an afterimage of 1 frame remained when the root unit was switched by animation.
- An issue is fixed that when playing animation with a script after calling Hide and Show, it was not normally converted to the more than second root unit 
- An issue is fixed that the processing result was not applied to the control parameter in the first frame when playing animation with a script

1.3.1 (April 19, 2021)
- It is changed so that the failure message does not appear when creating an overlapped keyframe
- An issue is fixed that some data of other animation clips could be initialized if you undo immediately after creating any data during animation work

1.3.2 (July 10, 2021)
- Editor performance has been improved.
- [Accelerated Mode] that additionally improves editor performance by installing a separate plug-in has been added. (Change in setting dialog)
- Overall performance in gameplay has been improved.
- The memory issue when updating modifiers has been improved.
- [Guideline] function to output arbitrary straight line in the workspace has been added to [View menu].
- You can right-click in the Hierarchy to open a menu and use various functions.
- [Search] function has been added to the right-click menu in Hierarchy.
- [Synchronize] script function has been added to synchronize animation or control parameters by connecting to another apPortrait.
- When creating a new portrait in the editor, the text box is automatically focused.
- Functions such as Duplicate and Migrate can be used simultaneously with multiple objects selected in the mesh group setting.
- The edited values can be copied, pasted, or initialized while multiple objects are selected on the edit screen of the modifier linked to the control parameter.
- Multiple layers can be selected using the Ctrl and Shift keys in the PSD file import dialog.
- [RemoveForce] function and overloaded [RemoveTouch] function to disable only target physics force are added.
- An issue has been fixed that the Additive method does not work properly when changing the scale of an object in the modifier using two or more control parameters.
- An issue has been fixed that that objects in a child mesh group do not appear in the Hierarchy UI when undoing immediately after copying or migrating objects.
- An issue has been fixed that the edit state properly is not recovering intermittently if undo and redo are repeated.
- An issue has been fixed the animation of objects with the Important option turned off behaved strangely when changing Unity's Time.timeScale.
- An issue has been fixed undo is not possible after migration of objects in a mesh group.
- An issue has been fixed undo is not working after modifying the depth of objects in a mesh group.
- An issue has been fixed that escape characters are added to the path setting value, so the problem that the path is not recognized depending on the system environment.
- An issue has been fixed that saving and opening files is not work normally caused by encoding not being UTF-8 depending on the system environment.
- An issue has been fixed that the rotation gizmo behaves in the opposite way when the scale of the bone is inverted
- An issue has been fixed that the physics modifier behaved as if it was running at low FPS depending on the execution environment

1.3.3 (July 20, 2021)
- Editor timer and FPS counter improved
- Added "Color Only" modifier
- An issue has been fixed that the editor slowing down due to too much undo data being generated
- An issue has been fixed that the Hierarchy UI did not update properly when selecting and editing a mesh group belonging to another mesh group
- An issue has been fixed that the error occurred during executing Bake when assigning rigging weights using the "Pos-Paste" function to bones belonging to other mesh groups
- An issue has been fixed that bones and meshes were not selected properly in the workspace

1.3.4 (October 26, 2021)
- Mesh and bone sockets can be checked in the Inspector UI
- Animation events can be checked in the Inspector UI and copied to the clipboard
- Similar to UnityEvent, it can be changed to call an animation event by way of a callback call.
- Added icon to GameObject created with AnyPortrait in Hierarchy UI (settings can be changed)
- Added "Rotation Lock" option used when editing
- Added Rotational Interpolation option of "Vector Method"
- Added a script function to synchronize the movement of bones of target characters
- Added a script function to synchronize the Root Unit index of target characters whose animations are not synchronized.
- You can select multiple objects and set Extra Option at once
- Improved to be able to see Depth changes that exceed the max/min range in the UI
- When changing the depth of multiple meshes, you can check how they are changed by selecting each in the depth change UI.
- The UI for changing image size is improved.
- A menu to delete multiple objects at once in the Hierarchy right-click menu is added.
- The ability to automatically generate multiple meshes in batches is added.
- When adding a guideline, regardless of the option, the guideline is displayed immediately in the workspace.
- It is improved to show the bones in the workspace by releasing the "Hide Bone option" when clicking the Bone tab of the mesh group.
- "Invert background-color" function is added in the View menu.
- It is changed to show internal polygons even when the setting tab is selected on the mesh editing screen.
- The feature to copy vertices in the "Make Mesh > Edit" tab of the mesh edit screen (can also be copied to other meshes) is added.
- A script function to change the animation update speed during the game is added.
- Start dialog is changed.
- "Video Tutorials" item is added to menu.
- Processing of clicking a vertex in the Add tool of Make Mesh is improved
- An issue has been fixed that an inconsistent result when changing the Depth setting in Extra Option for multiple meshes.
- An issue has been fixed that multiple requests with Depth setting in Extra Option out of the range of the entire mesh were not processed properly.
- An issue has been fixed that it could not be opened at once due to an error when opening a Prefab.
- An issue has been fixed that the editor cannot be opened at once has been fixed when the installation location has been changed.
- An issue has been fixed that the "Create New PSD Set" function did not work on the "Reimport PSD File" screen.
- An issue has been fixed that the automatic mesh generation function does not work properly when the size of the actual image file and the size of the image imported as an asset are different.
- An issue has been fixed that the Extra Option did not work in the Color modifier.

1.3.5 (January 12, 2022)
- Support URP of Unity 2021/2022
- An issue is fixed that clipping meshes were not rendered properly in URP (Unity 2021/2022.a)
- "Merging Materials" function added.
- When executing Bake, if the value of "Render Pipeline" among the settings of Bake does not match the settings of the project, a message asking whether to change them automatically appears.
- A script function has been added that allows you to flexibly control the character's update time by receiving it as a callback function.
- When editing animation, the color of the controller on the left UI is changed to red so that it can be distinguished whether the value of the keyframe of the control parameter is being edited. Otherwise, it will not be applied to the keyframe.
- When creating a material set from the material library, the created material set is selected immediately.
- It is improved that it is easier to adjust the brush size of Soft Selection, Blur, and Rigging Brush.
- When using Bake, Optimized Bake, Quick Bake, and Refresh Meshes functions, only the first root unit is displayed.
- It is improved that the draw call does not increase when using a script function that changes the material properties of meshes in batches, such as "SetMeshColorAll".
- An issue is fixed that changing a Shader asset in the material library could not be changed again after changing it.
- An issue is fixed that the properties are shown like that it is not changed has been fixed when undoing in the material library.
- An issue is fixed that a script error occurs when a class with the name "Action" exists in the project.
- An issue is fixed that the color could not be changed after changing the image of the mesh using the script function of apPortrait.
- An issue is fixed that the initial value is set to 0 when adding keyframes for animations linked to control parameters has been fixed.
- An issue is fixed that the layout of the dialog to select a texture asset was abnormal.
- An issue is fixed that the function that automatically restores to default values if the animation played next does not have a timeline layer that existed in the previous animation when switching animations.
- An issue is fixed that the blending of control parameters was awkward when switching animations.
- An issue is fixed that the first frame or the last frame was not expressed properly when non-Loop animations were played by Mecanim and Timeline.

1.4.0 (July 16, 2022)
- A new curve-based "Pin" tool is added as an element of the mesh to improve working efficiency in the Morph modifier.
- Various tools for editing Pins are added.
- UI to distinguish "Vertex edit mode" and "Pin edit mode" in Morph modifier are added.
- The vertex image rendered in the workspace is changed nicely while improving the rendering performance.
- Images of control parameter UI are changed while improving rendering performance.
- Many icons such as Bake Dialog, Inspector, and Hierarchy are changed to be pretty
- "Pixel Perfect" technique is applied to clearly display the images in the animation timeline.
- An option is added to the Bake dialog to determine whether the "default" or "last value" will be applied to undefined control parameters in the next animation to be transitioned.
- An option is added to the Bake dialog to prevent physics function (Jiggle bones and Physics material) from behaving strangely when the character moves far in just 1 frame.
- An option is added to the Setting dialog to extend the UI height of Vector type control parameters in the editor
- An option is added to the Settings dialog to determine if "Visibility Preset" will remain on when other objects are selected
- Performance of Inspector UI is improved.
- The tab layout is added to Inspector to make it easier to find the properties you need.
- The icons and layout of the Inspector UI are changed to be pretty.
- Character animations are able to be previewed in the Unity editor.
- The Image list is added.
- The usability of the Animation Event dialog is improved.
- A function for users to save events as presets is added.
- An option is added to change the color of event markers
- It is changed that event markers are to be rendered in front of sliders on the timeline.
- Continuous Event markers are expressed more intuitively.
- The feature to create "Secondary Atlas", which is a secondary image with the same Atlas structure is added to "PSD Reimport dialog".
- The stability of functions related to set the path when saving PSD files is improved.
- Editor execution performance is improved.
- Improved way of clicking vertices introduced in previous versions is adapted for all cases.
- When Bake is executed, the target GameObject is focused.
- "Auto-scroll" function of the animation timeline is improved.
- A warning appears when disabling the "Auto Normalize" function in the Rigging modifier.
- The performance of all script functions that reference textures, objects, and animations is improved.
- A script is added to assist in creating a "Command Buffer" to control character rendering.
- The registered keys are displayed in the UI of Control Parameters that are not being edited.
- When the registered Control Parameters in the modifier UI are output, they are changed to be displayed with the preset icon designated by the user.
- It is improved that the UI of the Rigging modifier can intuitively distinguish the currently unavailable UI elements depending on the situation.
- It is changed that buttons that are expected to be clicked by users are flash.
- A button that deletes all Control Parameter keys from the modifier at once and removes the Control Parameter from the modifier at the same time is added.
- In the latest version of Mac OS, "Metal" is changed to a required option so that the guide dialog about Metal is no longer shown.
- The readability of the dialog for selecting the texture of the image and the dialog for selecting the image for the mesh is improved.
- The process of clicking and selecting keyframes etc in the animation timeline is improved.
- An option to adjust the weight of the Jiggle Bone with a Control Parameter is added.
- When selecting one or several bones, the layout order is optimized to minimize changes in the bone property UI.
- The size of the button to remove all vertices in the mesh editing UI is changed to be slightly larger to match the frequency of use.
- When using the "Reset Value" and "Paste" functions of the modifier controlled by the control parameter, the new dialog for selecting the target property to be adapted appears.
- It is changed that all vertices are selected immediately after registering the mesh to the Rigging modifier.
- The ability to refresh all meshes of apPortrait in the scene is added as a "Refresh All Meshes" menu in the Unity Editor menu.
- A new "Modify" right-click menu is added that allows you to directly modify the mesh or mesh group in the mesh group's hierarchy.
- An issue is fixed that the error occurred when using the "Blur" tool with no mesh selected when working with the Morph modifier.
- An issue is fixed that some icon images were displayed at low resolution in the Pro UI.
- An issue is fixed that when editing mode with Transform modifier is turned on while two or more Control Parameters handle the same object, the object disappears.
- An issue is fixed that caused other modifiers to be overly restricted when selecting the Color Only modifier and turning on multi-edit mode.
- An issue is fixed that modifiers that should be deactivated in this edit mode work.
- An issue is fixed that the edit mode is canceled when the multi-edit mode is turned on while the value of the Control Parameter is not located in the key.
- An issue is fixed that the multi-edit mode was not applied properly to modifiers in other timelines.
- An issue is fixed that editor resources were not loaded intermittently when opening the editor through the Inspector.
- An issue is fixed that animations targeting different root units would stop working when trying to play animations in the same frame using a script.
- An issue is fixed that the asynchronous initialization function did not work properly.
- An issue is fixed that the animation playback is too fast to match the frame settings after a long time elapses while the editor is not in focus.
- An issue is fixed that it is recognized as an error and a log is outputted if Bake is executed without specifying an appropriate script function and if the animation event execution method is "Callback".
- An issue is fixed that the Control Parameter UI was displayed strangely when opening the "View Menu" in Unity 2019 and later versions.
- An issue is fixed that you could not select a Control Parameter by clicking it immediately after changing a category of Control Parameters.
- An issue is fixed that the changed shader asset of a material set is not applied to the preset in the material library.
- An issue is fixed that the gizmo of "Pose Test" mode of Rigging modifier could not be controlled until the bone was clicked again after clicking outside the gizmo.
- An issue is fixed that an internal ID issuance error when creating an object before or after undoing will occur with a very low probability.
- An issue is fixed that whether an object is selected immediately when creating or duplicating an object was not consistent depending on the type of object.
- An issue is fixed that the workspace will be zoomed in/out when scrolling with the mouse wheel outside the workspace and moving the cursor back to the workspace.
- An issue is fixed that the shaders of the "Keep Alpha" material preset were rendered strangely when the rendering target was "Render Texture". (You will need to install that package again.)
- An issue is fixed that the previous Atlas was continuously displayed on the screen even after pressing the Bake button of Atlas again in the PSD file import dialog.
- An issue is fixed that an error occurred because some code using the "Type" class was not written explicitly.

1.4.1 (August 3, 2022)
- A function to validate and automatically correct animation event names is added.
- It is changed that "overloading" methods will be excluded that cause an error when assigning callback-style animation events in the Inspector UI.
- The ability to batch assign event listeners in the Callback animation event UI of the Inspector UI is added.
- A function to check the validity of assigned events in the Callback type animation event UI of the Inspector UI is added.
- A function to copy the control parameter name to the clipboard in Inspector UI is added.
- The Korean notation of the button to set whether to automatically play the animation while the root unit is selected is corrected.
- It is changed that Pins created by clicking the workspace with “Add Tool” cannot be moved until clicked again.
- When undoing immediately after adding or deleting a control parameter to a modifier, the control parameter is now properly selected.
- The ability to change options in a batch in the dialog that automatically creates multiple meshes is added.
- An issue is fixed that the marker color property is not duplicated when duplicating an animation event.
- An issue is fixed that the "Remove Animation event button" was missed in the previous version.
- An issue is fixed that an error occurred when undoing after creating a curve connecting pins or deleting a curve.
- An issue is fixed that 2 pins could be connected with 2 curves.
- An issue is fixed that the buttons related to the Prefab were too small in the Inspector UI.
- An issue is fixed that the rendering order was calculated abnormally when the rendering order of objects in a mesh group was changed and the setting tab was not selected and undone.
- An issue is fixed that the remove button ("X") used in Control Parameter UI, Rigging UI, etc. is too dark in Pro UI.

1.4.2 (January 19, 2023)
- Some Bake options are shared on a project-by-project.
- The feature that export/import editor environment settings to a file is added.
- The feature to save user-custom Bake options as "default settings" is added.
- Editor preferences and Bake settings are stored in text file format, compatible with version control tools.
- The editor option to show bones as "Ghost bones" when creating bones by mouse click is added.
- The editor option to change the size of vertices or pins shown in the workspace is added.
- The editor option that allows you to turn off the gizmo behavior of "move the target immediately by dragging right after clicking" is added.
- When re-importing a PSD file, the names of the layers and meshes in the PSD are displayed on the screen
- When re-importing a PSD file, a function to show the image's pixels is added to accurately compare the positions of the PSD layer and the mesh
- When a PSD file is re-imported, it is improved so that the rendering order of the meshes is properly specified based on the PSD file
- When generating meshes from PSD files, it is changed that the record for undo is not divided per processing steps
- When importing a PSD file, the default visibility of meshes is specified based on the visibility of PSD layers
- When importing a PSD file, an error message appears when the path is invalid
- Another object can be selected by pressing the up/down key after clicking the target in the Hierarchy UI
- The Hierarchy UI on the right automatically scrolls when an object is selected in the workspace
- When selecting an animation clip, the Hierarchy UI on the left automatically scrolls
- If you right-click to display the menu of the Hierarchy UI, the name of the clicked object appears, and if multiple objects are selected at the same time, the number of additionally selected objects is displayed as well.
- If you press the Enter key in the "Rename" dialog through the right-click menu, the changed name is applied immediately.
- When trying to change the order of objects through the right-click menu of the Hierarchy UI on the left, a message asking whether to change the sorting mode to "Custom Method" can be shown.
- By some right-click menu functions of the Hierarchy UI, the object will be selected.
- Changing the rendering of a bone with holding the Alt key, the visibility of the child bones will also change.
- If the Color Option is off, a small dot will appear instead, and clicking on it will activate the Color Option immediately.
- Changing the rendering order (Depth) of selected objects at once is supported
- Duplicating selected objects at once through the right-click menu of the Hierarchy UI is supported
- When deleting through the right-click menu of the Hierarchy UI, an option for multiple deletions is provided
- When deleting multiple objects from a mesh group, it has been improved to enable removing even when child mesh groups and meshes are selected together
- It is changed that running "Reset vertices" creates a square mesh to fit the Atlas region 
- The feature that copy and paste selected pins to another mesh is added
- The feature to convert their position values into relative coordinates based on the pivot when copying and pasting vertices or pins is added
- It is changed that pins appear as translucent when the "Make Mesh" tab is selected
- It is improved that the submenu changes in order when you press the number 2 key or number 7 key consecutively among the hotkeys for switching mesh tabs
- The slider at the bottom has been changed to work in the opposite way so that you can intuitively adjust the zoom in/out of the animation timeline UI.
- It is improved that you can move the time slider continuously when clicking and dragging the top of the timeline UI while holding down the Ctrl + Shift keys
- It is improved that the “last clicked timeline layer” for multi-selecting layers is refreshed even when the target is clicked in the workspace or Hierarchy UI
- Added script functions to get playback time, etc. from AnimPlayData
- The speed and stability of searching for the latest version are improved by changing the server that informs the latest version
- Usability is improved by distinguishing the scroll value of the right UI according to the type of selected object
- If you press the Enter key in the dialog to set the name of the new portrait, the portrait is created immediately with the set name.
- If the screen resolution is greater than FHD, the range for clicking vertices or edges is slightly expanded.
- It is improved that the message asking if you want to complete the FFD tool appears on the more a variety of situations.
- Since Unity 2022.2, some APIs related to prefabs have been deprecated, so related codes are changed
- The highlight design of "Selected Items" in the list UI to look is a little prettier
- An issue is fixed where the UI for entering numbers to change the weight of a single vertex did not work in the Rigging modifier edit screen
- An issue is fixed where the rendering status of bones does not return to its original state when a different mesh group is selected immediately after the rendering status of bones has been changed to "gray, translucent" in the editing screen of the Rigging modifier
- An issue is fixed where multiple objects are not removed at the same time by pressing the "Trash can" button in the UI on the right of the mesh group.
- An issue is fixed where the rendering order of objects could not be changed normally when there was a child mesh group
- An issue is fixed where objects are sorted in reverse order when there is a child mesh group of 3 levels or higher when importing a PSD file.
- An issue is fixed where meshes of child mesh groups of 3 levels or higher were not additionally created when re-importing a PSD file
- An issue is fixed where disconnecting the existing parent bone and baking again causes the bone to move strangely
- An issue is fixed where the editor could not be run when it was incorrectly recognized that data existed even after the modifier was deleted due to an error
- An issue is fixed where the character could not be opened in the editor if an error occurred in the animation clip that was linked to the mesh group after the mesh group was deleted or after a similar operation.
- An issue is fixed where animation events were not called in the last frame where auto loop keyframes were located in loop animation.
- An issue is fixed where objects in a mesh group are abnormally sorted when undoing after deleting multiple objects at the same time.
- An issue is fixed where the error log occurs when removing meshes in a child mesh group with the Shift key.
- An issue is fixed where the mapping target mesh was rendered in black in the Re-import PSD dialog
- An issue is fixed where layer information and preview did not work properly when first entering the position correction step in the secondary atlas import dialog.
- An issue is fixed where objects could not be duplicated through the right-click menu of the Hierarchy UI on the left side of the editor while no mesh or mesh group was selected.
- An issue is fixed where some items in the UI on the right were displayed strangely when a child mesh group was selected while the setting tab of the mesh group was selected.
- An issue is fixed where the initial value of the physics effect was incorrectly applied and vertices or bones moved strangely when the game was run 
- An issue is fixed where the physics effect works exaggeratedly when the FPS was too low or the application stopped when the game was running.
- An issue is fixed where it was difficult to click the horizontal edges and vertical edges
- An issue is fixed where meshes with the Rigging modifier applied in the child mesh group - could not be targeted for the Transform modifier - could be bypassed and registered through the "yellow eye button" in the Hierarchy UI.
- An issue is fixed where the Jiggle Bone's Constraint setting would look strange in the workspace when a target is not a root bone and the parent mesh group is rotated
- An issue is fixed where vertices rather than control points were selected when pressing the "Ctrl+A" hotkey while the FFD tool was on
- An issue is fixed where non-target child mesh groups were registered or meshes in child mesh groups that should be targeted were not registered when trying to register all meshes in "Morph (Animation)" as timeline layers
- An issue is fixed where the button to remove objects from the timeline did not appear when an object that was not the target of the timeline was registered as a timeline layer due to a bug, etc.
- An issue is fixed where object selection was canceled when dragging the mouse on the animation editing screen

1.4.3 (March 7, 2023)
- Property ID of "Int" type can be entered as an argument of script functions such as SetMeshCustomImage that controls custom material properties.
- An issue is fixed where the gizmo behaves abnormally when a keyframe is selected by moving the time slider after selecting an object in a frame without a keyframe in the animation editing screen.
- An issue is fixed where the keyframe rotation angle correction option was not applied in "Optimized Bake".
- An issue is fixed where objects rotated abnormally in sections where the rotation angle correction option was not set when keyframes with the rotation angle correction option were repeatedly played in Unity scenes.
- An issue is fixed where an error occurred when previewing an animation in the Inspector UI after adding an animation and Bake again.
- An issue is fixed where information about animation curves in the timeline UI was not updated normally when two or more common keyframes were moved to overwrite other keyframes.


------------------------------------------------------------
			한국어 설명 (버전 1.4.3)
------------------------------------------------------------

AnyPortrait를 사용해주셔서 감사를 드립니다.
AnyPortrait는 2D 캐릭터를 유니티에서 직접 만들 수 있도록 개발된 확장 에디터입니다.
여러분이 게임을 만들 때, AnyPortrait가 많은 도움이 되기를 기대합니다.

아래는 AnyPortrait를 사용하기에 앞서서 알아두시면 좋을 내용입니다.


1. 시작하기

AnyPortrait를 실행하려면 "Window > AnyPortrait > 2D Editor"메뉴를 실행하시면 됩니다.
AnyPortrait는 Portrait라는 단위로 작업을 합니다.
새로운 Portrait를 만들거나 기존의 것을 여시면 됩니다.
더 많은 정보는 "사용 설명서"를 참고하시면 되겠습니다.



2. 사용 설명서

사용 설명서는 Documentation 폴더의 "AnyPortrait User Guide.pdf" 파일입니다.
이 문서에는 2개의 튜토리얼이 작성되어 있습니다.

AnyPortrait의 많은 기능을 사용하시려면 홈페이지를 참고하시길 권장합니다.

홈페이지 : https://www.rainyrizzle.com/



3. 언어

AnyPortrait는 10개의 언어를 지원합니다.
(영어, 한국어, 프랑스어, 독일어, 스페인어, 덴마크어, 일본어, 중국어(번체/간체), 이탈리아어, 폴란드어)

AnyPortrait의 설정 메뉴에서 언어를 선택할 수 있습니다.

홈페이지는 한국어와 영어, 일본어를 지원합니다.



4. 고객 지원

AnyPortrait를 사용하시면서 겪은 문제점이나 개선할 점이 있다면, 저희에게 문의를 주시길 바랍니다.
에디터의 오탈자를 문의 주셔도 좋습니다.
추가적으로 구현되면 좋은 기능을 알려주신다면, 가능한 범위 내에서 구현을 하도록 노력하겠습니다.

문의는 홈페이지나 이메일로 주시면 됩니다.


문의 페이지 : 
https://www.rainyrizzle.com/anyportrait-report-eng (영어)
https://www.rainyrizzle.com/anyportrait-report-kor (한국어)

이메일 : contactrainyrizzle@gmail.com


참고: 저희 팀의 개발자가 많지 않아 처리에 시간이 걸릴 수 있으므로 양해부탁드립니다.



5. 저작권

AnyPortrait에 관련된 저작권은 "license.txt" 파일에 작성이 되어있습니다.
AnyPortrait의 "설정 > About"에서도 확인할 수 있습니다.



6. 대상 기기와 플랫폼

AnyPortrait는 PC, 모바일, 웹, 콘솔에서 구동되도록 개발되었습니다.
실제 게임에서 사용되도록 최적화 하였습니다.
그래픽적인 문제에 대한 높은 호환성을 가지도록 노력하였습니다.

그렇지만, 현실적인 이유로 모든 환경에서 테스트를 할 수 없었기에, 잠재적인 문제점이 있을 수 있습니다.
경우에 따라 사용자의 작업 결과물에 따라서 성능에 차이가 있을 수도 있습니다.

저희는 모든 기기에서 어떠한 경우라도 정상적으로 동작하는 것을 목표로 삼고 있기 때문에,
실행 과정에서 겪는 모든 이슈에 대해 연락을 주신다면 매우 감사하겠습니다.



7. 업데이트 노트

1.0.1 (2018년 3월 18일)
- 이탈리아어, 폴란드어를 추가하였습니다.
- Linear Color Space를 지원합니다.
- 에디터에서 텍스쳐 에셋 설정을 변경할 수 있습니다.

1.0.2 (2018년 3월 27일)
- Bake를 한 이후에 다시 메시를 수정한 경우, 에러 메시지와 함께 더이상 Bake를 할 수 없는 문제가 수정되었습니다.
- 백업 파일을 열 수 없는 문제를 수정하였습니다.
- Scale이 음수 값을 가지는 경우 렌더링이 안되는 문제를 수정하였습니다.
- 모디파이어 잠금(Modifier Lock) 기능을 개선하였습니다.
- 모디파이어 잠금을 해제하고 다중 처리시 제대로 결과가 나오지 않은 점을 수정하였습니다.
- Sorting Layer/Order 기능을 추가하였습니다. Bake 다이얼로그, Inspector에서 설정할 수 있습니다.
- Sorting Layer/Order 값을 스크립트를 이용하여 변경할 수 있습니다.
- 대상이 되는 GameObject가 Prefab인 경우, Bake를 하면 자동으로 Apply를 하도록 변경되었습니다. Prefab Root가 아니어도 적용됩니다.
- 사용자 분들이 알려주신 다수의 에러 메시지들에 대한 버그를 수정하였습니다. 감사합니다.
- PSD 파일을 가져올 때 발생하는 에러와 처리 실패 문제를 수정하였습니다.
- Bake를 계속할 경우 캐릭터의 형태가 왜곡되는 문제를 수정하였습니다.

1.0.3 (2018년 4월 14일)
- 화면 캡쳐 기능이 개선되었습니다.
- 투명색으로 배경으로 화면을 캡쳐하여 이미지로 저장할 수 있습니다. (GIF 제외)
- 스프라이트 시트(Sprite Sheet)로 저장할 수 있습니다.
- 화면 캡쳐 UI가 변경되었습니다.
- Mac OSX에서 화면 캡쳐 기능을 지원합니다.
- 물리 모디파이어가 수정되었습니다.
- 외부에서 위치를 수정할 경우 관성이 잘못 적용되는 문제가 수정되었습니다.
- 객체의 스케일이 음수인 경우 기즈모가 반전되어 나타나도록 수정했습니다.
- 메시의 텍스쳐를 교체할 때, AnyPortrait에 등록된 이미지를 사용할 수 있는 스크립트 함수가 추가되었습니다.
- 객체를 생성하거나 삭제한 이후 "실행 취소"를 할때 발생하는 오류를 수정하였습니다.
- 애니메이션 포즈를 Import하면서 자동으로 타임라인이 생성될 때 데이터가 누락되지 않도록 하였습니다.
- FFD 툴을 사용할 때 다른 버텍스가 선택되지 않도록 수정하였습니다.
- FFD 툴을 사용하고 "실행 취소"를 하면 버텍스의 위치가 이상해지는 문제가 수정되었습니다.
- 모디파이어가 삭제된 메시를 잘못 인식하여 발생시키는 에러를 수정하였습니다.
- Important 옵션이 꺼지면 클리핑 메시가 제대로 렌더링하지 못하는 문제가 수정되었습니다.
- 하위 메시 그룹에서 클리핑 메시를 생성할 수 없는 문제가 수정되었습니다.
- 삭제한 메시나 메시 그룹이 GameObject로 등장하는 문제가 수정되었습니다.
- 스크립트의 함수로 메시의 텍스쳐를 변경할때, 제대로 반영되지 않는 문제가 수정되었습니다.

1.0.4 (2018년 6월 10일)
- 유니티 메카님으로 애니메이션을 제어할 수 있습니다.
- 본 IK의 처리가 자연스러워졌습니다.
- 외부의 본에 의해서 IK가 제어될 수 있습니다.
- 외부의 본에 의해서 IK가 제어될 때 가중치를 설정할 수 있으며, 이 가중치를 컨트롤 파라미터에 연동할 수도 있습니다.
- 본 생성 시 미러 복사가 가능하며, 포즈를 복사할 때 반전된 포즈로 붙여넣을 수 있습니다.
- 본 제어 함수 2종이 추가되었습니다.
- 애니메이션 제작시 자동으로 키프레임을 생성하는 Auto-Key 기능이 추가되었습니다. 
- Onion Skin이 개선되어 색상, 렌더링 방식, 렌더링 순서를 변경할 수 있으며, 애니메이션 작업시 연속된 프레임을 출력할 수 있습니다.
- Ctrl+Alt 키(OSX에서는 Command+Alt키)와 마우스 드래그로 화면을 이동하거나 확대/축소를 할 수 있습니다.
- 메시가 추가된 직후에는 Setting 탭으로 자동으로 전환됩니다.
- 화면 상단에 "메시 출력 여부"를 변경하는 버튼이 추가되었습니다.
- 메시 설정에서 양면 렌더링(2-Sides)을 설정할 수 있습니다.
- 새로운 버전의 AnyPortrait가 업데이트된 경우 첫 화면에서 알려줍니다.
- Ctrl키 (OSX에서는 Command키)를 누르면 사용자 설정이 가능한 버튼들의 색상이 바뀝니다.
- AnyPortrait의 타이틀 이미지가 Demo 폴더에 추가되었습니다.
- 1.0.4 버전의 새로운 기능들이 포함된 7번째 데모 씬이 추가되었습니다.
- 클리핑 마스크가 적용된 메시의 물리 모디파이어 설정시 버텍스 색상이 제대로 출력되지 않는 문제가 수정되었습니다.
- Rigging 정보가 없는 메시가 Bake 후에 제대로 처리가 안되는 문제가 수정되었습니다.
- Bake 다이얼로그에서 일부 텍스트가 번역이 안된 문제가 수정되었습니다.
- PSD 파일을 가져올 때 중첩된 메시 그룹을 생성한 경우 Depth가 이상하게 Bake되는 문제가 수정되었습니다.
- PSD 파일에서 4096 해상도의 아틀라스를 생성할 때 메시들이 이상하게 생성되는 문제가 수정되었습니다.
- 애니메이션을 실행할 때, Morph (Animation) 모디파이어가 제대로 처리되지 않는 문제가 수정되었습니다.

1.0.5 (2018년 6월 16일)
- Mac OSX에서 발생하는 apEditorUtil.cs의 스크립트 에러를 수정하였습니다.

1.0.6 (2018년 7월 14일)
- PSD 다시 가져오기 기능이 추가되었습니다.
- PSD 가져오기 기능시 생성되는 텍스쳐 에셋이 고화질이 되도록 설정됩니다.
- 상단 UI의 도구 그룹을 접을 수 있도록 변경되었습니다.
- 에디터 설정 다이얼로그에서 최신 버전을 확인하는 기능을 켜거나 끌 수 있습니다.
- 애니메이션 편집시 발생하는 스크립트 에러를 수정하였습니다.
- Rigging 값이 Bake 후 잘못 적용되는 문제를 수정하였습니다.
- 최신 버전을 확인할 때 발생하는 에러를 수정하였습니다.

1.0.7 (2018년 8월 6일)
- Rigging 가중치 값이 0이거나 Bone이 할당 안된경우, Bake가 실패하거나 에러가 발생되는 문제가 수정되었습니다.
- AnyPortrait의 DLL의 기본 설정값에서 iOS가 누락된 문제가 수정되었습니다.

1.1.0 (2018년 10월 7일)
- 메시 자동 생성 기능이 추가되었습니다.
- 메시 미러 툴이 추가되었습니다.
- 메시의 버텍스들을 편집할 수 있는 기능이 추가되었습니다.
- Perspective 카메라를 지원하며, 이를 위한 빌보드 옵션이 추가되었습니다.
- "Pirate Game"의 3D 버전인 "Pirate Game 3D" 데모 씬이 추가되었습니다.
- 애니메이션을 스크립트로 제어할 때, 애니메이션의 배속을 설정할 수 있도록 SetAnimationSpeed 함수가 추가되었습니다.
- 메시를 제작할 때, Ctrl 키(Mac OSX에서는 Command 키)를 누르면 가까운 버텍스로 커서가 스냅됩니다.
- 메시를 제작할 때, Shift 키를 누른 상태로 선분을 클릭하면 버텍스가 선분에 추가됩니다.
- 메시 제작 UI가 변경되었습니다.
- Bake 설정에서 그림자 설정(Receive Shadow, Cast Shadow)을 변경할 수 있습니다.
- Bake 다이얼로그의 UI가 변경되었습니다.
- Inspector UI가 변경되었습니다.
- Inspector에서 바로 에디터를 열 수 있으며, 바로 Bake를 할 수도 있습니다.
- 하위의 메시 그룹에 모디파이어와 본을 추가하고, 상위의 메시 그룹이 이를 제어할 수 있도록 개선되었습니다.
- Q&A 웹페이지를 여는 메뉴가 추가되었습니다.
- 메시를 생성할 때 폴리곤이 제대로 생성되지 않는 문제가 수정되었습니다.
- 낮은 배속이나 낮은 FPS로 설정된 애니메이션이 부드럽게 재생되지 않는 문제가 수정되었습니다.
- 애니메이션을 삭제할 때 Hierarchy가 갱신되지 않는 문제가 수정되었습니다.
- 게임 실행 시 Clipping Mask가 간헐적으로 동작하지 않는 문제가 수정되었습니다.
- 본을 연속으로 생성할 때, 첫번째 본의 IK 설정이 Disabled되는 문제가 수정되었습니다.
- 수동 백업 저장 시 간헐적으로 데이터가 누락되는 문제가 수정되었습니다.
- Inspector에서 컨트롤 파라미터를 제어할 때 발생하는 에러가 수정되었습니다.
- iOS 개발 환경에서 썸네일이 비정상적으로 출력되는 문제가 수정되었습니다.
- Mecanim을 사용하는 상태에서 Optimized Bake를 할 때 애니메이션 클립을 중복해서 생성하는 문제가 수정되었습니다.

1.1.1 (2018년 10월 11일)
- 본들을 연결하면 자식 본의 위치와 각도가 바뀌는 문제(v1.1.1)

1.1.2 (2018년 11월 8일)
- MP4 영상 내보내기 기능이 추가되었습니다. (Unity 2017.4부터 가능)
- GIF 애니메이션 품질을 4단계로 쉽게 설정할 수 있도록 변경하였습니다.
- GIF 애니메이션의 최고 품질은 기존보다 조금 더 향상되었습니다.
- 애니메이션 캡쳐 도중 중지할 수 있도록 UI가 변경되었습니다.
- Bake 다이얼로그에서 Lightweight Render Pipeline용 Shader 생성할 수 있습니다.
- Bake 다이얼로그에서 AnyPortrait에 맞게 Ambient Light를 검정색으로 변경하는 기능이 추가되었습니다.
- 애니메이션 재생에 관련된 스크립트 API에서 apPlayData를 활용할 수 있는 함수들이 추가되었습니다.
- 언어가 일본어로 설정된 경우, 메뉴를 선택할 때 일본어 홈페이지로 연결됩니다.
- Modifier에 오브젝트를 등록하는 모든 과정에서, 자동으로 편집 모드가 시작되도록 변경되었습니다.
- Perspective 카메라에서 렌더링하는 경우, 카메라 각도에 따라 Clipping Mask가 제대로 계산되지 않는 문제가 수정되었습니다.
- Rigging을 할 때, Blend 기능을 사용하면 가중치가 이상하게 적용되는 문제가 수정되었습니다.
- AnyPortrait의 리소스 경로를 인위적으로 수정하면 에러가 무한하게 발생하는 문제가 수정되었습니다.
- Bone의 기본 위치를 수정할 때, 클릭하자마자 마우스 위치로 Bone이 이동되는 문제가 수정되었습니다.

1.1.3 (2018년 11월 15일)
- 새로운 업데이트가 있을 경우 에셋 스토어로 바로 접속할 수 있는 기능이 추가되었습니다.
- 화면 캡쳐 속도와 품질이 향상되었습니다.
- 화면 캡쳐 해상도의 제한이 사라졌습니다.
- 화면 캡쳐를 할 때, 투명색이 제대로 적용되지 않는 문제가 수정되었습니다.
- 업데이트 로그 다이얼로그에서 일본어 홈페이지로 접속이 되지 않는 문제가 수정되었습니다.

1.1.4 (2018년 12월 18일)
- 렌더링 순서와 이미지를 실시간으로 변경할 수 있는 Extra 설정이 추가되었습니다.
- 드로우콜이 더욱 최적화되었습니다.
- Inspector UI에서 메시를 갱신하는 "Refresh Meshes" 버튼이 추가되었습니다.
- 매시의 재질을 제어하는 3종의 함수가 추가되었습니다.
- 상단 UI에서 불필요한 객체 정보 UI가 출력되지 않도록 변경되었습니다.
- 메시 그룹을 추가하는 다이얼로그에서 부모 메시 그룹이 나타나는 문제가 수정되었습니다.

1.1.5 (2018년 12월 24일)
- Unity 2018에서 메시의 기본 Depth를 수정할 수 없는 문제가 수정되었습니다.

1.1.6 (2019년 4월 19일)
- 유니티의 Timeline을 지원하여 시네마틱 장면 제작
- 노트북 과열 방지를 위한 에디터 성능 제한 기능 추가
- "편집 모드"가 켜질 때, "선택 잠금"이 같이 켜질지 여부 설정
- Scale이 음수의 값을 가져도 드로우콜이 증가되지 않도록 개선
- Important 옵션을 껐을 때 CPU가 더욱 최적화
- Hierarchy UI에서 항목의 순서를 변경하는 기능 추가됨
- 애니메이션을 특정 시점부터 재생하는 함수가 추가됨
- 일부의 optTransform의 Sorting Layer/Order를 변경하는 함수가 추가됨
- Animator의 Speed Multiplier 속성 적용
- Inspector UI의 디자인 개선
- 컨트롤 파라미터를 선택해서 모디파이어에 추가 가능
- Mecanim 설정의 "애니메이션 클립이 저장되는 경로"가 "상대 경로"로 변경
- 메시를 Physic 모디파이어에 추가할 때 "편집 모드"가 바로 켜지도록 변경
- 모디파이어에서 보간되는 Scale의 부호가 서로 다른 경우, Scale이 불연속적으로 보간되어 0이 되지 않도록 변경
- 모디파이어를 편집할 때, 컨트롤 파라미터의 값을 변경해도 "편집 모드"가 꺼지지 않도록 변경
- 애니메이션 이벤트를 복제할 수 있는 버튼 추가
- 자식인 메시 그룹이 애니메이션 클립에 연결될 때 경고 메시지 출력
- Ctrl + A를 눌러서 모든 버텍스 선택
- Ctrl + C, V를 눌러서 선택한 키프레임들을 복사
- 여러개의 키프레임들의 애니메이션 커브를 일괄 편집할 수 있도록 개선
- 애니메이션 커브를 타임라인 레이어에 관계없이 모든 키프레임에 적용하는 기능 추가
- Bake를 할 때, 씬의 Ambient Color의 설정을 자동으로 변경할 지 물어보는 다이얼로그 출력
- 본 설정 UI에 "Length" 항목 추가
- 일부 기능을 사용할 때 Console에 로그가 계속 출력되는 문제가 수정
- Hierarchy UI에서 발생하는 에러가 수정
- AnyPortrait 에디터를 유니티 에디터에 도킹한 상태에서 최대화를 켰다가 끄면 유니티가 멈추는 문제가 수정
- AnyPortrait 에디터를 연 상태에서, 씬을 전환하거나 유니티 에디터를 재시작할 때 발생하는 에러가 수정
- Once 타입의 애니메이션이 Animator에서 정상적으로 재생되지 않는 문제가 수정
- Extra Option이 본을 대상으로 하는 경우, 해당 Portrait를 더이상 열 수 없는 문제가 수정
- 모디파이어의 키를 복사할 때, Extra Option의 값이 누락되는 문제가 수정
- 컨트롤 파라미터의 동일한 값에 여러개의 키가 생성될 수 있는 문제가 수정
- Optimized Bake를 할 때, 애니메이션의 FPS가 정상적으로 적용되지 않는 문제가 수정
- Ctrl 키를 누르고 가까운 버텍스를 선택할 때, 버텍스가 마우스의 위치로 이동되는 문제가 수정
- "편집 모드"가 켜진 상태에서 컨트롤 파라미터의 값을 변경할 때 숨겨진 메시가 나타나는 문제가 수정
- 유효하지 않은 채널 정보를 가진 PSD 파일을 임포트할 수 없는 문제가 수정

1.1.7 (2019년 7월 10일)
- 재질과 쉐이더을 통합하여 관리하고 다양한 렌더링 기법을 적용할 수 있도록 "재질 라이브러리"와 "재질 세트"가 추가
- 모디파이어와 애니메이션 데이터를 최적화하여 프리팹의 파일 크기 감소
- CPU 부하가 적도록 "개선된 AsyncInitialize" 함수 추가
- 애니메이션 작업시 일부 추가, 삭제 과정에서의 처리 속도 향상
- Ctrl+C,V 키를 이용하여 다른 애니메이션 클립으로 키프레임 복사 가능
- 메시 그룹에 동시에 여러개의 메시, 메시 그룹을 선택하여 추가할 수 있도록 개선
- 애니메이션이나 모디파이어 선택시 자동으로 "Controller 탭"이 열릴지 여부를 사용자가 설정 가능
- 작업 도중에 실행 취소나 키값 변경 등의 이유로 메시의 "임시의 렌더링 여부"가 해제될지 여부를 사용자가 설정 가능
- 메시의 "임시의 렌더링 여부"를 리셋하는 버튼 추가
- Ambient Color 보정 다이얼로그에서 "Do Not Show this message"가 반대로 동작되는 문제 수정
- 애니메이션 키프레임을 Ctrl+C,V 키로 복사할 때, 기존 키프레임을 덮어쓰지 못하는 문제 수정
- PSD 파일을 가져올 때 레이어의 클리핑 설정이 적용안되던 문제 수정
- 양면 메시의 그림자와 노멀 벡터가 비정상적으로 계산되는 문제 수정
- 애니메이션의 Start Frame과 End Frame이 같은 경우 에디터에서 열리지 않는 문제 수정

1.1.8 (2019년 9월 8일)
- 리깅 모디파이어 개선
- 가중치를 수정하는 UI 추가
- 가중치 편집을 제한하는 "잠금 기능" 추가
- "브러시 모드" 추가
- 현재 본에 리깅된 버텍스들을 일괄 선택하는 기능 추가
- "자동 리깅(Auto-Rig)" 기능 대폭 향상
- 원형 그래프 방식으로 버텍스를 출력하는 기능 추가
- Bake 다이얼로그에 SRP에서 클리핑 메시를 렌더링하기 위한 옵션이 추가
- 재질 라이브러리에 LWRP 2D를 지원하는 패키지 추가
- 설정 다이얼로그에 새로 추가되는 본의 색상이 부모의 색상과 유사할지 여부에 관한 옵션 추가
- 본을 자식의 본으로 스냅하는 기능 추가
- 본을 복제하는 기능 추가
- LookAt 방식의 IK 컨트롤러가 단일 본 또는 IK 체인이 설정되지 않은 본에도 적용이 되도록 변경
- 좌우의 UI를 숨기는 버튼이 추가되었으며, 오른쪽 UI를 상하로 접는 버튼의 디자인이 변경
- Sorting Group 지원
- Sorting Order를 설정하는 옵션이 Bake 다이얼로그와 Inspector UI에 추가
- Sorting Order를 설정하는 옵션에 관련된 "SetSortingOrderChangedAutomatically" 함수 추가
- 애니메이션 클립의 대상의 메시 그룹을 변경할 때, 해당 메시 그룹의 부모 메시 그룹을 선택하면 애니메이션 데이터를 유지하는 기능 추가
- 애니메이션 복제시, 모디파이어의 Extra옵션과 애니메이션 이벤트가 복제되지 않는 버그 수정

1.2.0 (2019년 10월 28일)
- VR 지원
- 카메라의 Target Texture 속성 지원
- Bake 실행 시에는 빌보드 옵션이 적용되지 않고, 게임 실행 중에만 적용되도록 변경
- 메카님을 위한 애니메이션 클립 저장 경로에 공백이 있을 경우 제대로 저장되지 않는 버그 수정
- FFD의 크기를 임의로 수정할 때, 크기가 2인 경우 제대로 동작하지 않는 버그 수정
- 본을 Detach한 직후, 해당 본을 제어할 수 없는 버그 수정
- SetMeshImageAll, SetMeshColorAll과 같은 메시들을 일괄 제어하는 함수들이 정상적으로 동작하지 않는 버그 수정 (Bake를 1회 해야 적용됨)

1.2.1 (2019년 11월 25일)
- AnyPortrait 에디터의 성능이 일부 향상
- 애니메이션 타임라인 UI의 자동 스크롤 기능이 키프레임을 추가할 때, 오브젝트를 선택하고 편집할 때에도 동작하도록 개선
- 애니메이션 타임라인 UI의 키보드 단축키 입력처리가 개선
- AnyPortrait 에디터의 FPS 출력 방식이 개선
- 모디파이어의 Extra 옵션으로 자식 메시 그룹의 Depth를 변경하는 경우 Bake가 되지 않는 버그 수정
- 화면 캡쳐시 Extra 옵션에 의해 변경된 렌더링 순서가 적용되지 않는 문제 수정
- 애니메이션 타임라인 UI의 자동 스크롤 기능이 정상적으로 동작하지 않는 버그 수정
- 오브젝트 리스트 UI의 "작업을 위한 일시적 Show/Hide" 버튼이 잘못 출력되는 버그 수정

1.2.2 (2020년 1월 31일)
- 메시 복제 기능 추가
- 메시 그룹 복제 기능 추가
- 메시 그룹의 객체(메시와 하위 메시 그룹)들을 복제하는 기능 추가
- 메시 그룹의 메시를 다른 메시 그룹에 속하도록 이동시키는 기능 추가
- 다수의 키프레임을 선택한 상태에서 커브를 일괄적으로 편집할 때, "이전/중간/다음"을 각각 구분하여 편집하도록 개선
- 타임라인 UI의 상단부에서 Ctrl+Shift+클릭을 하면 타임 슬라이더를 바로 해당 위치로 이동
- 키프레임의 회전 값을 180도 이내로 제한할 지 여부를 변경하는 기능 추가
- 에디터 성능이 조금 더 향상됨
- Mac OS에서 처음 실행할 때, Metal과 관련된 안내 메시지가 등장
- Statistics 출력 내용에 본 개수가 추가됨
- Rigging 모디파이어의 Auto-Rig 버튼을 Ctrl 키(또는 Command 키)를 누른 상태에서 누르면 자동 리깅의 대상이 될 본을 선택할 수 있음
- 이미지의 텍스쳐를 선택하는 다이얼로그를 열 때, 텍스쳐 에셋들이 순차적으로 로딩되도록 변경됨
- Unity 2019.3에 추가된 URP(Universal Rendering Pipeline)를 지원하는 재질 프리셋이 추가됨
- 키보드의 방향키(또는 Shift+방향키)를 이용하여 선택된 객체를 이동, 회전 및 크기 제어할 수 있도록 개선
- 키보드의 Enter키나 Esc키를 눌러서 FFD 툴을 적용하거나 취소할 수 있도록 개선
- Bake를 할 때, 설정된 Color Space와 이미지들의 Color Space가 다를 경우, 일괄적으로 이미지의 Color Space를 변경하는 기능 추가
- 객체를 추가하거나 삭제시 "실행 취소"나 "다시 실행"이 더 정확하게 처리되도록 내부적으로 개선
- Advanced Manual 페이지로 이동하는 메뉴가 추가됨 (Unity Editor의 Window > AnyPortrait > Advanced manual)
- Hierarchy를 스크롤할 때 간헐적으로 에러 로그가 발생하는 버그 수정
- Inspector에서 Quick Bake 기능과 Open Editor and Select가 정상적으로 동작하지 않는 문제 수정
- Update Log 다이얼로그에서 간헐적으로 문장이 가운데 정렬로 나타나는 문제 수정
- 메시 그룹 내의 객체들의 순서를 바꿀 때, 클리핑 메시나 하위 메시 그룹간에 순서 변경이 안되는 문제 수정

1.2.3 (2020년 4월 25일)
- 2개의 애니메이션 클립이 이전보다 더 자연스럽게 전환되도록 개선
- 애니메이션 클립의 전환과 레이어링 관련 처리를 전체적으로 크게 개선
- 애니메이션 클립이 전환될 때, 컨트롤 파라미터가 너무 빠르게 전환되는 문제 수정
- 애니메이션에서 정수형(Integer) 컨트롤 파라미터의 값이 음수인 경우, 정상적으로 연산이 안되는 문제 수정
- 유니티의 타임라인(Timeline) 기능 사용시 클립이 반복하여 재생될 때 블렌딩이 정상적으로 되지 않는 문제 수정
- 유니티의 타임라인(Timeline) 기능을 미리보는 "Timeline Simulator" 사용시, 에디터에서 게임이 실행하지 않을 때도 미리보기가 되도록 개선
- 모디파이어를 편집하는 상태에서 메시 그룹의 하위 Hierarchy UI에서 가로 스크롤이 되지 않도록 변경
- Hierarchy UI에서의 순서가 컨트롤 파라미터 탭에도 적용되도록 개선
- 모디파이어 UI의 상단 영역과 여백들을 조절하여 정보들이 더 잘 보이도록 개선
- 마우스를 드래그하다가 작업 공간 밖으로 커서가 이동해도 정상적으로 동작되도록 개선
- 설정 다이얼로그의 옵션이 기본값과 다르면 색상이 다르게 출력되도록 변경
- 설정을 기본값으로 되돌릴 때 경고 메시지가 등장
- Mac OSX에서 Backspace와 Delete가 구분되어 있지 않으므로, 두개의 키를 같은 단축키로 인식하도록 변경
- 삭제, Detach 버튼에 휴지통 아이콘을 모두 추가
- 애니메이션 클립의 우측 UI를 개선
- 애니메이션 타임라인 UI의 확대/축소를 Ctrl키를 누르고 마우스 휠을 스크롤해도 가능하도록 개선
- Hierarchy UI가 갱신될 때 불필요하게 메모리가 사용되거나 성능이 떨어지는 문제 수정
- 모디파이어 편집 시작 및 종료시, Hierarchy에서 객체 선택시, 메시를 모디파이어에 추가, 삭제시 등 많은 과정에서 실행 속도가 향상되도록 개선
- 애니메이션 클립의 개수가 많으면 에디터의 처리 시간이 급격히 증가하는 문제 수정
- 에디터에서 Portrait를 처음 로딩할 때, 로딩 과정을 볼 수 있는 팝업이 등장하도록 개선 (Inspector에서 에디터를 열 경우는 제외)
- 렌더링되지 않는 메시에 대한 불필요한 처리를 개선하여 처리 속도 향상
- 설정 다이얼로그에서 본의 크기와 화면 확대에 따른 크기 증가 여부 설정 가능
- 작업 공간에서 본을 선택할 때의 외곽선의 색상이 본 색상과 구분되도록 개선
- 작업 공간에서 본을 선택하면, 본의 외곽선이 약하게 반짝거려서 구분하기 쉽도록 변경
- 새로운 본의 외형인 "바늘 모양"이 추가
- 마우스로 클릭시 본을 조금 더 선택하기 쉽도록 개선됨
- 메시 그룹의 본 설정 UI에서, 본의 색상을 쉽게 설정할 수 있는 프리셋 버튼이 추가
- "부모와 유사한 색상"으로 본이 생성되도록 옵션이 설정된 경우, 부모와 아주 유사한 색상을 갖지는 않도록 변경
- 기존과 다른 색상으로 구성된 리깅 그라데이션이 보여지는 "Vivid" 옵션 추가
- 버텍스의 원형 리깅 가중치의 선택된 영역이 더 구분되기 쉽도록 만드는 옵션 추가
- 버텍스의 원형 리깅 가중치의 크기를 설정하는 옵션 추가
- 버텍스의 원형 리깅 가중치에서 선택된 가중치의 값이 작으면 잘 보이지 않으므로, 원형의 중앙에 별도로 표시
- 버텍스의 원형 리깅 가중치의 클릭 영역이 기존보다 확대됨
- 리깅 작업 화면의 하단에 선택된 메시에 리깅이 되지 않은 본을 반투명하게 보여주거나 숨길 수 있는 버튼이 추가됨
- 리깅 가중치를 조절하는 단축키 추가 (Z,X키 : 0.02씩 증감, Shift+Z,X키 : 0.05씩 증감)
- 리깅 작업 중에 Ctrl, Shift, Alt키를 누른 상태에서는 본이 선택되지 않도록 개선
- 본 설정 화면에서 본을 삭제하는 단축키(Delete키) 추가
- 모디파이어의 Color 옵션으로 객체를 숨기거나 보여지게할 때, 반투명되지않고 바로 전환되는 옵션 추가
- AnyPortrait 패키지 설치 경로를 바꿀 수 있는 옵션 추가 <color=red>(자세한 설명은 홈페이지를 확인하세요)</color>
- Important 옵션을 끄고 고정 FPS로 다수의 캐릭터들을 배치하고 실행할 때의 성능을 개선
- 모디파이어 편집을 시작하거나 종료할 때 메시, 본의 일시적 숨김이 유지되도록 옵션을 선택한 경우, 다른 작업을 하더라도 숨김 설정이 계속 유지되도록 개선
- Unity 2019.3부터 툴팁의 위치로 인하여 버튼을 누르기가 힘들게 되어, 해당 버전부터는 툴팁이 나오지 않도록 변경
- Inspector UI에서 Bake가 되지 않은 상태에 관한 문구를 변경
- PSD 파일을 열때, 하위 메시 그룹이나 그 하위의 메시로의 매핑이 해제되버리는 문제 수정
- 하위 메시 그룹의 객체들의 출력 여부 정보(눈 모양의 아이콘)가 정상적으로 갱신되지 않는 문제 수정
- 모디파이어를 편집하는 상태에서 GUI Control 에러 로그가 발생하는 문제 수정
- 등록된 Root Unit이 없을때, Inspector에서 바로 에디터를 열 경우, 정상적으로 에디터가 실행되지 않는 문제 수정
- 애니메이션의 대상 메시 그룹을 부모 메시 그룹으로 변경하고 데이터를 초기화한 뒤, 실행 취소를 하면 관련된 모든 데이터가 손상되는 문제 수정
- 하위 메시 그룹의 본에 리깅이 된 후, 하위 메시 그룹이 제거 되었을 때 해당 정보로 인해 Bake가 실패되는 문제 수정
- 모디파이어 편집시, 메시 그룹의 Hierarchy를 통해서 한개의 본과 한개의 메시, 또는 메시 그룹을 동시에 선택할 수 있는 문제 수정
- FFD 툴을 이용하는 중에 다른 객체를 선택하면 FFD가 해제되지 않는 문제 수정
- Physic 모디파이어 편집 중에 다른 객체를 선택하면 GUI 에러 로그가 발생하는 문제 수정
- Physic 모디파이어 편집 중에 객체를 선택하면 상단 UI가 이상하게 보여지는 문제 수정
- Hierarchy UI에서 2레벨의 자식 항목의 앞쪽 여백이 이상하여 부모 항목과 구분하기 어려운 문제 수정
- 애니메이션 타임라인 UI의 세로 스크롤바를 마우스를 이용하여 움직일 때, 정상적으로 스크롤되지 않는 문제 수정
- 애니메이션의 길이나 Loop 옵션을 변경할 때, "자동 루프 키프레임"들이 갱신되지 않는 문제 수정
- 애니메이션의 길이나 Loop 옵션을 변경했을 때, "실행 취소"가 안되는 문제 수정
- 애니메이션의 키프레임 속성 UI에서 Transform 항목의 벡터 속성들이 (X, X)로 출력되는 것을 (X, Y)로 출력되도록 수정
- 재질 라이브러리(Material Library)에서 텍스쳐 타입의 커스텀 프로퍼티를 만들고 "Texture Per Image"를 선택한 경우 Bake가 되지 않는 문제 수정

1.2.4 (2020년 7월 20일)
- 다중 선택 기능 추가
- 지글(Jiggle) 본 속성 추가
- 보라색 계열의 본의 "선택된 상태의 외곽선 색상"이 더 잘보이도록 변경
- 작업 영역에서 메시를 선택하는 처리의 속도가 향상
- 하단 UI의 "선택된 키와 객체"의 이름과 정보가 삭제
- 애니메이션 커브의 양쪽의 포인트들의 클릭 범위 확대
- PSD 파일로부터 불러온 후, 메시를 처음 선택하면 바로 버텍스를 초기화할 것인지 묻는 다이얼로그 추가(옵션에서 설정 가능)
- Unity 2019.3의 신규 기능인 "Enter Play Mode 옵션"을 지원
- "Forum" 메뉴 추가
- 키보드의 방향키를 이용하여 객체를 편집하고 실행취소한 직후에 방향키를 다시 누르면, 실행취소가 되지 않는 버그 수정
- 애니메이션 편집시, 실행 취소를 2회 이상 연속으로 누르면 애니메이션이 재생되지 않고 편집도 되지 않는 버그 수정
- 애니메이션 편집시, 간헐적으로 GUI 에러 로그가 발생하는 문제 해결
- Rigging 모디파이어에서 "포즈 테스트"를 할 때, 실행 쉬소를 하면 더이상 편집이 되지 않는 버그 수정
- "All Bones to Layers" 버튼을 이용하여 본을 애니메이션의 Morph 모디파이어에 등록할 수 있었던 문제 수정
- 애니메이션 편집 모드를 켠 직후에, 클릭을 해도 본이 간헐적으로 선택되지 않는 버그 수정
- 편집 모드를 켠 직후, "자식 메시 그룹"이 선택된 상태에서 기즈모를 클릭하면, 선택이 해제되는 문제 수정
- 모디파이어에서 복사/붙여넣기를 한 직후, Hierarchy UI에서 객체의 현재 렌더링 여부가 아이콘으로 바로 반영되지 않았던 버그 수정
- 게임을 시작할 때, 컨트롤 파라미터의 초기값이 "설정된 기본값"이 아닌 "0 또는 0에 가까운 값"으로 설정되는 버그 수정
- 메시 편집 화면에서 실행 취소를 하면 모든 선분이 사라지는 버그 수정
- Hierarchy UI가 갱신된 직후, 선택된 항목에 포커스가 반영이 안되는 문제 수정
- Ctrl, Shift키를 누른 직후, 단축키로 도구 변환이 빠르게 되지 않는 문제 수정
- 모디파이어를 편집하는 중에 실행 취소를 하면 객체의 출력 여부가 초기화되는 문제 수정
- IK가 설정된 부모 본의 키프레임이 이동 툴에 의해 자동으로 생성되는 직후 실행 취소를 하면 본이 사라지는 문제 수정

1.2.5 (2020년 9월 24일)
- Non-Uniform 스케일 옵션 추가
- 옵션에 따라서 리깅된 메시가 본의 크기 반전에 맞추어서 같이 플립되어 렌더링
- 레거시 프리팹과 동일하게 Unity 2018.3에 도입된 새로운 프리팹 시스템에서도 Apply가 가능하도록 개선
- Inspector UI에서 프리팹과의 연동을 제어하는 기능 추가
- 알 수 없는 원인에 의해서 비정상적으로 저장된 백업 파일로부터 복구시, 최소한 작업이 가능한 수준으로 복원되도록 개선됨
- Soft Selection 범위, Blur 브러시, Rigging 모디파이어의 브러시의 크기의 최대값이 기존의 3배로 증가
- 메시 그룹의 메시들이나 자식 메시 그룹들, 본들을 일괄적으로 삭제하는 버튼과 단축키가 추가
- 에디터 언어를 일본어로 선택할 때, 어색했던 일부 단어들을 수정
- 자식 메시 그룹이 움직이면, 리깅이 적용된 메시들이 이상하게 움직이던 문제가 수정됨
- Rigging 모디파이어의 Pose Test 기능을 사용할 때 실행 취소를 하면 본을 바로 제어하지 못했던 문제가 수정됨
- Unity 2019.3 및 이후 버전의 에디터에서 "애니메이션 이벤트 다이얼로그"의 길이가 짧았던 문제가 수정됨
- Soft Selection를 켠 상태에서 일부 버텍스들을 선택한 후, Ctrl+A 키를 눌러서 모든 버텍스를 선택하면 편집이 정상적으로 되지 않는 문제가 수정됨
- 3D 월드에 캐릭터를 배치하고 빌보드 방식으로 렌더링을 하는 경우, 지글본이 카메라와의 각도에 따라 비정상적으로 동작하는 문제가 수정됨 (단, Physics 모디파이어에서도 동일한 현상이 있지만 성능상의 문제로 개선 작업에서 제외됨)
- 마지막으로 기즈모가 보여졌던 곳을 클릭하면 해당 위치의 객체가 선택되지 않는 문제가 수정됨
- Make Mesh의 Edit 탭이 켜진 상태에서 메시를 복제하거나 다른 메시를 선택하면 기즈모가 활성화되지 않던 문제가 수정됨
- Make Mesh의 Edit 탭이 켜진 상태에서 버텍스를 선택하고 바로 다른 메시를 선택하면, 기즈모가 사라지지 않던 문제가 수정됨

1.2.6 (2020년 11월 7일)
- 에디터로 캐릭터를 열 때, 컨트롤 파라미터들의 기본값이 범위를 벗어나면 자동으로 보정하도록 변경
- 에디터에서 대상을 회전하거나 대상의 크기를 변경할 때, 위치값을 보정하여 이동하지 않도록 개선
- Bake를 할 때, "이미지가 설정되지 않는 메시"가 있음을 알려주는 메시지가 추가됨
- 기본 크기가 음수인 메시를 기즈모로 편집할 수 없었던 문제가 수정됨
- 기본 크기가 음수인 메시에 Rigging 또는 Transform 모디파이어가 적용된 경우 정상적으로 동작하지 않는 문제가 수정됨

1.3.0 (2021년 4월 10일)
- 게임 실행 중의 Rigging 모디파이어와 애니메이션 처리 성능 향상
- 에디터 내의 대부분의 단축키를 사용자가 지정할 수 있는 메뉴가 설정 다이얼로그에 추가
- 단축키를 누른 경우 나타나는 상단 메시지의 내용이 더 상세하게 변경되며, 불필요한 경우는 나타나지 않도록 개선
- (추가된 단축키) Page Up, Page Down : 애니메이션 타임라인 UI를 상하로 스크롤
- (추가된 단축키) Ctrl + <, > : 이전, 다음 키프레임으로 이동
- (추가된 단축키) N : 애니메이션의 "Auto-Key" 기능을 켜거나 끄기
- (추가된 단축키) ~ : 에디터 왼쪽의 Hierarchy 탭과 Controller 탭을 전환
- (추가된 단축키) Enter : 메시 제작시 Make Polygon 기능이 실행
- (추가된 단축키) 숫자 1~6 키 : 메시 편집 메뉴를 전환
- (추가된 단축키) F2 : 현재 오브젝트의 이름을 수정하는 UI로 포커스가 이동    
- (추가된 단축키) I : 보기 프리셋 켜거나 끄기
- (추가된 단축키) 숫자 1~5 키 : 보기 프리셋 규칙 전환
- (추가된 단축키) Alt + O : 로토스코핑 켜거나 끄기
- (추가된 단축키) 숫자 9 키 : 이전 로토스코핑 이미지 파일
- (추가된 단축키) 숫자 0 키 : 다음 로토스코핑 이미지 파일
- (추가된 단축키) D : 편집 모드시 여러개의 모디파이어 동작 여부 전환
- (추가된 단축키) Alt + D : 편집 모드시 선택 잠금을 해제해도 모디파이어의 대상이 선택되지 않도록 설정
- (추가된 단축키) Alt + G : 편집 모드시 모디파이어의 대상이 되지 않는 객체를 회색으로 출력
- (추가된 단축키) Alt + B : 편집 모드시, 본의 처리 결과 미리보기
- (추가된 단축키) Alt + C : 편집 모드시, 색상 처리 결과 미리보기
- "보기 메뉴"가 추가되어 기존의 보기 아이콘의 역할을 대체
- 기존의 잔상(Onion Skin), 메시, 본의 출력 옵션 버튼들이 삭제됨 (설정 다이얼로그에서 다시 나타나게 설정 가능)
- 작업 공간에서 오브젝트들이 어떻게 출력되고 편집될 수 있는지를 나타내는 상태 아이콘들이 우측 상단에 등장    
- 작업 공간에서 어떤 본과 메시들을 출력할지를 사용자가 직접 미리 지정할 수 있는 "보기 프리셋" 기능이 "보기 메뉴"에 새롭게 추가됨
- 모디파이어 잠금 옵션이 삭제되고, "편집 모드 옵션"으로 대체 (보기 메뉴에 추가됨)
- 기존의 자동 메시 생성 기능은 삭제되고, 완전히 새롭게 개선된 자동 메시 생성 기능이 제공
- 메시 설정 화면에서도 메시를 바로 생성 가능
- 메시를 생성하기 위한 영역(Area)를 간편하게 마우스로 편집할 수 있도록 개선
- PSD 파일로부터 생성된 메시를 선택할 때 버텍스를 삭제할지 물어보는 메시지가 나타나는 옵션 선택시, 이 메시지에 자동으로 메시를 생성하는 선택지가 추가
- 작업 공간의 배경에 외부 이미지를 출력하는 "로토스코핑" 기능이 보기 메뉴에 추가
- 컨트롤 파라미터에 의한 모디파이어에서 2개 이상의 키를 조합하여 붙여넣을 수 있는 기능이 추가
- 최대 4개의 슬롯에 값을 저장한 후, 이를 더한 값이나 평균 값의 형태로 붙여넣기 가능
- 최소의 조건만 만족하면 동일한 객체가 아니더라도 복사된 값을 붙여넣을 수 있도록 개선 (단, 애니메이션에서의 복사&붙여넣기는 예외)
- 특정 메시의 Sorting Order를 가져올 수 있는 함수 추가
- AddForce와 같은 물리 함수가 "지글본"에도 적용되도록 개선
- 현재 재생 중인 apOptRootUnit이나 인덱스를 리턴하는 함수 추가
- 에디터의 상단 UI, 오른쪽 첫번째 UI의 크기가 축소
- 컨트롤 파라미터 UI 축소되어 한 화면에 더 많은 컨트롤 파라미터 UI가 나타나도록 개선. (편집 바로가기 버튼이 삭제됨)
- 애니메이션 타임라인 UI에서 드래그를 하여 키프레임을 선택할 때, 상하좌우로 자동으로 스크롤이 되도록 개선
- 메뉴에서 메시를 선택할 때, 메시의 영역(Area) 또는 피벗(Pivot)에 맞게 자동으로 화면이 스크롤되도록 개선
- "컨트롤 파라미터들을 일괄적으로 타임라인에 추가하기" 기능 추가
- 메시 그룹의 Hierarchy UI의 Bone 탭의 하위 메시 그룹 순서가 Depth에 맞게 정렬
- 일괄적으로 객체들을 타임라인에 추가할 때, Hierarchy의 순서와 동일하게 정렬
- "타임라인 레이어를 Depth 기준으로 정렬하는 버튼"을 누르면, 타임라인 레이어가 Hierarchy에서의 순서와 동일하게 정렬
- Bake 다이얼로그에서 Depth To Order 방식을 사용시, 1이 아닌 그 이상의 간격으로 Sorting Order를 할당할 수 있는 옵션 추가
- 이미지가 한개인 경우, 메시를 생성하면 해당 이미지가 자동으로 설정됨
- 외부의 파일을 열거나 저장할 때, 마지막 경로가 기록되어 다음에 해당 다이얼로그를 열 때 기본 경로로 사용됨
- "Auto-Key" 기능이 편집 모드가 종료된 이후에도 유지되는 옵션 추가
- 메시 그룹의 이름을 바꿀 때, 이것이 다른 메시 그룹의 자식으로 등록되었다면, 연결된 객체의 이름이 동기화되어 같이 변경되도록 개선 (그 반대로는 동기화되지 않음)
- apOptTransform의 내부 함수인 GetMeshTransform, GetMeshGroupTransform에서 발생하는 에러가 수정됨
- 빌보드 옵션을 끈 상태에서도 Transparency Sort Mode가 강제로 Orthographic으로 고정되는 문제가 수정됨
- 게임 실행 중에 "컨트롤 파라미터" 타입의 애니메이션 타임라인의 보간이 정상적으로 처리되지 않는 문제가 수정됨
- Ctrl, Shift, Alt키를 누른 후 다른 단축키를 뗀 이후에도 잠시동안, 이 키들이 계속해서 입력된 것 같은 "고스트 현상"이 수정됨
- Unity 에디터 특정 버전에서, 컨트롤 파라미터가 선택된 상태에서 에디터 외부로 커서가 나갔다 들어온 직후, 다른 컨트롤 파라미터 UI를 클릭할 때 제대로 선택되지 않는 문제가 수정됨
- 에디터의 옵션이 저장될 때, 너무 많은 레지스트리 값이 생성되는 문제가 수정됨
- 사용자의 기기 환경에 따라서 백업 파일이 스펙과 다르게 잘못 저장되는 문제가 수정됨 (이전 버전에서의 백업 파일을 이번 버전에서 열 수 있지만, 반대로 이번 버전에서 생성되는 백업 파일은 이전 버전에서 열 수 없음)
- "PSD 다시 가져오기" 기능에서 "메시 그룹의 기본 위치, 회전, 크기"가 적용되지 않아서 미리보기가 이상하게 출력되는 문제가 수정됨
- 본 구조를 파일로 내보낼 때, 지글본(Jiggle Bone) 설정이 누락되는 문제가 수정됨
- 메시 그룹이 연결되지 않은 애니메이션 클립의 UI에서 일부 버튼이 누를 수 있는 것처럼 나타나는 문제가 수정됨
- 모디파이어 이름 중 "Physic"을 "Physics"로 수정됨
- PSD 파일을 가져올 때, 레이어의 불투명도가 적용되지 않는 문제가 수정됨
- 양면 메시에 Physics 모디파이어가 적용된 경우 Bake를 할 수 없는 문제가 수정됨
- apPortrait의 GameObject 또는 그 상위의 GameObject들의 크기가 반전된 상태에서 Bake를 하면, Scale을 원래대로 복원해도 정상적으로 렌더링되지 않는 문제가 수정됨
- "리깅 가중치가 적용되지 않은 본들은 회색으로 보이기"가 "Pos-Paste" 기능을 수행한 직후에는 갱신되지 않는 문제가 수정됨
- 최신 macOS에서 Unity Editor 에러로 인하여 PSD 파일을 여는 과정에서 다이얼로그가 강제로 종료되는 문제가 수정됨 (단, 해당 에러 로그는 macOS 또는 Unity에서 업데이트하기 전까지는 계속 발생 가능)
- 자식 메시 그룹의 모디파이어를 수정할 때 실행 취소를 하면 편집이 불가한 상태가 되버리는 문제가 수정됨
- 애니메이션으로 루트 유닛이 전환될 경우 1프레임 잔상이 남는 문제가 수정됨
- Hide, Show를 호출한 이후 스크립트로 애니메이션을 재생할 때, 2번째 이후의 루트 유닛으로 정상적으로 전환되지 않는 문제가 수정됨
- 스크립트로 애니메이션을 재생할 때, 첫 프레임에서 컨트롤 파라미터에 처리 결과가 적용되지 않는 문제가 수정됨

1.3.1 (2021년 4월 19일)
- 중첩된 키프레임 생성시 실패 메시지가 나타나지 않도록 변경됨
- 애니메이션 작업 중 데이터 생성 직후 실행 취소를 하면 다른 애니메이션 클립의 일부 데이터가 초기화될 수 있는 문제가 수정됨

1.3.2 (2021년 7월 10일)
- 에디터 동작 성능 향상
- 별도의 플러그인을 설치하여 에디터 성능을 추가적으로 향상시키는 [가속 모드] 추가 (설정 다이얼로그에서 변경)
- 게임 플레이에서 전반적인 실행 성능 향상
- 모디파이어 업데이트시 발생하는 메모리 문제 개선
- [보기 메뉴]에 작업 공간에 임의의 직선을 출력하는 [가이드라인] 기능 추가됨
- Hierarchy에서 우클릭을 하여 메뉴를 열고 다양한 기능 사용 가능
- Hierarchy에서의 우클릭 메뉴에서 [검색] 기능이 추가됨
- 다른 apPortrait에 연결하여 애니메이션이나 컨트롤 파라미터를 동기화하여 같이 제어할 수 있는 [동기화 함수] 추가
- 에디터에서 새로운 포트레이트(Portrait) 생성시 텍스트 박스에 자동으로 포커스
- 메시 그룹 설정에서 복수개의 객체들을 선택한 상태에서 Duplicate, Migrate 등의 기능을 동시에 사용할 수 있도록 개선
- 컨트롤 파라미터와 연동된 모디파이어의 편집 화면에서 복수개의 객체들을 선택한 상태에서 편집된 값을 동시에 복사, 붙여넣기 하거나 초기화할 수 있도록 개선
- PSD 파일 가져오기 다이얼로그에서 Ctrl, Shift 키를 이용하여 여러개의 레이어를 선택할 수 있도록 개선
- 대상 물리 힘만 비활성화하는 RemoveForce 함수와 오버로드된  RemoveTouch 함수 추가
- 2개 이상의 컨트롤 파라미터등을 이용하여 모디파이어에서 객체의 Scale를 변경할 경우, Additive 방식이 정상적으로 동작하지 않는 문제가 수정됨
- 자식 메시 그룹의 객체들을 복사, 이동한 직후 실행 취소를 하면 Hierarchy UI에서 나타나지 않는 문제가 수정됨
- 실행 취소, 되돌리기를 복합적으로 반복하면 간헐적으로 편집 상태가 정상적으로 복구되지 못하는 문제가 수정됨
- Unity의 Time.timeScale을 변경하면 Important 옵션이 꺼진 객체들의 애니메이션이 이상하게 동작하는 문제가 수정됨
- 메시 그룹의 객체들의 Migration 이후 실행 취소가 되지 않는 문제가 수정됨
- 메시 그룹에서 객체들의 순서(Depth)를 수정한 이후 실행 취소가 되지 않는 문제가 수정됨
- 시스템 환경에 따라서 경로 설정값에 이스케이프 문자가 추가되어 경로를 인식하지 않는 문제가 수정됨
- 시스템 환경에 따라서 인코딩이 UTF-8이 아니게 되어 파일 저장 및 열기시 발생하는 문제가 수정됨
- 본의 Scale이 반전된 경우 회전 기즈모가 반대로 동작하는 문제가 수정됨
- 실행 환경에 따라서 물리 모디파이어가 낮은 FPS에서 실행되는 것처럼 보이는 문제가 수정됨

1.3.3 (2021년 7월 20일)
- 에디터 타이머 및 FPS 카운터 로직 개선
- "Color Only" 모디파이어 추가
- 실행 취소 데이터가 너무 많이 생성되어 에디터가 느려지는 문제가 수정됨
- 다른 메시 그룹에 속한 메시 그룹을 선택하여 편집할 때 Hierarchy UI가 제대로 갱신되지 않는 문제가 수정됨
- 다른 메시 그룹에 속한 본을 대상으로 Pos-Paste 기능을 이용하여 리깅 가중치를 할당하면 Bake시 에러가 발생하는 문제가 수정됨
- 작업 공간에서 본과 메시가 제대로 선택되지 않는 문제가 수정됨

1.3.4 (2021년 10월 26일)
- 메시, 본의 소켓들을 Inspector UI에서 확인 가능
- 애니메이션 이벤트들을 Inspector UI에서 확인 및 클립보드로의 복사 가능
- UnityEvent와 유사하게 콜백(Callback) 호출 방식으로 애니메이션 이벤트를 호출하도록 변경 가능
- Hierarchy UI에서 AnyPortrait로 만든 GameObject에 아이콘이 추가됨 (설정 변경 가능)
- "회전 잠금" 옵션이 추가됨
- "벡터 방식"의 회전 보간 옵션이 추가됨
- 서로 다른 캐릭터들의 본들의 움직임을 동기화하는 함수가 추가됨
- 애니메이션이 동기화되지 않은 서로 다른 캐릭터들의 루트 유닛(RootUnit) 인덱스를 동기화하는 함수가 추가됨
- 여러개의 대상들을 선택하여 한번에 Extra Option을 설정 가능
- 최대/최소 범위를 초과하는 Depth 변경을 UI에서 볼 수 있도록 개선
- 여러개의 메시들의 Depth를 변경하는 경우, Depth 변경 UI에서 각각 선택하여 어떻게 변경되는지를 확인 가능
- 이미지의 크기를 변경하는 UI가 개선됨
- Hierarchy 우클릭 메뉴에서 여러개의 객체들을 한번에 삭제하는 메뉴가 추가됨
- 여러개의 메시들을 일괄적으로 자동 생성하는 기능이 추가됨
- 가이드라인을 추가할 때, 옵션에 상관없이 바로 작업 공간에서 가이드라인이 보여지도록 변경됨
- 메시 그룹의 본(Bone) 탭을 클릭할 때, "본 숨기기 옵션"를 해제하여 작업 공간에 본들이 보여지도록 개선됨
- 보기 메뉴에 "배경색 반전" 기능이 추가됨
- 메시 편집 화면에서 설정 탭을 선택한 상태에서도 내부의 폴리곤들이 보여지도록 변경됨
- 메시 편집 화면의 "Make Mesh > Edit" 탭에 버텍스들을 복사할 수 있는 기능이 추가됨 (다른 메시로도 복사 가능)
- 게임 중에 애니메이션 업데이트 배속을 변경할 수 있는 함수가 추가됨
- 시작 다이얼로그가 변경됨
- 메뉴에 "Video Tutorials" 항목이 추가됨
- Make Mesh의 Add 툴에서 버텍스를 클릭하여 선택하는 처리가 향상됨
- 여러개의 메시들을 대상으로 Extra Option에서의 Depth 설정을 변경하는 경우 그 결과가 일관적이지 않은 문제가 수정됨
- Extra Option에서의 Depth 설정이 메시 전체의 범위를 벗어난 복수개의 요청들이 정상적으로 처리되지 않는 문제가 수정됨
- Prefab인 대상을 열때 에러로 인하여 한번에 열 수 없었던 문제가 수정됨
- 설치 위치가 변경된 경우, 에디터를 한번에 열 수 없는 문제가 수정됨
- "PSD 파일 다시 가져오기" 화면에서 "PSD 세트 새로 생성" 기능이 동작하지 않는 문제가 수정됨
- 실제 이미지 파일의 크기와 에셋으로 임포트된 이미지의 크기가 다른 경우 자동 메시 생성 기능이 제대로 동작하지 않는 문제가 수정됨
- Color 모디파이어에서 Extra Option이 동작하지 않는 버그가 수정됨

1.3.5 (2022년 1월 12일)
- 유니티 2021, 2022의 URP 지원
- 유니티 2021, 2022.a의 URP에서 클리핑 메시가 제대로 렌더링 되지 않는 문제가 수정됨
- "재질 병합" 기능 추가됨    
- Bake를 실행할 때, Bake의 설정 중 "Render Pipeline"의 설정이 프로젝트의 설정과 맞지 않다면 자동으로 변경할지를 묻는 메시지가 표시됨
- 캐릭터의 업데이트 시간을 콜백 함수로 받아서 유연하게 제어할 수 있는 스크립트 함수가 추가됨
- 애니메이션 편집시, 컨트롤 파라미터의 키프레임의 값을 편집 중인 경우엔 왼쪽 UI의 컨트롤러의 색상이 붉은 색으로 표시되도록 변경됨
- 재질 라이브러리에서 재질 세트를 생성 직후엔 해당 재질 세트가 바로 선택됨
- Soft Selection, Blur, Rigging Brush의 브러시의 크기를 조절하기 편하도록 개선됨
- Bake, Optimized Bake, Quick Bake, Refresh Meshes 기능을 사용할 때, 첫번째 루트 유닛만 보여지도록 변경됨
- "SetMeshColorAll"과 같이 메시들의 재질의 속성을 일괄적으로 변경하는 스크립트 함수를 사용할 때 드로우콜이 증가하지 않도록 개선됨
- 재질 라이브러리에서 Shader 에셋을 변경한 이후에 다시 변경하지 못하는 문제가 수정됨
- 재질 라이브러리에서 실행 취소를 했을 때, 화면이 갱신되지 않는 문제가 수정됨
- 프로젝트 내에 "Action"이라는 이름을 가진 클래스가 존재하는 경우 스크립트 에러가 발생하는 문제가 수정됨
- apPortrait의 함수를 이용하여 메시의 이미지를 변경한 이후 색상을 변경할 수 없었던 문제가 수정됨
- 컨트롤 파라미터와 연결되는 애니메이션의 키프레임을 추가할 때, 초기값이 무조건 0으로 지정되는 문제가 수정됨
- 텍스쳐 에셋을 선택하는 다이얼로그의 레이아웃이 비정상적이었던 문제가 수정됨
- 애니메이션을 전환할 때, 다음에 재생되는 애니메이션에 이전의 애니메이션에 존재했던 타임라인 레이어가 없다면 기본값으로 자동으로 복구되는 기능이 비정상적으로 동작하는 문제가 수정됨
- 애니메이션을 전환할 때 컨트롤 파라미터의 블렌딩이 어색했던 문제가 수정됨
- 메카님, 타임라인에 의해서 Loop가 아닌 애니메이션이 재생될 때, 첫번째 프레임이나 마지막 프레임이 정상적으로 표현되지 못하는 문제가 수정됨

1.4.0 (2022년 7월 16일)
- Morph 모디파이어에서의 작업 효율을 높이기 위한 커브 기반의 "핀" 도구가 메시의 요소로서 새롭게 추가됨
- 핀 편집을 위한 다양한 도구가 추가됨
- Morph 모디파이어에서 "버텍스 편집 모드"와 "핀 편집 모드"를 구분하는 UI가 추가됨
- 작업 공간에서 출력되는 버텍스 이미지가 예쁘게 변경되고 렌더링 성능이 향상됨
- 컨트롤 파라미터 UI의 이미지가 변경되고, 렌더링 성능이 향상됨
- Bake 다이얼로그, Inspector, Hierarchy 등의 다수의 아이콘들이 예쁘게 변경됨    
- 애니메이션 타임라인의 이미지를 선명하게 출력하기 위해 "픽셀 퍼펙트(Pixel Perfect)" 기법이 적용됨
- 애니메이션이 전환될 때 다음 애니메이션에서 정의되지 않은 컨트롤 파라미터에 "기본값"이 적용될지 "마지막 값"이 유지될지 결정하는 옵션이 Bake 다이얼로그에 추가됨
- 캐릭터가 1프레임만에 멀리 이동하는 경우에 물리 기능(지글본, 물리 재질)이 이상하게 동작하는 것을 막는 옵션이 Bake 다이얼로그에 추가됨
- 에디터에서 Vector 타입의 컨트롤 파라미터의 UI의 높이를 확장하는 옵션이 설정 다이얼로그에 추가됨    
- 보기 프리셋이 다른 객체 선택시에도 계속 켜져 있을지 결정하는 옵션이 설정 다이얼로그에 추가됨
- Inspector UI의 성능이 개선됨
- 탭으로 필요한 속성을 쉽게 찾을 수 있도록 개선됨
- Inspector UI의 아이콘 및 레이아웃이 예쁘게 변경됨
- 유니티 에디터 상에서 캐릭터의 애니메이션을 미리보는 기능이 추가됨
- 캐릭터에서 사용된 이미지들을 볼 수 있는 항목이 추가됨
- 애니메이션 이벤트 다이얼로그의 UI의 사용성이 개선됨
- 사용자가 이벤트를 프리셋으로 저장할 수 있는 기능이 추가됨
- 이벤트 마커의 색상을 지정할 수 있도록 변경됨
- 이벤트 마커가 타임라인에서 슬라이더보다 앞에서 렌더링되도록 변경됨
- Continuous 타입의 이벤트의 마커가 더 직관적으로 표현됨
- 동일한 Atlas 구조를 가진 보조 이미지인 "Secondary Atlas"를 만드는 기능이 "PSD 파일 다시 가져오기 기능"에 추가됨
- PSD 파일 저장시 경로 저장 관련 기능에 대한 안정성이 향상됨
- 에디터 실행 성능 개선됨
- 이전 버전에 도입된 버텍스를 클릭하는 개선된 방식이 모든 경우에 대해 적용됨
- Bake를 실행하면, 대상이 되는 GameObject가 포커스됨
- 애니메이션 타임라인의 "자동 스크롤" 기능이 개선됨
- Rigging 모디파이어에서 "Auto Normalize" 기능을 끌때 경고창이 등장함
- 텍스쳐나 객체, 애니메이션들을 참조하는 모든 스크립트 함수들의 성능이 개선됨
- 사용자가 "커맨드 버퍼"를 생성하여 캐릭터 렌더링을 제어하고자 할 때, 이를 보조하는 스크립트가 추가됨
- 편집 중이지 않은 컨트롤 파라미터들의 UI에서도 등록된 키들이 보여지도록 변경됨
- 모디파이어 UI에서 등록된 컨트롤 파라미터들이 출력될 때 사용자가 지정한 프리셋 아이콘과 함께 출력되도록 변경됨
- Rigging 모디파이어의 UI에서 상황에 따라 현재 사용 불가능한 UI를 직관적으로 구분할 수 있도록 개선됨
- 사용자가 클릭할 것으로 예상되는 버튼이 반짝거리도록 개선됨
- 모디파이어에서 컨트롤 파라미터의 키들을 한번에 모두 삭제하고 동시에 컨트롤 파라미터를 모디파이어에서 제거하는 버튼이 추가됨
- 최신 버전의 Mac OS에서 "Metal"이 필수 옵션으로 바뀌면서 더이상 Metal 관련 안내 다이얼로그가 나타나지 않도록 변경됨
- 이미지의 텍스쳐를 선택하는 다이얼로그와 메시의 이미지를 선택하는 다이얼로그의 가독성이 향상됨
- 애니메이션 타임라인에서 키프레임 등을 클릭하여 선택하는 처리가 개선됨
- 지글본의 가중치를 컨트롤 파라미터로 조절하는 옵션이 추가됨
- 본을 하나 혹은 여러개를 선택할 때 본 속성 UI의 변화가 최소화되도록 레이아웃 순서가 변경됨
- 메시 편집 UI에서 버텍스를 모두 삭제하는 버튼의 크기가 사용 빈도에 맞게 조금 더 커지도록 변경됨
- 컨트롤 파라미터로 제어하는 모디파이어의 "Reset Value" 및 "Paste" 기능을 사용할 때, 변경될 대상 속성을 선택하는 다이얼로그가 나타나도록 개선됨
- 메시를 Rigging 모디파이어에 등록 직후에 메시의 모든 버텍스들이 선택되도록 변경됨
- 씬에 존재하는 모든 apPortrait의 메시들을 갱신하는 기능이 유니티 에디터 메뉴의 "Refresh All Meshes" 메뉴로서 추가됨
- 메시 그룹의 Hierarchy에서 메시나 자식 메시 그룹 항목을 우클릭하면 해당 객체를 바로 수정할 수 있는 "Modify" 메뉴가 추가됨
- Morph 모디파이어 작업시, 메시를 선택하지 않은 상태에서 "Blur" 도구를 사용했을때 에러가 발생하는 문제가 수정됨
- Pro UI에서 일부의 아이콘 이미지들이 낮은 해상도로 표시되던 문제가 수정됨
- Transform 모디파이어 작업시, 2개 이상의 컨트롤 파라미터가 동일한 대상을 제어하는 상태에서 편집 모드를 켜면 해당 객체가 사라져버리는 문제가 수정됨
- Color Only 모디파이어를 선택하고 다중 편집 모드를 켜면 다른 모디파이어들이 지나치게 제한되는 문제가 수정됨
- 본 편집 모드에서 비활성화되어야 할 모디파이어가 동작하는 문제가 수정됨
- 모디파이어 작업시, 컨트롤 파라미터의 값이 키에 위치하지 않은 상태에서 다중 편집 모드를 켜면 편집 모드가 해제되는 문제가 수정됨
- 애니메이션 작업시, 다중 편집 모드가 다른 타임라인의 모디파이어들에게 제대로 적용되지 않는 문제가 수정됨
- Inspector를 통해서 에디터를 여는 경우, 간헐적으로 에디터 리소스가 로드되지 않은 문제가 수정됨
- 서로 다른 루트 유닛을 대상으로 하는 애니메이션들을 동일한 프레임에서 스크립트를 이용하여 재생하고자 하면 동작이 멈추는 문제가 수정됨
- 비동기 초기화 함수를 호출하면 제대로 초기화가 되지 않는 문제가 수정됨
- 에디터가 포커스되지 않은 상태에서 시간이 오래 지난 후, 애니메이션 재생이 프레임 설정에 맞지 않게 너무 빠르게 재생되는 문제가 수정됨
- 애니메이션 이벤트 실행 방식이 "Callback"일 때, 적절한 스크립트 함수를 지정하지 않은 상태에서 Bake를 실행하면 에러로 인식하여 로그가 출력되는 문제가 수정됨
- 유니티 2019 및 이후 버전에서 "보기 메뉴"를 열 때 컨트롤 파라미터 UI가 이상하게 출력되는 문제가 수정됨
- 컨트롤 파라미터 UI에서 카테고리를 변경한 직후에 컨트롤 파라미터를 클릭하여 선택할 수 없는 문제가 수정됨
- 재질 라이브러리에서 쉐이더 에셋을 변경한 직후 프리셋으로 등록하면, 프리셋에는 해당 에셋이 반영되지 않은 문제가 수정됨
- Rigging 모디파이어에서 "Pose Test"를 켠 상태에서 기즈모의 외부를 클릭했을 때 본을 다시 클릭하기 전까지 기즈모를 제어할 수 없는 문제가 수정됨
- 실행 취소 전후로 객체 생성시 내부적으로 ID 발급에 관한 오류가 매우 낮은 확률로 발생할 수 있었던 문제가 수정됨
- 객체를 생성하거나 복제할 때, 해당 객체가 바로 선택되는지 여부가 객체 종류에 따라 일관적이지 않았던 문제가 수정됨
- 작업 공간 외부에서 마우스 휠로 스크롤을 하고, 다시 작업 공간으로 커서를 이동하면 작업 공간이 확대/축소되는 문제가 수정됨
- 렌더링 대상이 "렌더 텍스쳐(Render Texture)"인 경우에 "Keep Alpha" 재질 프리셋의 쉐이더들이 이상하게 렌더링되는 문제가 수정됨 (다시 해당 패키지를 설치해야함)
- PSD 파일 임포트 다이얼로그에서, Atlas의 Bake 버튼을 다시 누른 직후에도 이전 Atlas가 화면에 계속 출력되는 문제가 수정됨
- "Type" 클래스에 대해서 명시적으로 작성되지 않아 에러가 발생하는 문제가 수정됨

1.4.1 (2022년 8월 3일)
- 애니메이션 이벤트 이름의 유효성을 검사하고 자동으로 수정해주는 기능이 추가됨
- Inspector UI에서 Callback 방식의 애니메이션 이벤트를 할당할 때, 에러를 야기하는 "오버로딩" 함수가 제외되도록 변경됨
- Inspector UI의 Callback 방식의 애니메이션 이벤트 UI에서 이벤트 리스너를 일괄 할당할 수 있는 기능이 추가됨
- Inspector UI의 Callback 방식의 애니메이션 이벤트 UI에서 할당된 이벤트들의 유효성을 검사할 수 있는 기능이 추가됨
- Inspector UI에서 컨트롤 파라미터의 이름을 클립보드로 복사하는 기능이 추가됨
- 루트 유닛을 선택한 상태에서 애니메이션의 자동 재생 여부를 설정하는 버튼의 한국어 표기가 수정됨
- 메시의 핀 편집 화면의 "Add Tool"을 선택한 상태에서, 작업 공간을 클릭하여 생성된 핀은 다시 클릭하기 전까지 이동할 수 없도록 변경됨
- 모디파이어에 컨트롤 파라미터를 추가하거나 삭제한 직후에 실행 취소를 했을 때, 경우에 따라 컨트롤 파라미터가 적절히 선택되도록 개선됨
- 여러개의 메시들을 자동 생성하는 다이얼로그에서 옵션을 일괄 변경할 수 있는 기능이 추가됨
- 애니메이션 이벤트를 복제할 때 마커 색상 속성이 복제되지 않는 문제가 수정됨
- "애니메이션 이벤트 삭제 버튼"이 누락되었던 문제가 수정됨
- 핀들을 연결하는 커브를 생성하거나 또는 커브를 삭제한 후 실행취소를 했을때 에러가 발생하는 문제가 수정됨
- 2개의 핀들을 2개의 커브로 연결할 수 있는 문제가 수정됨
- Inspector UI에서 프리팹 관련 UI의 버튼이 너무 작았던 문제가 수정됨
- 메시 그룹의 객체들의 렌더링 순서를 바꾸고 Setting 탭이 선택되지 않은 상태에서 실행 취소를 하면 렌더링 순서가 비정상적으로 연산되는 문제가 수정됨
- 컨트롤 파라미터, 리깅 UI 등에서 사용되는 삭제 버튼("X")이 Pro UI에서 잘 보이지 않는 문제가 수정됨

1.4.2 (2023년 1월 19일)
- Bake 관련 일부 옵션이 프로젝트 단위로 공유됨
- 에디터의 환경 설정을 파일로 내보내거나 가져올 수 있는 기능이 추가됨
- 사용자가 설정한 Bake 옵션을 "기본 설정값"으로 저장하는 기능이 추가됨
- 에디터 환경 설정 및 Bake 설정이 텍스트 파일 형식으로 저장되어 버전 관리 툴과 호환됨
- 마우스를 클릭하여 본을 생성할 때 "고스트 본(Ghost)"이 보여지도록 만드는 설정이 추가됨
- 작업 공간에서 보여지는 버텍스나 핀의 크기를 변경할 수 있는 설정이 추가됨
- "클릭 직후 드래그를 하여 대상을 바로 이동"하는 기존의 기즈모 동작 방식을 끌 수 있는 설정이 추가됨
- PSD 파일 다시 가져오기시, PSD의 레이어와 메시의 이름이 화면에 출력됨
- PSD 파일 다시 가져오기시, PSD의 이미지와 메시의 이미지의 위치를 정확히 비교하기 위해, 보정없이 픽셀값 그대로 볼 수 있는 기능이 추가됨
- PSD 파일 다시 가져오기를 수행한 경우, PSD 파일을 기준으로 추가되는 메시들의 렌더링 순서가 적절히 지정되도록 개선됨
- PSD 파일로부터 메시들을 생성할 때, 실행 취소를 위한 기록이 처리 단계별로 나누어지지 않도록 변경됨
- PSD 파일을 가져올 때 원본 레이어의 출력 여부가 메시의 기본값으로 반영되도록 개선됨
- PSD 파일을 가져올 때 경로가 유효하지 않은 상태에서 메시를 생성하고자 시도한다면 에러 메시지가 나타나며 동작이 멈추도록 변경됨
- Hierarchy UI에서 대상을 클릭한 후, 상하 방향키를 이용하여 다른 객체를 선택할 수 있도록 개선됨
- 작업 공간에서 객체를 선택하면 오른쪽의 Hierarchy UI가 자동으로 스크롤됨
- 애니메이션 클립을 선택할 때, 왼쪽의 Hierarchy UI가 자동으로 스크롤이 됨
- Hierarchy UI에서 우클릭을 하여 메뉴를 출력할 경우 클릭한 대상의 이름이 등장하며, 여러개의 대상이 동시에 선택된 경우엔 추가로 선택된 객체의 개수가 같이 표시됨
- Hierarchy UI에서 우클릭 메뉴의 "이름 변경(Rename)" 다이얼로그에서 Enter 키를 누르면 변경된 이름이 바로 적용됨
- 왼쪽의 Hierarchy UI의 우클릭 메뉴를 통해 객체의 순서를 변경하고자 할 때 정렬 모드를 "사용자 정의 방식"으로 변경할지 묻는 메시지가 등장함
- 우클릭 메뉴의 기능을 수행하면 해당 객체가 선택되도록 변경됨
- 메시 그룹의 Hierarchy UI에서 Alt 키를 누르고 본의 렌더링 여부를 변경하면 자식 본들의 출력 여부도 같이 변경됨
- 색상 옵션(Color Option)이 꺼진 경우에 작은 점이 대신 등장하며, 이를 클릭하면 색상 옵션이 바로 활성화됨
- 렌더링 순서(Depth)를 변경할 때 선택된 객체들의 렌더링 순서가 함께 변경됨
- Hierarchy UI의 우클릭 메뉴를 통해 복제할 때, 선택된 객체들이 동시에 모두 복제됨
- Hierarchy UI의 우클릭 메뉴를 통해 삭제할 때, 다중 삭제가 가능한 옵션이 제공됨
- 메시 그룹에서 여러개의 객체들을 삭제할 때, 서로 다른 타입인 자식 메시 그룹들과 메시들이 함께 선택된 상태에서도 다중 삭제가 가능하도록 개선됨
- "버텍스 초기화"를 실행할 때 Atlas 영역에 맞게 사각형 메시가 생성되도록 변경됨
- 선택한 핀들을 다른 메시로 복사하여 붙여넣는 기능이 추가됨
- 버텍스나 핀을 복사하여 붙여넣을 때, 기준점(Pivot)에 맞는 상대 좌표로 변환하여 붙여넣는 기능이 추가됨
- "Make Mesh" 탭이 선택된 상태에서 핀들이 반투명하게 보이도록 변경됨
- 메시의 탭을 전환하는 단축키 중 숫자 2 또는 숫자 7을 연속으로 누르면 하위 메뉴가 순서대로 변경되도록 개선됨
- 애니메이션 타임라인 UI의 확대/축소를 직관적으로 조절할 수 있도록 하단의 슬라이더가 기존과 반대로 동작하도록 변경됨
- Ctrl+Shift 키를 누른 상태로 타임라인 UI 상단을 클릭하고 드래그를 하면 타임 슬라이더가 계속 이동되도록 개선됨
- 레이어 다중 선택을 위한 "마지막으로 클릭한 타임라인 레이어"가 작업 공간이나 Hierarchy UI에서 대상을 클릭한 경우에도 갱신되도록 개선됨
- AnimPlayData에서 재생 시간 등을 가져오는 함수들이 추가됨
- 최신 버전을 알려주는 서버를 변경하여, 최신 버전을 조회하는 속도 및 안전성이 향상됨
- 선택된 대상의 종류에 따라 오른쪽 UI의 스크롤 값을 구분하여 사용성이 향상됨
- 새로운 Portrait의 이름을 설정하는 다이얼로그에서 Enter 키를 누르면 설정된 이름과 함께 Portrait가 바로 생성됨
- 화면 해상도가 FHD보다 크다면, 버텍스나 선분을 클릭하는 범위가 조금 더 커짐
- FFD 도구를 켠 상태에서 다른 작업을 할 때 나타나는 "FFD 작업을 완료할지를 묻는 메시지"가 더 많은 상황에서 나타나도록 개선됨
- Unity 2022.2부터 프리팹과 관련된 일부 API가 Deprecated됨에 따라 관련된 코드가 변경됨
- 리스트 UI의 "선택된 항목"의 하이라이트 디자인이 조금 더 예쁘게 보이도록 개선됨
- Rigging 모디파이어 편집 화면에서 단일 버텍스의 가중치를 변경하기 위해 숫자를 입력하는 UI가 동작하지 않는 문제가 수정됨
- Rigging 모디파이어의 편집 화면에서 본의 렌더링 방식이 "회색, 반투명" 등으로 변경된 상태에서 바로 다른 메시 그룹을 선택하면 본의 렌더링 방식이 원래대로 돌아오지 않는 문제가 수정됨
- 메시 그룹의 오른쪽 UI에서 "휴지통" 버튼에 의해 여러개의 객체가 동시에 삭제되지 않는 문제가 수정됨
- 자식 메시 그룹이 있는 경우 객체들의 렌더링 순서를 정상적으로 변경할 수 없는 문제가 수정됨
- PSD 파일 임포트시, 3레벨 이상의 자식 메시 그룹이 있는 경우 객체들의 순서가 역순으로 정렬되는 문제가 수정됨
- PSD 파일을 다시 로드할 때, 3레벨 이상의 자식 메시 그룹의 메시들이 추가로 생성되지 않는 문제가 수정됨
- 부모 본이 있는 상태로 Bake를 한 이후, 다시 부모 본과의 연결을 해제하고 Bake를 다시 했을 때 부모 본이 계속 연결된 것으로 인식되어 이상하게 움직이는 문제가 수정됨
- 오류로 인하여 모디파이어가 삭제된 이후에도 데이터가 존재하는 것으로 잘못 인식될 때 에디터를 실행할 수 없는 문제가 수정됨
- 메시 그룹을 삭제 했거나 유사한 작업 이후에 해당 메시 그룹과 연결되었던 애니메이션 클립에서 에러가 발생하면 해당 캐릭터를 에디터로 열 수 없게 되는 문제가 수정됨
- 루프 애니메이션에서 자동 루프 키프레임이 위치한 마지막 프레임에서는 애니메이션 이벤트가 호출되지 않는 문제가 수정됨
- 메시 그룹의 객체들을 동시에 여러개 삭제한 이후 실행 취소를 할 경우 순서가 비정상적으로 정렬되는 문제가 수정됨
- Shift로 자식 메시 그룹의 메시들을 삭제한 경우 에러 로그가 발생하는 문제가 수정됨
- PSD 다시 열기 다이얼로그에서 매핑 대상인 메시가 검은색으로 렌더링되는 문제가 수정됨
- Secondary Atlas를 가져오는 다이얼로그에서 위치를 보정하는 단계로 처음 진입할 때 레이어 정보와 미리보기가 제대로 동작하지 않는 문제가 수정됨
- 메시나 메시 그룹을 선택하지 않은 상태에서 에디터 왼쪽의 Hierarchy UI의 우클릭 메뉴를 통해 대상의 객체를 복제(Duplicate)할 수 없는 문제가 수정됨
- 메시 그룹의 Setting 탭이 선택된 상태에서 자식 메시 그룹을 선택할 경우, 오른쪽 UI의 일부 항목이 이상하게 출력되는 문제가 수정됨
- 게임 실행시 물리 효과가 실행되는 첫 프레임에서 초기값이 잘못 적용되어 버텍스나 본이 이상하게 움직이는 문제가 수정됨
- 게임 실행시 FPS가 지나치게 낮거나 어플리케이션이 중지된 경우 물리 효과가 지나치게 과장되어 움직이는 문제가 수정됨
- 메시를 구성하는 선분이 수평선이나 수직선일 때 마우스로 클릭하기 어려웠던 문제가 수정됨
- 자식 메시 그룹에 속한 Rigging 모디파이어가 적용된 메시들은 Transform 모디파이어의 대상이 될 수 없는데, 이를 Hierarchy UI의 "노란색 눈 버튼"을 통해 우회하여 등록할 수 있었던 문제가 수정됨
- 지글 본(Jiggle Bone)의 제한 영역(Constraint) 설정이 "부모 메시 그룹이 회전된 상태에서 대상이 루트 본이 아닌 경우"에 작업 공간에서 이상하게 보여지는 문제가 수정됨
- FFD 도구가 켜진 상태에서 "Ctrl+A" 단축키를 누른 경우 컨트롤 포인트가 아닌 버텍스가 선택되는 문제가 수정됨
- 애니메이션에 "Morph (Animation)" 모디파이어를 타임라인으로서 추가하고, 모든 메시를 타임라인 레이어로서 등록하고자 할 때, 대상이 아닌 자식 메시 그룹이 등록이 되고 반대로 대상이 되는 자식 메시 그룹의 메시들이 등록이 되지 않는 문제가 수정됨
- 버그 등에 의해서 타임라인의 대상이 아닌 객체가 타임라인 레이어로 등록된 경우에, 이를 타임라인으로부터 삭제하는 버튼이 나타나지 않는 문제가 수정됨
- 애니메이션 편집 화면에서 마우스 드래그시에 객체 선택이 해제되는 문제가 수정됨

1.4.3 (2023년 3월 7일)
- 임의의 재질 프로퍼티를 제어하는 SetMeshCustomImage 등과 같은 스크립트의 함수의 인자로 Int 타입의 프로퍼티 ID를 입력할 수 있음
- 애니메이션 편집 화면에서 키프레임이 없는 프레임에서 객체 선택 후, 타임 슬라이더를 옮겨 키프레임을 선택했을 때 기즈모가 비정상적으로 동작하는 문제가 수정됨
- 키프레임의 회전 각도 보정 옵션이 "최적화된 내보내기(Optimized Bake)"에 적용되지 않는 문제가 수정됨
- 유니티 씬에서 실행할 때, 회전 각도 보정 옵션이 있는 키프레임이 반복적으로 재생되면 회전 각도 보정 옵션이 설정되지 않은 구간에서 비정상적으로 객체가 회전하는 문제가 수정됨
- 애니메이션을 추가하고 Bake를 다시 한 후, 인스펙터 UI에서 애니메이션 미리보기시 에러가 발생하는 문제가 수정됨
- 2개 이상의 공통 키프레임들을 이동하여 다른 키프레임들을 덮어씌울 때 타임라인 UI에서 애니메이션 커브에 대한 정보가 정상적으로 갱신되지 않는 문제가 수정됨