using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading;

namespace GIF_Viewer
{
    /// <summary>
    /// Use this class to check for updates online
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Enumerator for the possible current update statuses
        /// </summary>
        public enum UpdateStatus
        {
            /// <summary>
            /// No update is available
            /// </summary>
            NoUpdateAvailable,
            /// <summary>
            /// An update is avaliable
            /// </summary>
            UpdateAvailable,
            /// <summary>
            /// An update is required to be downloaded
            /// </summary>
            UpdateRequired,
            /// <summary>
            /// This application was not deployed via ClickOnce and cannot be updated
            /// </summary>
            NotDeployedViaClickOnce,
            /// <summary>
            /// There was an error downloading the deployment
            /// </summary>
            DeploymentDownloadException,
            /// <summary>
            /// The deployment that was downloaded is invalid or corrupted
            /// </summary>
            InvalidDeploymentException,
            /// <summary>
            /// General operation exception
            /// </summary>
            InvalidOperationException
        }

        /// <summary>
        /// Enumerator for the possible update over statuses
        /// </summary>
        public enum UpdateOverStatus
        {
            /// <summary>
            /// The update was successful
            /// </summary>
            UpdateSuccessful,
            /// <summary>
            /// There was an error during the update
            /// </summary>
            UpdateError
        }

        /// <summary>
        /// The background worker used to unload the update query from the main process
        /// </summary>
        BackgroundWorker UpdateWorker;

        /// <summary>
        /// Delegate for the UpdateAvailableEvent
        /// </summary>
        /// <param name="status">The update status</param>
        public delegate void UpdateAvaibleEventArgs(UpdateStatus status);
        /// <summary>
        /// The event fired when an update is available
        /// </summary>
        public event UpdateAvaibleEventArgs UpdateAvaibleEvent;

        /// <summary>
        /// Delegate for the UpdateOverEvent
        /// </summary>
        /// <param name="status">The update status</param>
        public delegate void UpdateOverEventArgs(UpdateOverStatus status);
        /// <summary>
        /// The event fired when an update has ended
        /// </summary>
        public event UpdateOverEventArgs UpdateOverEvent;

        /// <summary>
        /// Checks for any avaible update for this software
        /// </summary>
        public void PerformUpdate()
        {
            UpdateWorker = new BackgroundWorker();

            UpdateWorker.DoWork += new DoWorkEventHandler(UpdateWorker_DoWork);
            UpdateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateWorker_RunWorkerCompleted);
        }

        /// <summary>
        /// Starts the update process for this application
        /// </summary>
        private void UpdateApplication()
        {
            try
            {
                ApplicationDeployment updateCheck = ApplicationDeployment.CurrentDeployment;
                updateCheck.Update();

                if (UpdateOverEvent != null)
                    UpdateOverEvent.Invoke(UpdateOverStatus.UpdateSuccessful);
            }
            catch (DeploymentDownloadException)
            {
                if (UpdateOverEvent != null)
                    UpdateOverEvent.Invoke(UpdateOverStatus.UpdateError);

                return;
            }
        }

        /// <summary>
        /// Event fired by the background worker and is used to check for updates
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        void UpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateCheckInfo info = null;

            // Check if the application was deployed via ClickOnce.
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                e.Result = UpdateStatus.NotDeployedViaClickOnce;
                return;
            }

            ApplicationDeployment updateCheck = ApplicationDeployment.CurrentDeployment;

            try
            {
                info = updateCheck.CheckForDetailedUpdate();
            }
            catch (DeploymentDownloadException)
            {
                e.Result = UpdateStatus.DeploymentDownloadException;
                return;
            }
            catch (InvalidDeploymentException)
            {
                e.Result = UpdateStatus.InvalidDeploymentException;
                return;
            }
            catch (InvalidOperationException)
            {
                e.Result = UpdateStatus.InvalidOperationException;
                return;
            }

            if (info.UpdateAvailable)
                e.Result = info.IsUpdateRequired ? UpdateStatus.UpdateRequired : UpdateStatus.UpdateAvailable;
            else
                e.Result = UpdateStatus.NoUpdateAvailable;
        }

        /// <summary>
        /// Event fired once the update worked has completed its run and has a result for the update query
        /// </summary>
        /// <param name="sender">Object that fired this event</param>
        /// <param name="e">The arguments for this event</param>
        void UpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (UpdateAvaibleEvent != null)
            {
                UpdateAvaibleEvent.Invoke((UpdateStatus)e.Result);
            }
        }
    }
}