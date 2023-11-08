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
        
        [ContextMenu("Set Attribute")]
        private void AddCustomField()
        {
            metalitixLogger.SetAttribute("custom", 10000);
        }

        [ContextMenu("Remove Attribute")]
        private void RemoveCustomField()
        {
            metalitixLogger.RemoveAttribute("custom");
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                metalitixLogger.LogEvent("example", "example");
            }
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                metalitixLogger.LogState("example", 1);
            }
        }
    }
}