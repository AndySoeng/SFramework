**************************************
*          HIGHLIGHT PLUS            *
* Created by Ramiro Oliva (Kronnect) * 
*            README FILE             *
**************************************


Notice about Universal Rendering Pipeline
-----------------------------------------
This package is designed for URP.
It requires Unity 2019.3 and URP 7.1.6 or later
To install the plugin correctly:

1) Make sure you have Universal Rendering Pipeline asset installed (from Package Manager).
2) Go to Project Settings / Graphics.
3) Double click the Universal Rendering Pipeline asset.
4) Double click the Forward Renderer asset.
5) Click "+" to add the Highlight Plus Renderer Feature to the list of the Forward Renderer Features.

Note: URP assets can be assigned to Settings / Graphics and also Settings / Quality. Check both sections!

You can also find a HighlightPlusForwardRenderer asset in the Highlight Plus / Pipelines / URP folder.
Make sure the Highlight Plus Scriptable Renderer Feature is listed in the Renderer Features of the  Forward Renderer in the pipeline asset.

Video instructions: https://youtu.be/EgyBs8v9aRI


Quick help: how to use this asset?
----------------------------------

1) Highlighting specific objects: add HighlightEffect.cs script to any GameObject. Customize the appearance options.
  In the Highlight Effect inspector, you can specify which objects, in addition to this one, are also affected by the effect:
    a) Only this object
    b) This object and its children
    c) All objects from the root to the children
    d) All objects belonging to a layer

2) Control highlight effect when mouse is over:
  Add HighlightTrigger.cs script to the GameObject. It will activate highlight on the gameobject when mouse pass over it.

3) Highlighting any object in the scene:
  Select top menu GameObject -> Effects -> Highlight Plus -> Create Manager.
  Customize appearance and behaviour of Highlight Manager. Those settings are default settings for all objects. If you want different settings for certain objects just add another HighlightEffect script to each different object. The manager will use those settings.

4) Make transparent shaders compatible with See-Through effect:
  If you want the See-Through effect be seen through other transparent objects, they need to be modified so they write to depth buffer (by default transparent objects do not write to z-buffer).
  To do so, select top menu GameObject -> Effects -> Highlight Plus -> Add Depth To Transparent Object.

5) Static batching:
  Objects marked as "static" need a MeshCollider in order to be highlighted. This is because Unity combines the meshes of static objects so it's not possible to highlight individual objects if their meshes are combined.
  To allow highlighting static objects make sure they have a MeshCollider attached (the MeshCollider can be disabled).




Help & Support Forum
--------------------

Check the Documentation (PDF) for detailed instructions:

Have any question or issue?
* Support Forum: https://kronnect.com/support
* Twitter: @Kronnect
* Email (non support): contact@kronnect.com

If you like Highlight Plus, please rate it on the Asset Store. It encourages us to keep improving it! Thanks!




Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of Highlight Plus will be eventually available on the Asset Store.



More Cool Assets!
-----------------
Check out our other assets here:
https://assetstore.unity.com/publishers/15018



Version history
---------------

Current version
- Added "Camera Distance Fade" option
- Improved see-through accurate method which now takes into account multi-part meshes from compound parents
- [Fix] Fixed glow/outline aspect ratio in Single Pass Instanced VR mode

Version 8.5
- Improved outline effect when combining "Independent" option with many elements in "Children" selection
- Improved see-through border only effect

Version 8.4.1
- [Fix] Fixed unnecessary memory allocation in the render feature

Version 8.4
- Added "Border Only" option to See-Through effect
- Adding a Highlight Effect component to a parent no longer deactivates highlighted children

Version 8.3
- Upgraded to Unity 2020.3.16 as minimum

Version 8.2
- Added "Ignore Mask" option to glow. Can be used to render the glow effect alone
- [Fix] Fixed issue with new input system and highlight manager/trigger if no Event System is present in the scene
- [Fix] Fixed glow passes UI overlap in Unity 2021.3.3 due to reorderable array bug

