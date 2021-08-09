This scene has a specific folder structure that should hold all derivative elements.
The following objects are only folder objects and must never have any components or non-default transforms

[RENDERING] - Lighting, Cameras, and Camera Effects
[AR_MANAGER] - ML Permission Requestor, Event subscriptions, Spatial Mapper, Transmission, etc.
[SCENE_MANAGER] - Game logic, global state control and flow, scene setup
[WORLD] - All elements of the game environment besides those created dynamically or if they fit in other folders
[GUI] - All canvases and menus
[_DYNAMIC] - Holds all objects instantiated at runtime. Make sure MLSpatialMapper has this object set as the mesh parent