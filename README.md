# metalitix-unity-example
Unity-based example project showing a Metalitix® integration. Unlock spatial analytics for your 3D scene. Learn more at https://www.metalitix.com/

## Official Links
- [Platform Breakdown](https://docs.metalitix.com/v/unity/)
- [Supported Platforms](https://docs.metalitix.com/v/unity/supported-platforms)
- [Getting Started](https://docs.metalitix.com/v/unity/getting-started)
- [Advanced Features](https://docs.metalitix.com/v/unity/advanced-features)

## Project setup
1. Clone this repository or download it as a zip file to your computer.
2. Open the project within Unity.
3. The project may need to be closed and re-opened a second time to initialize the Metalitix package. Metalitix will try to fix references to package files, though Unity oftentimes loses references anyway. If problems persist, you can download the latest version of the Metalitix package [here](https://cdn.metalitix.com/logger/unity/latest/metalitix.unitypackage) and import it by navigating to `Assets -> Import Package` in the menu bar.
4. In your project's settings on the [Metalitix dashboard](https://app.metalitix.com), whitelist your experience's domain. For Unity projects on your local machine, this is `4http://localhost`.
5. From your project's settings on the [Metalitix dashboard](https://app.metalitix.com), copy your API key. Then in your Unity project, navigate to to `Metalitix -> Logger` in the menu bar and paste your API key in the `API Key` field.

## Running the project
When you run this example project, it will not automatically record to Metalitix. Instead, it is set up so that UI elements, when clicked, manually start, end, pause, and send metrics. This allows developers to explore the various Metalitix functions in greater depth.
1. Press Play on the main Unity UI.  
2. While the project is running, find and select the `LoggerHandler` object in the hierarchy window (typically on the left of the window).
3. In the Inspector window of object `LoggerHandler` find script `ExampleMetalitixTrackerHandler` (typically on the right of the window).
4. Right-click on this script or the three vertical dots above it. The dropdown menu will show Metalitix options (Start, End, Pause, Resume, Update, SetCustomField, RemoveCustomField, SetPollInterval, ShowSurvey). To run Metalitix, click the `Start` option.
5. Metalitix is now logging. You can view your session data on the Metalitix website. Please explore the other dropdown menu options and its source code!