Version 8.1
- Selection state is now visible in inspector (used only by trigger and manager components)
- [Fix] Fixed outer glow not showing in higher quality with visibility set to normal and orthographic camera
- [Fix] Fixed mobile input using the new input system
- [Fix] Fixed outline settings mismatch when using a combination of Highlight Trigger and Manager

Version 8.0
- Added SelectObject / ToggleObject / UnselectObject methods to Highlight Manager
- Added ability to apply custom sorting to effects (check documentation: Custom sorting section)
- Independent option is now moved to Highlight Options section and affects both outline and glow
- Added "Clear Stencil" option to Highlight Plus Render Feature (useful to solve compatibility with other assets that use stencil buffers)

Version 7.9.2
- [Fix] Fixed an issue in Unity 2021.2 when using MSAA and High Quality outline/glow

Version 7.9.1
- Default values for all effects are now 0 (disabled) except outline so desired effects must be enabled. This option allows you to ensure no extra/undesired effects are activated by mistake
- Redesigned Highlight Plus Profile editor interface
- Removed dependency of HighlightManager

Version 7.8
- Added outer glow blend mode option
- API: added OnObjectHighlightStart/End events to HighlightTrigger (check documentation for differences with similar events on Highlight Effect main script)
- [Fix] API: Fixed specific issues with SetTarget method when used on shader graph based materials that don't use standard texture names

Version 7.7.2
- [Fix] Fixed fade in/out issue when disabling/enabling objects

Version 7.7
- Added support for the new Input System
- [Fix] Fixes to the align to ground option of target fx effect

Version 7.6.2
- [Fix] VR: fixed target effect "Align to Ground" issue with Single Pass Instanced

Version 7.6.1
- [Fix] Fixed overlay animation speed issue

Version 7.6
- Added "Target FX Align to Ground" option
- Added isSeeThroughOccluded(camera). Is true when any see-through occluder using raycast mode is blocking the see-through effect
- All shader keywords are now of local type reducing global keyword usage
- Fixes and improvements to see-through when combined with outline/outer glow

Version 7.5.2
- [Fix] See-through is now visible when using glow/outline/inner glow with Always Visible option

Version 7.5.1
- [Fix] Fixed regression bug with Outline in High Quality mode

Version 7.5
- Added new HitFX style: "Local Hit"
- Added new demo scene showcasing the HitFx variations
- Added "Overlay Texture" option
- Added "Min Distance" option to Highlight Manager and Highlight Trigger
- Added support for "Domain Reload" disabled option
- API: added OnObjectHighlightStart, OnObjectHighlightEnd events to HighlightManager
- [Fix] Fixed inner glow and overlay issue when MaterialPropertyBlock is used on the character material

Version 7.1
- Added "Respect UI" to Highlight Manager and Trigger which blocks interaction if pointer is over an UI element

Version 7.0.2
- Memory optimizations

Version 7.0
- Added support for Single Pass Instanced
- Internal improvements and fixes

Version 6.9
- Added "Ordered" option to see-through
- Removed "Non Overlap" option from see-through as now it pervents overdraw by default

Version 6.8
- Improvements to see-through rendering order
- [Fix] Fixed properties not being reflected in scene immediately when invoking Undo

Version 6.7
- Added "SeeThrough Max Depth" option. Limits the visibility of the see-through effect to certain distance from the occluders
- Added "SeeThrough Check Individual Objects" option. If enabled, occlusion test is performed for each individual child of the object, instead of using combined bounds

Version 6.6
- Added "SeeThrough Depth Offset" option. This option allows you to control the minimum distance from the occluder to the object before showing the see-through effect
- Added "SeeThrough Non Overlap" option. Enable it only if the see-through effect produces flickering due to overlapping geometry in the hidden object
- [Fix] Fixed properties not being reflected in scene immediately when invoking Undo

Version 6.5.2
- Added inspector tooltips and improved documentation

