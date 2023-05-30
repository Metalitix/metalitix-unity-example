# metalitix-unity-example
Unity-based example project showing a Metalitix® integration. Unlock spatial analytics for your 3D scene. Learn more at https://www.metalitix.com/

## Project setup
> 1. Clone this repository or download as zip file to your computer.
> 2. Open the project within Unity.
> 3. The project may need to be closed and re-opened a second time to initialize the Metalitix package. If a `Metalitix` option appears in the menu bar, then Metalitix has successfully installed. Alternatively, you can download the latest version of the Metalitix package [here](https://cdn.metalitix.com/logger/unity/latest/metalitix.unitypackage) and import it by navigating to `Assets -> Import Package` in the menu bar.

## Run the example
> 1. In Unity, add your Metalitix project's APP Key by navigating to `Metalitix -> Logger` in the menu bar.
> 2. Press Play on the main Unity UI.  
> 3. While the project is running, find `LoggerHandler` object in the hierarchy window.
> 4. In the Inspector window of object `LoggerHandler` find script `ExampleMetalitixTrackerHandler`.
> 5. Right-click on this script or the three vertical dots above it. The dropdown menu will show Metalitix options (Start, End, Pause, Resume, Update, SetCustomField, RemoveCustomField, SetPollInterval, ShowSurvey). To run Metalitix, click the `Start` option.
> 6. Metalitix is now logging. You can view your session data on the Metalitix website.
