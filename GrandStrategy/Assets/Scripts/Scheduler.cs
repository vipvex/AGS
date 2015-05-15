using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scheduler : MonoBehaviour
{

    Queue<Task> tasks;
    Stack<Task> repeatingTasksToAdd;

    static Scheduler s_instance;

    public delegate void TaskDelegate();

    Dictionary<TaskDelegate, TaskData> taskTimes;

    [System.Serializable]
    public class TaskTemp
    {
        public TaskTemp(TaskDelegate t, TaskData p_data)
        {
            task = t.Method.Name; data = p_data;
        }
        public TaskTemp() { }
        public string task;
        public TaskData data;
    }

    public List<TaskTemp> taskListTemp;

    [System.Serializable]
    public class TaskData
    {
        public TaskData(float p_time)
        {
            time = p_time;
            numTimes = 0;
        }
        public float time;
        public int numTimes;
    }

    const float MAX_FRAME_TIME = 1f / 240f;

    const int RUNS_TILL_SAMPLE = 10;

    public bool run = false;

    class Task
    {
        public Task(TaskDelegate p_myDelegate, bool p_repeating)
        {
            myDelegate = p_myDelegate;
            repeating = p_repeating;
        }
        public Task() { }
        public TaskDelegate myDelegate;
        public bool repeating;
    }

    public static void AddTask(TaskDelegate task, bool repeating)
    {
        instance.tasks.Enqueue(new Task(task, repeating));
    }

    void Awake()
    {
        s_instance = this;
        tasks = new Queue<Task>();
        repeatingTasksToAdd = new Stack<Task>();
        taskTimes = new Dictionary<TaskDelegate, TaskData>();
    }

    void Update()
    {
        while (repeatingTasksToAdd.Count > 0)
            tasks.Enqueue(repeatingTasksToAdd.Pop());

        if (!run)
            return;
        if (tasks.Count > 0)
        {
            int tasksDone = 0;						//Debug
            Task task = tasks.Dequeue();
            if (taskTimes.ContainsKey(task.myDelegate))
            {
                taskTimes[task.myDelegate].numTimes++;
                if (taskTimes[task.myDelegate].numTimes > RUNS_TILL_SAMPLE)
                {
                    TimeTask(task);
                    taskTimes[task.myDelegate].numTimes = 1;
                }
                else
                {
                    DoTask(task);
                }
                float curTasktime = taskTimes[task.myDelegate].time;
                tasksDone++;						//Debug

                bool moreTasks = true;
                while (moreTasks)
                {
                    if (tasks.Count == 0)
                    {
                        moreTasks = false;
                        break;
                    }
                    if (taskTimes.ContainsKey(tasks.Peek().myDelegate))
                    {
                        float addTime = taskTimes[tasks.Peek().myDelegate].time;
                        if (curTasktime + addTime < MAX_FRAME_TIME)
                        {
                            task = tasks.Dequeue();
                            DoTask(task);
                            tasksDone++;			//Debug
                            curTasktime += taskTimes[task.myDelegate].time;
                            if (curTasktime > MAX_FRAME_TIME)
                            {
                                moreTasks = false;
                            }
                        }
                        else
                        {
                            moreTasks = false;
                        }
                    }
                    else
                    {
                        moreTasks = false;
                    }
                }
            }
            else
            {
                TimeTask(task);
                tasksDone++;						//Debug
            }
            print("Tasks done: " + tasksDone);		//Debug
        }
    }

    void TimeTask(Task task)
    {
        float curTime = Time.realtimeSinceStartup;
        DoTask(task);
        float taskTime = Time.realtimeSinceStartup - curTime;

        if (taskTimes.ContainsKey(task.myDelegate))
        {
            taskTimes[task.myDelegate].time = (taskTimes[task.myDelegate].time + taskTime) / 2f;
            //			taskListTemp[taskListTemp.BinarySearch(new TaskTemp(task.myDelegate, taskTimes[task.myDelegate]))].data = taskTimes[task.myDelegate];
        }
        else
        {
            taskTimes.Add(task.myDelegate, new TaskData(taskTime));
            taskListTemp.Add(new TaskTemp(task.myDelegate, taskTimes[task.myDelegate]));
        }
        print("Task " + task.myDelegate.Method.Name + " takes " + (taskTime));
    }

    void DoTask(Task task)
    {
        task.myDelegate();
        if (task.repeating)
            repeatingTasksToAdd.Push(task);
        //			repeatingTasksToAdd.Enqueue(task);
    }

    public static Scheduler instance
    {
        get
        {
            return s_instance;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        foreach (Task task in tasks)
        {
            GUILayout.Label(task.myDelegate.Method.Name);
        }
        GUILayout.EndVertical();
    }
}