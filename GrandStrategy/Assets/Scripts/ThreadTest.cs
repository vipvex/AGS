using UnityEngine;
using System.Threading;
using System.Collections;

public class ThreadTest : MonoBehaviour {


    Poop[] poop;

    class Poop
    {

        public int[] arr;

    }

	// Use this for initialization
	void Start () {
        print("Processor count: " + SystemInfo.processorCount);

        //new Thread(PrintNum).Start();
        //new Thread(PrintString).Start();

        //poop = new Poop[2];
        //poop[0] = new Poop();
        //poop[0].arr = new int[1000];
        //
        //new Thread(() =>
        //{
        //
        //    Test(9001);
        //
        //}).Start();

	}

    void Test(int a)
    {
        //Debug.Log("Doing stuff " + a);

        for (int i = 0; i < poop[0].arr.Length; i++)
        {
            Debug.Log("Testing " + poop[0].arr[i]);
            
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void PrintNum()
    {
        for (int i = 0; i < 10000; i++)
        {
            Debug.Log("Print num");
            //Thread.Sleep(0);
        }
        Debug.Log("Done numbers!"); 
    }

    void PrintString()
    {
        for (int i = 0; i < 10000; i++)
        {
            Debug.Log("Print string");
            //Thread.Sleep(0);
        }
        Debug.Log("Done strings!"); 
    }

}
