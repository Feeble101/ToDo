using UnityEngine;
using System.Collections.Generic;

public class TodoList : ScriptableObject
{
	[System.Serializable]
	public class Todo
	{
        [Tooltip("ToDo List Item Name.")]
        public string itemName;

        [Tooltip("ToDo List Item Details/Notes.")]
        public string text;

        [Tooltip("ToDo List is completed?")]
        public bool completed;
	}

    [Tooltip("Name of specific ToDo List.")]
    public string name; // hiding object.name because it sometimes reverts the list name to the object name

    [Tooltip("ToDo List Items.")]
    public List<Todo> todos = new List<Todo>(0);

	public void CopyTo(TodoList l)
	{
		foreach (Todo t in todos)
		{
			l.todos.Add(new Todo()
			{
                itemName = t.itemName,
                text = t.text,
                completed = t.completed
			});
		}
	}
}