Version 6.5.1
- Calling ProfileLoad() method will now assign that profile to the highlight effect component in addition to loading its values
- Prevents _Time overflow which can cause glitching on some Android devices

Version 6.5
- Name filter now is ignored when effect group is set to Only This Object
- New shader "HighlightPlus/Geometry/UIMask" to cancel highlight effects when rendering through a UI Canvas (see documentation)

Version 6.4
- Added "Cameras Layer Mask" to specify which cameras can render the effects
- Hit FX color in Highlight Profile now exposes HDR color options

Version 6.3.1
- Added "Single Selection" option to Highlight Manager/Trigger
- Added "Toggle" option to Highlight Manager/Trigger
- Selection is cleared now when clicking anywhere in the scene (requires Highlight Manager)
- API: added SetGlowColor
- Improved Highlight Manager inspector

Version 6.2
- Added TargetFX Scale To Object Bounds (defaults to false)
- Added support for HDR color to Hit FX color field
- Option to list occluders in the inspector when See Through Occluder Mask "Accurate" option is enabled

Version 6.1
- Added more accurate occluder layer system ("Accurate" option)
- Added default hit fx settings to inspector & profile
- Added hit fx modes (overlay or inner glow)

Version 6.0
- Added Selection feature
- Inspector: sections can be now collapsed to reduce screen space
- API: added OnObjectSelected / OnObjectUnSelected events

Version 5.5 4/Apr/2021
- [Fix] Fixed glow overlay when MSAA is disabled on Windows

Version 5.4 5/Feb/2021
- Added Visibility option to targete effect
- Stencil mask is no longer computed when only overlay or inner glow is used improving performance

Version 5.3.4 22/01/2021
- Optimizations to material setters
- [Fix] Fixed outline color issue with quality level set to medium

Version 5.3.3
- Effects now reflect object transform changes when combines meshes option is enabled

Version 5.3
- Added "Combine Meshes" option to profile
- Optimizations and fixes

Version 5.2
- Added "Object Name Filter" option to profile

Version 5.1
- Added "Border When Hidden" effect (outline when see-through triggers)

Version 5.0.1
- Added support for Unity 2020.2 beta

Version 5.0
- API: added "TargetFX" method to programmatically start the target effect  
- Added support for double-sided shader effects

Version 4.9
- Added "Medium" quality level

Version 4.8.2
- [Fix] Fixed issue with outline set to fastest and glow using highest in latest URP version

Version 4.8.1
- [Fix] Fixed issue with outline/glow when overlay cameras are present on the stack

Version 4.8
- Added "Outer Glow Blend Passes" option
- [Fix] Fixed outline & glow issue with alpha cutout when using non-highest quality mode

Version 4.7
- Added "Normals Option" with Smooth, Preserve and Reorient variants to improve results
- Target effect now only renders once per gameobject if a specific target transform is specified
- API: added OnTargetAnimates. Allows you to override center, rotation and scale of target effect on a per-frame basis.

Version 4.6
- Added "SubMesh Mask" which allows to exclude certain submeshes
- [Fix] Fixed shader compilation issue with Single Pass Instanced mode enabled

Version 4.4
- Exposed "Smooth Normals" option in inspector.

Version 4.3.2
- Added HitFX effect
- Improvements to SeeThrough Occluder when Detection Mode is set to RayCast

Version 4.3.1
- [Fix] Fixed issue with Highlight Effect Occluder script

Version 4.3
- Added GPU instancing support for outline / outer glow effects

Version 4.2.2
- [Fix] Fixed effect being rendered when object is outside of frustum camera

Version 4.2.1
- Profile: added "Constant Width" property
- Enabled HDR color picker to Color properties
- [Fix] Fixed missing outline with flat surfaces like quads under certain angles

Version 4.2
- Glow/Outline downsampling option added to profiles
- [Fix] Removed VR API usage console warning

Version 4.1
- Added Outline Independent option
- [Fix] Fixed error when highlight script is added to an empty gameobject

Version 4.0
- Support for URP Scriptable Rendering Feature
