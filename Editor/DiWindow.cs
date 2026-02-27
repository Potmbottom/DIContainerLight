using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Vector.DI;

namespace Editor
{
    public class DiWindow : EditorWindow
    {
        private enum SortingType
        {
            Binding,
            Transient,
            Dispose
        }

        private static List<DiContext> _stack;
        static bool[] Array;
        static GUIStyle _style;
        static GUIStyle _style2;
        
        Vector2 scrollPos;
        private SortingType _sorting;
        

        [MenuItem("DI/Debug manager")]
        static void Init()
        {
            UpdateTable();
            _style = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15
            };
            
            _style2 = new GUIStyle();
            _style2.normal.textColor = Color.grey;
            _style2.normal.background = Texture2D.whiteTexture;
            _style2.clipping = TextClipping.Clip;
            _style.normal.textColor = Color.cyan;
            var window = (DiWindow)GetWindow(typeof(DiWindow));
            window.Show();
        }

        private static void UpdateTable()
        {
            _stack = StaticDi.DiContainer.Stack.ToList();
            Array = new bool[_stack.Count];
        }

        void OnGUI()
        {
            if(StaticDi.DiContainer == null) return;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"", GUILayout.Width(250));
            GUILayout.Label($"+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_");
            GUILayout.EndHorizontal();
            GUILayout.Label($"Stack. Last on top.");
            GUILayout.Space(33);
            if (GUILayout.Button("UPDATE"))
            {
                UpdateTable();
            }
            
            GUILayout.BeginHorizontal();
            if(GUILayout.Button($"Binding type", GUILayout.Height(25), GUILayout.Width(333)))
            {
                _sorting = SortingType.Binding;
            }
            if(GUILayout.Button($"Transient binding", GUILayout.Height(25), GUILayout.Width(600)))
            {
                _sorting = SortingType.Transient;
            }
            if(GUILayout.Button($"Disposable", GUILayout.Height(25), GUILayout.Width(100)))
            {
                _sorting = SortingType.Dispose;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            for (var i = 0; i < _stack.Count; i++)
            {
                var context = _stack[i];
                Array[i] = EditorGUILayout.Foldout(Array[i], "Context " + (_stack.Count - i) + $"({context.Bindings.Count})",
                    toggleOnLabelClick: true, _style);
                if (Array[i])
                {
                    foreach (var item in GetSorted(context.Bindings))
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.SelectableLabel($"{CreateName(item.BindingMeta.BindingType)}",_style2,
                            GUILayout.Height(25), GUILayout.Width(333));
                        EditorGUILayout.SelectableLabel($"{CreateTransientBindingStr(item)}", _style2,GUILayout.Height(25), GUILayout.Width(600));
                        GUILayout.Label($"{item.BindingMeta.Linked.Contains(typeof(IDisposable))}",_style2, GUILayout.Height(25), GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                    }   
                }
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private List<BindingModel> GetSorted(List<BindingModel> data)
        {
            switch (_sorting)
            {
                case SortingType.Binding:
                    return data.OrderBy(model => model.BindingMeta.BindingType.Name).ToList();
                case SortingType.Transient:
                    return data.OrderByDescending(model => model.BindingMeta.Transient).ToList();
                case SortingType.Dispose:
                    return data.OrderByDescending(model => model.BindingMeta.Linked.Contains(typeof(IDisposable))).ToList();
            }

            return null;
        }

        private static string CreateTransientBindingStr(BindingModel model)
        {
            if (!model.BindingMeta.Transient) return "-";
            var result = new StringBuilder();
            var value = StaticDi.DiContainer.LinkMap.
                Where(pair => pair.Value.Any(o => o.GetType() == model.BindingMeta.BindingType)).ToList();
            for (var i = 0; i < value.Count; i++)
            {
                var item = value[i];
                result.Append(item.Key.GetType().Name);
                if (i + 1 > value.Count)
                {
                    result.Append(",");
                }
            }
            return result.ToString();
        }

        private static string CreateName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var genericType = type.GetGenericArguments();
            var result = new StringBuilder();
            result.Append(type.Name.TrimEnd('1', '2', '3', '`'));
            result.Append("<");
            for (var i = 0; i < genericType.Length; i++)
            {
                var genType = genericType[i];
                result.Append(CreateName(genType));
                if (i + 1 > genericType.Length)
                {
                    result.Append(",");
                }
            }
            result.Append(">");
            return result.ToString();
        }
    }
    
}