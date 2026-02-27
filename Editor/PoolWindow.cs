using System.Linq;
using UnityEditor;
using UnityEngine;
using Vector.DI.Pool;

namespace Editor
{
    public class PoolWindow : EditorWindow
    {
        [MenuItem("DI/Pool manager")]
        static void Init()
        {
            var window = (PoolWindow)GetWindow(typeof(PoolWindow));
            window.Show();
        }
        
        Vector2 scrollPos;

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button($"Prefab name", GUILayout.Height(25), GUILayout.Width(333)))
            {
                
            }
            if(GUILayout.Button($"Count", GUILayout.Height(25), GUILayout.Width(600)))
            {
                
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            var names = MonoPoolManager.Pools
                .ToDictionary(pair => pair.Key.Split("/").Last(), pair => pair.Value.Count)
                .OrderByDescending(pair => pair.Value);
            foreach (var keyPool in names)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel($"{keyPool.Key} ({keyPool.Value})", GUILayout.Height(25), GUILayout.Width(333));
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}