using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cowart.FlowFramework.Editor
{
    /// <summary>提供 Flow 系统的场景创建入口。</summary>
    public static class FlowHierarchyCreator
    {
        private const string MenuPath = "GameObject/Cowart/框架/创建 Flow 系统";

        [MenuItem(MenuPath, false, -10000)]
        private static void Create(MenuCommand menuCommand)
        {
            Scene scene = GetTargetScene(menuCommand.context as GameObject);
            if (!scene.IsValid() || !scene.isLoaded || HasFlowManager(scene))
            {
                return;
            }

            // FlowManager 使用 DontDestroyOnLoad，必须保持在场景根节点。
            GameObject root = new GameObject("Flow", typeof(FlowManager));
            SceneManager.MoveGameObjectToScene(root, scene);
            GameObjectUtility.EnsureUniqueNameForSibling(root);
            Undo.RegisterCreatedObjectUndo(root, "Create Flow System");
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateCreate()
        {
            Scene scene = GetTargetScene(Selection.activeGameObject);
            return scene.IsValid() && scene.isLoaded && !HasFlowManager(scene);
        }

        private static Scene GetTargetScene(GameObject context)
        {
            return context != null && context.scene.IsValid()
                ? context.scene
                : SceneManager.GetActiveScene();
        }

        private static bool HasFlowManager(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<FlowManager>(true) != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
