# Ultra Bombers

Based off the original [BombersRTT](https://github.com/getbraincloud/examples-unity/tree/master/BombersRTT) example project. Be sure to check out its **README.md** for more information.

---

### Ultra Auth Plugin Package

The **Ultra Auth Plugin package** included in this example app via the Package Manager is not meant for redistribution and cannot be used in a commercial platform. Please download the **Ultra Auth Plugin** from its official distribution platform and follow the official instructions to add it to your Unity app.

## Building

1. Configure `brainCloud > Settings` to the correct Team and App. Your **Server URL** when logging in may not be on the **Default brainCloud Server** so be sure to use the correct URL if that is the case.

2. Configure `Ultra > Settings` to use the correct **Authentication Url** and **Client Id**.

You will not be able to log into the app until both **BrainCloudSettings** and **UltraSettings** are configured properly. Please read the documentation for the [brainCloud Unity/Csharp plugin](https://github.com/getbraincloud/braincloud-csharp) and the Ultra Auth Plugin on their official distribution platforms for more in-depth instructions.

All other **Player Settings** in Unity's **Build Settings** are set to what is recommended for this example app. **Windows** and **MacOS** are currently the only platforms supported.

## Adding New Plane Skins

Plane skins must be added to the client build before it can become available as an NFT blockchain item for users in-app.

Plane skins are dynamically loaded from `Resources > PlaneData` as **PlaneScriptableObjects**. You can create a new one in the **Create** menu by selecting `Create > ScriptableObjects > Plane Data`. Be sure the new **PlaneScriptableObject** asset is located under `Resources > PlaneData` to be loaded properly during runtime.

### PlaneData Fields

| Field | Description |
| ------------------------------ | --- |
| int planeID | This should match the **token_factory_id** of the NFT blockchain item and should be a positive number. Do not use **0** as it is reserved for the Default plane skin. |
| GameObject planeModel_green | The prefab that will be loaded for the player when they're on the Green Team. This prefab should be located under `Resources > Prefabs > Game`. |
| GameObject planeModel_red | The prefab that will be loaded for the player when they're on the Red Team. This prefab should be located under `Resources > Prefabs > Game`. |
| string planeName | The name of this plane skin. |
| string planeDescription | The description of this plane skin. |
| Sprite planeThumbnail_green | The thumbnail that will be used when the player is on the Green Team. |
| Sprite planeThumbnail_red | The thumbnail that will be used when the player is on the Red Team. |

Please see **00_Default**, or other included **PlaneScriptableObjects**, as an example of how your custom plane skin should be set up. For the green & red prefab plane GameObjects, make sure your custom prefabs are set up the same way as all of the example prefabs.
