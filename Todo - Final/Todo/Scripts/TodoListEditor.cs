using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class TodoListEditor : EditorWindow 
{
    #region file stuff
    public const string DB_FILE = "Assets/Todo/Todo Data/Database.prefab";
    public const string DB_DEF_TODO_FILE = "Assets/Todo/Todo Data/Todo.asset";

    public static string fullPath
    {
        get
        {
            if (pPath == null)
            {
                pPath = Application.dataPath;
                pPath = pPath.Substring(0, pPath.LastIndexOf("Assets"));
            }
            return pPath;
        }
    }
    private static string pPath;

    private static Database _db = null;
    #endregion

    #region gui stuff
    public static float LeftPanelWidth = 200f;
	private int currDbArea = 0;

    private Vector2 scroll1, scroll2;

    private TodoList curr = null;   // currently being edited
    private TodoList del = null;	// helper when deleting

    private Vector2[] textScroll = new Vector2[0];
    #endregion

    #region menu
    [MenuItem("ToDo/To Do List", false, 1)]
    public static void OpenTodoListEditor()
    {
        if (!File.Exists(fullPath + "/" + DB_FILE))
        {
            // delete the database prefab
            if (Directory.Exists(fullPath + "/" + DB_FILE))
            {
                Debug.Log("Deleting all assets in: " + DB_FILE);
                AssetDatabase.DeleteAsset(DB_FILE.Substring(0, DB_FILE.LastIndexOf('/')));
            }

            AssetDatabase.Refresh();

            // create database
            GameObject go = new GameObject("Database");
            go.AddComponent<Database>();
            GameObject dbPrefab = PrefabUtility.SaveAsPrefabAsset(go, DB_FILE);

            DestroyImmediate(go);

            Database db = dbPrefab.GetComponent<Database>();
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }
        else
        {
            _db = AssetDatabase.LoadAssetAtPath(DB_FILE, typeof(Database)) as Database;
        }

        TodoListEditor ed = GetWindow<TodoListEditor>();
        ed.titleContent = new GUIContent("Todo List");
        ed.minSize = new Vector2(800, 600);
        ed.Show();
    }
    #endregion

    GUIStyle comStyle = new GUIStyle(GUIStyle.none);
	void OnGUI()
	{
   		EditorStyles.textField.wordWrap = true;
        EditorStyles.textField.richText = true;

        comStyle.normal.textColor = Color.yellow;

        EditorGUILayout.BeginHorizontal();
        {
            TodoLists();
            TodoDetails();
        }
        EditorGUILayout.EndHorizontal();
    }

    #region panels
    private string focusName;
    private void TodoLists()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));
        GUILayout.Space(5);
        // -------------------------------------------------------------

        // the add button
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Add Todo List"), EditorStyles.miniButtonLeft))
            {
                //create todo asset if there is none
                GUI.FocusControl("");
                curr = CreateInstance<TodoList>();
                _db.todoList.Add(curr);

                if (!File.Exists(fullPath + "/" + DB_DEF_TODO_FILE))
                {
                    AssetDatabase.CreateAsset(curr, DB_DEF_TODO_FILE);
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(curr, DB_DEF_TODO_FILE);
                }

                AssetDatabase.SaveAssets();
                curr.name = "Todo";
                EditorUtility.SetDirty(curr);
                EditorUtility.SetDirty(_db);
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //left list of todos
        scroll1 = EditorGUILayout.BeginScrollView(scroll1, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Width(LeftPanelWidth));
        {
            if (_db.todoList.Count > 0)
            {
                int moveUp = -1;
                int moveDown = -1;
                int i = 0;
                //Debug.Log(_db.todoList.Count);
                foreach (TodoList todo in _db.todoList)
                {
                    if(todo == null)
                    {
                        _db.todoList.Remove(todo);
                        EditorUtility.SetDirty(_db);
                        AssetDatabase.SaveAssets();
                        GUIUtility.ExitGUI();
                        return;
                    }
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(TodoListEditor.LeftPanelWidth - 40), GUILayout.ExpandWidth(false));
                    {
                        if (GUILayout.Button("\u25B2", EditorStyles.toolbarButton, GUILayout.Width(20)))
                        {
                            moveUp = i;
                        }
                        if (GUILayout.Button("\u25BC", EditorStyles.toolbarButton, GUILayout.Width(20)))
                        {
                            moveDown = i;
                        }

                        if(GUILayout.Toggle(curr == todo, todo.name, EditorStyles.miniButton, GUILayout.Width(120), GUILayout.ExpandWidth(false)) != (curr == todo))
                        {
                            curr = todo;
                            GUI.FocusControl("");
                        }

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            del = todo;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    i++;
                }

                if (moveUp > 0)
                {
                    TodoList tempTodo = _db.todoList[moveUp];
                    _db.todoList.RemoveAt(moveUp);
                    _db.todoList.Insert(moveUp - 1, tempTodo);
                }
                if (moveDown >= 0 && moveDown < _db.todoList.Count - 1)
                {
                    TodoList tempTodo = _db.todoList[moveDown];
                    _db.todoList.RemoveAt(moveDown);
                    _db.todoList.Insert(moveDown + 1, tempTodo);
                }
            }
            else
            {
                GUILayout.Label("No Todo List defined");
            }
        }
        GUILayout.Space(30);
        EditorGUILayout.EndScrollView();
        // -------------------------------------------------------------
        GUILayout.Space(3);
        EditorGUILayout.EndVertical();

        if (del != null)
        {
            if (curr == del) curr = null;
            _db.todoList.Remove(del);
            DestroyImmediate(del, true);
            del = null;
            EditorUtility.SetDirty(_db);
            AssetDatabase.SaveAssets();
        }
    }

    private void TodoDetails()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Space(5);

            GUILayout.Label("To Do Lists:", EditorStyles.boldLabel);
            if (curr == null) { EditorGUILayout.EndVertical(); return; }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(600));
            {
                EditorGUI.BeginChangeCheck();
                curr.name = EditorGUILayout.TextField("Name:", curr.name);

                EditorGUILayout.Space();
                GUI.enabled = true;

                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);

                    if (GUILayout.Button("New Item", EditorStyles.miniButtonLeft, GUILayout.Width(80)))
                    {
                        curr.todos.Add(new TodoList.Todo());

                        EditorUtility.SetDirty(curr);
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                scroll2 = EditorGUILayout.BeginScrollView(scroll2, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Height(350));
                {
                    int delTodo = -1;
                    int comTodo = -1;

                    if (curr.todos.Count != textScroll.Length)
                    {
                        textScroll = new Vector2[curr.todos.Count];
                    }

                    int moveUp = -1;
                    int moveDown = -1;

                    for (int i = 0; i < curr.todos.Count; i++)
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.Width(400), GUILayout.ExpandWidth(false));
                            {
                                if (GUILayout.Button("\u25B2", EditorStyles.toolbarButton, GUILayout.Width(20)))
                                {
                                    moveUp = i;
                                }
                                if (GUILayout.Button("\u25BC", EditorStyles.toolbarButton, GUILayout.Width(20)))
                                {
                                    moveDown = i;
                                }

                                GUI.SetNextControlName(i.ToString());
                                if (curr.todos[i].completed)
                                {
                                    GUILayout.Label("  Completed!  " + StrikeThrough(curr.todos[i].itemName), comStyle, GUILayout.Width(400));
                                }
                                else
                                {
                                    curr.todos[i].itemName = GUILayout.TextField(curr.todos[i].itemName, GUILayout.Width(400));
                                }

                                if (GUILayout.Button("\u2713", EditorStyles.miniButton, GUILayout.Width(20))) comTodo = i;

                                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20))) delTodo = i;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (focusName == i.ToString())
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(40);

                                    textScroll[i] = EditorGUILayout.BeginScrollView(textScroll[i], false, true, GUILayout.Width(500), GUILayout.Height(100), GUILayout.ExpandHeight(true));
                                    {
                                        curr.todos[i].text = EditorGUILayout.TextArea(curr.todos[i].text, GUILayout.Width(500), GUILayout.ExpandHeight(true));
                                    }
                                    GUILayout.EndScrollView();
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            if (GUI.GetNameOfFocusedControl() != "")
                            {
                                if (focusName != GUI.GetNameOfFocusedControl())
                                {
                                    focusName = GUI.GetNameOfFocusedControl();
                                    GUI.FocusControl("");
                                }
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }

                    if (moveUp > 0)
                    {
                        TodoList.Todo tempTodo = curr.todos[moveUp];
                        curr.todos.RemoveAt(moveUp);
                        curr.todos.Insert(moveUp - 1, tempTodo);
                    }
                    if (moveDown >= 0 && moveDown < curr.todos.Count - 1)
                    {
                        TodoList.Todo tempTodo = curr.todos[moveDown];
                        curr.todos.RemoveAt(moveDown);
                        curr.todos.Insert(moveDown + 1, tempTodo);
                    }

                    if (delTodo >= 0)
                    {
                        curr.todos.RemoveAt(delTodo);
                        EditorUtility.SetDirty(curr);
                    }

                    if (comTodo >= 0)
                    {
                        curr.todos[comTodo].completed = !curr.todos[comTodo].completed;
                        TodoList.Todo tempTodo = curr.todos[comTodo];
                        curr.todos.RemoveAt(comTodo);
                        curr.todos.Insert(curr.todos.Count, tempTodo);
                    }
                }
                GUILayout.Space(30);
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
            // check if data changed and should be saved
            if (GUI.changed && curr != null)
            {
                EditorUtility.SetDirty(curr);
            }

            GUILayout.Space(3);
        }
        EditorGUILayout.EndVertical();
    }
    #endregion

    public string StrikeThrough(string s)
    {
        string strikethrough = "";
        if (s.Length > 0)
        { 
            foreach (char c in s)
            {
                strikethrough = strikethrough + c + '\u0336';
            }
        }
        return strikethrough;
    }
}