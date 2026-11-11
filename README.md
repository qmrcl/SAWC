# SAWC

Simple Advanced Walking Controller is a Unity 6 package for first-person, third-person, and mobile character controls.

## Install

In **Window > Package Manager**, select **+ > Install package from git URL** and enter `https://github.com/qmrcl/SAWC.git`. To develop against a local checkout, choose **Install package from disk** and select the checkout's `package.json`. Keep the checkout outside the Unity project's `Assets` folder.

## Import the demo

Open SAWC in Package Manager, select **Samples**, and click **Import** next to **Starter Assets**. Open `Scenes/Player.unity` in the imported sample under `Assets/Samples/SAWC/<version>/Starter Assets`.

Use a project with Universal Render Pipeline configured and **Active Input Handling** set to **Input System Package (New)** or **Both** to run the demo. The sample includes the player prefabs, a Character Settings preset, sounds, and mobile UI textures.

For your own setup, add a **SAW Controller** and an input provider. The controller uses built-in character settings until you assign a preset. To create an editable preset, choose **Assets > Create > SAWC > Core > Character Settings**.

## Edit Starter Assets in the development project

Edit the imported **Starter Assets** under `Assets/Samples` in Unity and save your scenes and prefabs. In the SAWC development project, choose **Tools > SAWC > Publish Starter Assets to Local Package** to copy the saved sample and its metadata back into `Samples~/StarterAssets`. The command backs up the previous package sample in `Library/SAWCStarterAssetsBackups`.

Keep a single imported copy under `Assets`. A symlink to the package sample can expose the same GUIDs twice and break prefab references when Unity assigns new ones.

## Run the package tests

In the test project's `Packages/manifest.json`, include `com.qmrcl.sawc` and `com.unity.inputsystem` in `testables`. Run the Edit Mode tests in Unity Test Runner. Import **Starter Assets** to run the tests that check sample prefabs.

[Project documentation](https://qmrcl.github.io/SAWC-docs/)
