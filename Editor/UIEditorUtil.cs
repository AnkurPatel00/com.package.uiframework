using UnityEditor;
using UnityEngine;

namespace UIFrameWork.Editor
{
	public class UIEditorUtil
	{
		private static string TEMPLATE_PATH = "Packages/com.package.uiframework/Runtime/Prefabs/";
		private static string PREFAB_FILE_EXTENSION = ".prefab";
		private static string UI_MANAGER_PREFAB = "PfUIManagerTemplate";
		private static string UI_MANAGER_NAME = "UIManager";
		private static string UI_PREFAB = "PfUITemplate";
		private static string UI_NAME = "UI";
		private static string WIDGET_PREFAB = "PfUIWidgetTemplate";
		private static string WIDGET_NAME = "Widget";
		private static string LABEL_PREFAB = "PfUILabelTemplate";
		private static string LABEL_NAME = "Label";
		private static string BUTTON_PREFAB = "PfUIButtonTemplate";
		private static string BUTTON_NAME = "Button";
		private static string EDIT_BOX_PREFAB = "PfUIEditBoxTemplate";
		private static string EDIT_BOX_NAME = "EditBox";
		private static string PROGRESS_BAR_PREFAB = "PfUIProgressBarTemplate";
		private static string PROGRESS_BAR_NAME = "ProgressBar";
		private static string TOGGLE_BUTTON_PREFAB = "PfUIToggleButtonTemplate";
		private static string TOGGLE_BUTTON_NAME = "ToggleButton";
		private static string MENU_PREFAB = "PfUIMenuTemplate";
		private static string MENU_NAME = "Menu";
		private static string DROP_DOWN_PREFAB = "PfUIDropDownTemplate";
		private static string DROP_DOWN_NAME = "DropDown";

		[MenuItem("GameObject/UIFramework/UI Manager", false, 10)]
		public static void CreateUIManager(MenuCommand menuCommand)
		{
			GameObjectCreationCommon(null, UI_MANAGER_PREFAB, UI_MANAGER_NAME);
		}

		[MenuItem("GameObject/UIFramework/UI", false, 10)]
		public static GameObject CreateUI(MenuCommand menuCommand)
		{
			if (Object.FindObjectOfType<UIManager>() == null)
				CreateUIManager(menuCommand);

			return GameObjectCreationCommon(menuCommand.context as GameObject, UI_PREFAB, UI_NAME);
		}

		[MenuItem("GameObject/UIFramework/Widget", false, 10)]
		public static GameObject CreateWidget(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, WIDGET_PREFAB, WIDGET_NAME);
		}

		[MenuItem("GameObject/UIFramework/Label", false, 10)]
		public static GameObject CreateLabel(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, LABEL_PREFAB, LABEL_NAME);
		}

		[MenuItem("GameObject/UIFramework/Button", false, 10)]
		public static GameObject CreateButton(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, BUTTON_PREFAB, BUTTON_NAME);
		}

		[MenuItem("GameObject/UIFramework/EditBox", false, 10)]
		public static GameObject CreateEditBox(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, EDIT_BOX_PREFAB, EDIT_BOX_NAME);
		}

		[MenuItem("GameObject/UIFramework/Progress Bar", false, 10)]
		public static void CreateProgressBar(MenuCommand menuCommand)
		{
			GameObjectCreationCommon(menuCommand.context as GameObject, PROGRESS_BAR_PREFAB, PROGRESS_BAR_NAME);
		}

		[MenuItem("GameObject/UIFramework/Toggle Button", false, 10)]
		public static GameObject CreateToggleButton(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, TOGGLE_BUTTON_PREFAB, TOGGLE_BUTTON_NAME);
		}

		[MenuItem("GameObject/UIFramework/Menu", false, 10)]
		public static GameObject CreateMenu(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, MENU_PREFAB, MENU_NAME);
		}

		[MenuItem("GameObject/UIFramework/Drop Down", false, 10)]
		public static GameObject CreateDropDown(MenuCommand menuCommand)
		{
			return GameObjectCreationCommon(menuCommand.context as GameObject, DROP_DOWN_PREFAB, DROP_DOWN_NAME);
		}

		private static GameObject GameObjectCreationCommon(GameObject parentObj, string prefabName, string name)
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TEMPLATE_PATH + prefabName + PREFAB_FILE_EXTENSION);
			Transform parentTransform = (parentObj != null) ? parentObj.transform : null;
			GameObject gameObj = GameObject.Instantiate(prefab, parentTransform);
			gameObj.name = name;
			GameObjectUtility.SetParentAndAlign(gameObj, parentObj);
			Undo.RegisterCreatedObjectUndo(gameObj, "Create " + gameObj.name);
			return gameObj;
		}
	}
}
