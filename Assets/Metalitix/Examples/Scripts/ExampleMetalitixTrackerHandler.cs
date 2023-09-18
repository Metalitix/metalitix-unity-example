using Metalitix.Scripts.Logger.Core.Base;
using UnityEngine;

namespace Metalitix.Examples.Scripts
{
    public class ExampleMetalitixTrackerHandler : MonoBehaviour
    {
        //Assign metalitix tracker here
        [SerializeField] private MetalitixLogger metalitixLogger;

        //Initialization
        private void Start()
        {
            //Handle user`s afk state
            metalitixLogger.OnAwayFromKeyboard += AfkHandle;
            
            if (Camera.main != null)
            {
                //The SetData method must be called at least once and before calling StartSession
                metalitixLogger.Initialize();
            }
        }

        private void AfkHandle()
        {
            Debug.Log("User is afk");
        }


        [ContextMenu("ChangeAPIkey")]
        private void ChangeApiKey()
        {
            string apiKey = "99671e0f-11e2-444c-b685-36445c6685b9";
            metalitixLogger.ChangeAPIKey(apiKey);
        }
        

        [ContextMenu("Start")]
        public void StartSession()
        {
            metalitixLogger.StartSession();
        }
        
        [ContextMenu("End")]
        public void EndSession()
        {
            metalitixLogger.EndSession();
        }
        
        [ContextMenu("Pause")]
        public void PauseSession()
        {
            metalitixLogger.PauseSession();
        }
        
        [ContextMenu("Resume")]
        public void ResumeSession()
        {
            metalitixLogger.ResumeSession();
        }
        
        [ContextMenu("Update")]
        private void UpdateSession()
        {
            metalitixLogger.UpdateSession();
        }
        
        [ContextMenu("Set custom field")]
        private void AddCustomField()
        {
            metalitixLogger.SetCustomField("custom", 10000);
        }

        [ContextMenu("Remove custom field")]
        private void RemoveCustomField()
        {
            metalitixLogger.RemoveCustomField("custom");
        }

        [ContextMenu("Set pollInterval")]
        private void SetPollInterval()
        {
            metalitixLogger.SetPollInterval(0.2f);
        }
        
        [ContextMenu("Show survey")]
        public void ShowSurvey()
        {
            metalitixLogger.ShowSurveyPopUp();
        }

        /// <summary>
        /// An example of using event logging
        /// </summary>
        private void Update()
        {
            if (metalitixLogger.IsRunning)
            {
                //Sending a Metalitix KnowType event with `MetalitixInteractionType`
                if (Input.GetMouseButtonDown(0))
                {
                    metalitixLogger.EventHandler.LogMouseDownEvent(Input.mousePosition.x, Input.mousePosition.y);
                }
            
                //Sending a Metalitix KnowType event with `MetalitixInteractionType` and custom fields
                if (Input.GetMouseButtonDown(1))
                {
                    metalitixLogger.EventHandler.LogMouseDownEvent(Input.mousePosition.x, Input.mousePosition.y, ("customEventField", "customValue"));
                }

                //Sending a user custom event
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    metalitixLogger.EventHandler.LogCustomEvent("SpaceDown", ("customEventBool", true), ("ButtonDown", "space.click"));
                }
            
                //Sending a user custom event
                if (Input.GetKeyDown(KeyCode.Break))
                {
                    metalitixLogger.EventHandler.LogCustomEvent("BreakDown", ("BreakDown", "break.click"));
                }
                
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    metalitixLogger.EventHandler.LogCustomEvent("ReturnDown", ("BoolTest", true));
                }
            }
        }
    }
}