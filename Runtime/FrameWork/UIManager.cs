using MonoUtility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIFrameWork
{
    // UIManager does not directly use EventSystem or StandaloneInputModule. However in our convention, we put UIManager, EventSystem and StandaloneInputModule
    // in the same game object in a scene.
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]
    public class UIManager : Singleton<UIManager>
    {
        private const int BaseSortOrder = 20000;

        struct ExclusiveData
        {
            public UI _UI;
            public int _PreviousSortingOrder;
            public bool _PreviousOverrideSorting;
        }

        public bool _UpdatePixelDragThresold = true;
        public float _ReferenceDPI = 115;// For 5 as reference pixel drag threshold in EventSystem

        private List<ExclusiveData> mExclusiveList = new List<ExclusiveData>();
        private UIWidget mGlobalMouseOverItem = null;

        public UIWidget pGlobalMouseOverItem
        {
            get
            {
                return (mGlobalMouseOverItem != null && mGlobalMouseOverItem.isActiveAndEnabled && mGlobalMouseOverItem.pInteractableInHierarchy) ? mGlobalMouseOverItem : null;
            }

            set { mGlobalMouseOverItem = value; }
        }

        protected override void Start()
        {
            base.Start();
            UpdateThreshold();
        }

        private void UpdateThreshold()
        {
            if (_UpdatePixelDragThresold && EventSystem.current != null)
            {
                _UpdatePixelDragThresold = false;
                EventSystem.current.pixelDragThreshold = (int)(EventSystem.current.pixelDragThreshold / _ReferenceDPI * Screen.dpi);
            }
        }

        /// <summary>
        /// This method must be called only from UI.SetExclusive(). Do not call this directly
        /// </summary>
        public void AddToExclusiveListOnTop(UI ui)
        {
            int highestSortOrder = BaseSortOrder;
            if (mExclusiveList.Count > 0)
                highestSortOrder = mExclusiveList[mExclusiveList.Count - 1]._UI.GetComponent<Canvas>().sortingOrder;

            Canvas uiCanvas = ui.GetComponent<Canvas>();
            ExclusiveData exclusiveData = new ExclusiveData();
            int existingExclusiveDataIndex = mExclusiveList.FindIndex(x => x._UI == ui);
            if (existingExclusiveDataIndex == -1)
                exclusiveData = new ExclusiveData { _UI = ui, _PreviousSortingOrder = uiCanvas.sortingOrder, _PreviousOverrideSorting = uiCanvas.overrideSorting };
            else
            {
                exclusiveData = mExclusiveList[existingExclusiveDataIndex];
                mExclusiveList.RemoveAt(existingExclusiveDataIndex);
            }
            mExclusiveList.Add(exclusiveData);
            uiCanvas.overrideSorting = true;
            uiCanvas.sortingOrder = highestSortOrder + 1;
            UI._GlobalExclusiveUI = ui;
        }

        /// <summary>
        /// This method must be called only from UI.RemoveExclusive(). Do not call this directly
        /// </summary>
        public void RemoveFromExclusiveList(UI ui)
        {
            int existingExclusiveDataIndex = mExclusiveList.FindIndex(x => x._UI == ui);
            if (existingExclusiveDataIndex != -1)
            {
                ExclusiveData exclusiveData = mExclusiveList[existingExclusiveDataIndex];
                Canvas uiCanvas = ui.GetComponent<Canvas>();
                uiCanvas.overrideSorting = exclusiveData._PreviousOverrideSorting;
                uiCanvas.sortingOrder = exclusiveData._PreviousSortingOrder;
                mExclusiveList.RemoveAt(existingExclusiveDataIndex);
                UI._GlobalExclusiveUI = mExclusiveList.Count > 0 ? mExclusiveList[mExclusiveList.Count - 1]._UI : null;
            }
            else
                Debug.LogWarning("Trying to remove UI that is not already present from the exclusive list: " + ui.name);
        }
    }
}